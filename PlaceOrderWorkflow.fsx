#load "DomainApi.fsx"

module PlaceOrderWorkflow =
    open SimpleTypes
    open DomainApi

    // ----------------------------------------
    // パート1: 設計
    // https://scrapbox.io/radish-miyazaki/%E5%9E%8B%E3%82%92%E4%BD%BF%E3%81%A3%E3%81%9F%E3%83%AF%E3%83%BC%E3%82%AF%E3%83%95%E3%83%AD%E3%83%BC%E3%81%AE%E5%90%84%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%81%AE%E3%83%A2%E3%83%87%E3%83%AA%E3%83%B3%E3%82%B0
    // ----------------------------------------

    // --------------- 注文の検証 ---------------
    type ValidatedOrderLine =
        { OrderLineId: SimpleTypes.OrderLineId
          ProductCode: SimpleTypes.ProductCode
          Quantity: SimpleTypes.OrderQuantity }

    type ValidatedOrder =
        { OrderId: SimpleTypes.OrderId
          CustomerInfo: DomainApi.CustomerInfo
          ShippingAddress: DomainApi.Address
          BillingAddress: DomainApi.Address
          Lines: ValidatedOrderLine list }

    type ValidateOrder =
        DomainApi.CheckProductCodeExists -> DomainApi.CheckAddressExists -> DomainApi.UnvalidatedOrder -> ValidatedOrder

    // --------------- 注文の価格設定 ---------------
    type PriceOrder = DomainApi.GetProductPrice -> ValidatedOrder -> DomainApi.PricedOrder

    // --------------- 注文の確認 ---------------
    type AcknowledgeOrder =
        DomainApi.CreateOrderAcknowledgmentLetter
            -> DomainApi.SendOrderAcknowledgment
            -> DomainApi.PricedOrder
            -> DomainApi.OrderAcknowledgmentSent option

    // --------------- イベントの作成 ---------------
    type CreateEvents =
        DomainApi.PricedOrder -> DomainApi.OrderAcknowledgmentSent option -> DomainApi.PlaceOrderEvent list

    // ----------------------------------------
    // パート2: 実装
    // https://scrapbox.io/radish-miyazaki/%E6%9C%80%E5%88%9D%E3%81%AE%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%81%AE%E5%AE%9F%E8%A3%85
    // https://scrapbox.io/radish-miyazaki/%E6%AE%8B%E3%82%8A%E3%81%AE%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%81%AE%E5%AE%9F%E8%A3%85
    // ----------------------------------------

    let predicateToPassthru errorMsg f x = if f x then x else failwith errorMsg

    let toCustomerInfo (customer: DomainApi.UnvalidatedCustomerInfo) =
        let firstName = customer.FirstName |> SimpleTypes.String50.create
        let lastName = customer.LastName |> SimpleTypes.String50.create
        let emailAddress = customer.EmailAddress |> SimpleTypes.EmailAddress.create

        let name: DomainApi.PersonalName =
            { FirstName = firstName
              LastName = lastName }

        let customerInfo: DomainApi.CustomerInfo =
            { Name = name
              EmailAddress = emailAddress }

        customerInfo

    let toAddress (checkAddressExists: DomainApi.CheckAddressExists) unvalidatedAddress =
        let checkedAddress = checkAddressExists unvalidatedAddress
        let (DomainApi.CheckedAddress checkedAddress) = checkedAddress

        let addressLine1 = checkedAddress.AddressLine1 |> SimpleTypes.String50.create
        let addressLine2 = checkedAddress.AddressLine2 |> SimpleTypes.String50.createOption
        let addressLine3 = checkedAddress.AddressLine3 |> SimpleTypes.String50.createOption
        let addressLine4 = checkedAddress.AddressLine4 |> SimpleTypes.String50.createOption
        let city = checkedAddress.City |> SimpleTypes.String50.create
        let zipCode = checkedAddress.ZipCode |> SimpleTypes.ZipCode.create

        let address: DomainApi.Address =
            { AddressLine1 = addressLine1
              AddressLine2 = addressLine2
              AddressLine3 = addressLine3
              AddressLine4 = addressLine4
              City = city
              ZipCode = zipCode }

        address

    let toProductCode (checkProductCodeExists: DomainApi.CheckProductCodeExists) productCode =
        let checkProduct =
            let errorMsg = sprintf "Invalid: %A" productCode
            predicateToPassthru errorMsg checkProductCodeExists

        productCode |> SimpleTypes.ProductCode.create |> checkProduct

    let toOrderQuantity productCode quantity =
        match productCode with
        | SimpleTypes.Widget _ ->
            quantity
            |> int
            |> SimpleTypes.UnitQuantity.create
            |> SimpleTypes.OrderQuantity.Unit
        | SimpleTypes.Gizmo _ ->
            quantity
            |> SimpleTypes.KilogramQuantity.create
            |> SimpleTypes.OrderQuantity.Kilogram

    let toValidatedOrderLine checkProductCodeExists (unvalidatedOrder: DomainApi.UnvalidatedOrderLine) =
        let orderLineId = unvalidatedOrder.OrderLineId |> SimpleTypes.OrderLineId.create

        let productCode =
            unvalidatedOrder.ProductCode |> toProductCode checkProductCodeExists

        let quantity = unvalidatedOrder.Quantity |> toOrderQuantity productCode

        let validatedOrderLine =
            { OrderLineId = orderLineId
              ProductCode = productCode
              Quantity = quantity }

        validatedOrderLine

    let listOfOption opt =
        match opt with
        | Some x -> [ x ]
        | None -> []

    let toPricedOrderLine getProductPrice (line: ValidatedOrderLine) =
        let qty = line.Quantity |> SimpleTypes.OrderQuantity.value
        let price = line.ProductCode |> getProductPrice
        let linePrice = price |> SimpleTypes.Price.multiply qty

        let pricedOrderLine: DomainApi.PricedOrderLine =
            { OrderLineId = line.OrderLineId
              ProductCode = line.ProductCode
              Quantity = line.Quantity
              LinePrice = linePrice }

        pricedOrderLine

    let createBillingEvent (pricedOrder: DomainApi.PricedOrder) : DomainApi.BillableOrderPlaced option =
        let billingAmount = pricedOrder.AmountToBill |> SimpleTypes.BillingAmount.value

        if billingAmount > 0.0 then
            let order: DomainApi.BillableOrderPlaced =
                { OrderId = pricedOrder.OrderId
                  BillingAddress = pricedOrder.BillingAddress
                  AmountToBill = pricedOrder.AmountToBill }

            Some order
        else
            None


    // --------------- 注文の検証 ---------------
    let validateOrder: ValidateOrder =
        fun checkProductCodeExists checkAddressExists unvalidatedOrder ->
            let orderId = unvalidatedOrder.OrderId |> SimpleTypes.OrderId.create
            let customerInfo = toCustomerInfo unvalidatedOrder.CustomerInfo

            let shippingAddress: DomainApi.Address =
                toAddress checkAddressExists unvalidatedOrder.ShippingAddress

            let billingAddress: DomainApi.Address =
                toAddress checkAddressExists unvalidatedOrder.BillingAddress

            let lines =
                unvalidatedOrder.Lines |> List.map (toValidatedOrderLine checkProductCodeExists)

            let validatedOrder: ValidatedOrder =
                { OrderId = orderId
                  CustomerInfo = customerInfo
                  ShippingAddress = shippingAddress
                  BillingAddress = billingAddress
                  Lines = lines }

            validatedOrder

    // --------------- 注文の価格設定 ---------------
    let priceOrder: PriceOrder =
        fun getProductPrice validatedOrder ->
            let lines = validatedOrder.Lines |> List.map (toPricedOrderLine getProductPrice)

            let amountToBill =
                lines
                |> List.map (fun line -> line.LinePrice)
                |> SimpleTypes.BillingAmount.sumPrices

            let pricedOrder: DomainApi.PricedOrder =
                { OrderId = validatedOrder.OrderId
                  CustomerInfo = validatedOrder.CustomerInfo
                  ShippingAddress = validatedOrder.ShippingAddress
                  BillingAddress = validatedOrder.BillingAddress
                  AmountToBill = amountToBill
                  Lines = lines }

            pricedOrder

    // --------------- 注文の確認 ---------------
    let acknowledgeOrder: AcknowledgeOrder =
        fun createOrderAcknowledgmentLetter sendOrderAcknowledgment pricedOrder ->
            let letter = createOrderAcknowledgmentLetter pricedOrder

            let acknowledgment: DomainApi.OrderAcknowledgment =
                { EmailAddress = pricedOrder.CustomerInfo.EmailAddress
                  Letter = letter }

            match sendOrderAcknowledgment acknowledgment with
            | DomainApi.Sent ->
                let event: DomainApi.OrderAcknowledgmentSent =
                    { OrderId = pricedOrder.OrderId
                      EmailAddress = pricedOrder.CustomerInfo.EmailAddress }

                Some event
            | DomainApi.NotSent -> None

    // --------------- イベントの作成 ---------------
    let createEvents: CreateEvents =
        fun pricedOrder acknowledgmentEventOpt ->
            let events1 = pricedOrder |> DomainApi.PlaceOrderEvent.OrderPlaced |> List.singleton

            let events2 =
                acknowledgmentEventOpt
                |> Option.map DomainApi.PlaceOrderEvent.AcknowledgmentSent
                |> listOfOption

            let events3 =
                pricedOrder
                |> createBillingEvent
                |> Option.map DomainApi.BillableOrderPlaced
                |> listOfOption

            [ yield! events1; yield! events2; yield! events3 ]

    // ----------------------------------------
    // パート3: ワークフローの全体像
    // https://scrapbox.io/radish-miyazaki/%E3%83%91%E3%82%A4%E3%83%97%E3%83%A9%E3%82%A4%E3%83%B3%E3%81%AE%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%82%92_1_%E3%81%A4%E3%81%AB%E5%90%88%E6%88%90%E3%81%99%E3%82%8B
    // ----------------------------------------
    let placeOrder
        checkProductCodeExists
        checkAddressExists
        getProductPrice
        createOrderAcknowledgmentLetter
        sendOrderAcknowledgment
        =
        fun unvalidatedOrder ->
            let validatedOrder =
                unvalidatedOrder |> validateOrder checkProductCodeExists checkAddressExists

            let pricedOrder = validatedOrder |> priceOrder getProductPrice

            let acknowledgment =
                pricedOrder
                |> acknowledgeOrder createOrderAcknowledgmentLetter sendOrderAcknowledgment

            let events = createEvents pricedOrder acknowledgment

            events
