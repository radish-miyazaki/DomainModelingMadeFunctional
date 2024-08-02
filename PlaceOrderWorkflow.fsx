#load "Common.fsx"
#load "DomainApi.fsx"

open Common

module PlaceOrderWorkflow =
    open SimpleTypes
    open DomainApi

    // ----------------------------------------
    // パート1: 設計
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
    type CreateEvents = DomainApi.PricedOrder -> DomainApi.OrderAcknowledgmentSent -> DomainApi.PlacedOrderEvent list

    // ----------------------------------------
    // パート2: 実装
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

    // ----------------------------------------
    // パート2: ワークフローの全体像
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

            let pricedOrder = validatedOrder |> getOrder getProductPrice

            let acknowledgment =
                pricedOrder
                |> acknowledgeOrder createOrderAcknowledgmentLetter sendOrderAcknowledgment

            let events = acknowledgment |> createEvents

            events
