#load "SimpleTypes.fsx"

// ----------------------------------------
// 呼び出し元に公開されるパブリックな型
// https://scrapbox.io/radish-miyazaki/Modeling_Workflows_as_Pipelines#66a3222575d04f0000bcae06
// ----------------------------------------
module DomainApi =
    open System
    open SimpleTypes

    // ----------------------------------------
    // 入力データ
    // ----------------------------------------
    type UnvalidatedCustomerInfo =
        { FirstName: string
          LastName: string
          EmailAddress: string }

    type UnvalidatedAddress =
        { AddressLine1: string
          AddressLine2: string
          AddressLine3: string
          AddressLine4: string
          City: string
          ZipCode: string }

    type UnvalidatedOrderLine =
        { OrderLineId: string
          ProductCode: string
          Quantity: float }

    type UnvalidatedOrder =
        { OrderId: string
          CustomerInfo: UnvalidatedCustomerInfo
          ShippingAddress: UnvalidatedAddress
          BillingAddress: UnvalidatedAddress
          Lines: UnvalidatedOrderLine list }

    type Address =
        { AddressLine1: SimpleTypes.String50
          AddressLine2: SimpleTypes.String50 option
          AddressLine3: SimpleTypes.String50 option
          AddressLine4: SimpleTypes.String50 option
          City: SimpleTypes.String50
          ZipCode: SimpleTypes.ZipCode }

    type PersonalName =
        { FirstName: SimpleTypes.String50
          LastName: SimpleTypes.String50 }

    type CustomerInfo =
        { Name: PersonalName
          EmailAddress: SimpleTypes.EmailAddress }

    type PricedOrderLine =
        { OrderLineId: SimpleTypes.OrderLineId
          ProductCode: SimpleTypes.ProductCode
          Quantity: SimpleTypes.OrderQuantity
          LinePrice: SimpleTypes.Price }

    type PricedOrder =
        { OrderId: SimpleTypes.OrderId
          CustomerInfo: CustomerInfo
          ShippingAddress: Address
          BillingAddress: Address
          AmountToBill: SimpleTypes.BillingAmount
          Lines: PricedOrderLine list }

    type OrderPlaced = PricedOrder

    type BillableOrderPlaced =
        { OrderId: SimpleTypes.OrderId
          BillingAddress: Address
          AmountToBill: SimpleTypes.BillingAmount }

    type OrderAcknowledgmentSent =
        { OrderId: SimpleTypes.OrderId
          EmailAddress: SimpleTypes.EmailAddress }

    type CheckedAddress = CheckedAddress of UnvalidatedAddress

    type OrderAcknowledgment =
        { EmailAddress: SimpleTypes.EmailAddress
          Letter: SimpleTypes.HtmlString }

    type SendResult =
        | Sent
        | NotSent

    // --------------- 依存関係 ---------------
    type CheckProductCodeExists = SimpleTypes.ProductCode -> bool
    type CheckAddressExists = UnvalidatedAddress -> CheckedAddress
    type GetProductPrice = SimpleTypes.ProductCode -> SimpleTypes.Price
    type CreateOrderAcknowledgmentLetter = PricedOrder -> SimpleTypes.HtmlString
    type SendOrderAcknowledgment = OrderAcknowledgment -> SendResult

    // ----------------------------------------
    // 入力コマンド
    // ----------------------------------------
    type Command<'data> =
        { Data: 'data
          TimeStamp: DateTime
          UserId: string }

    type PlacedOrderCommand = Command<UnvalidatedOrder>

    // ----------------------------------------
    // 出力データ
    // ----------------------------------------
    type PlaceOrderEvent =
        | OrderPlaced of OrderPlaced
        | BillableOrderPlaced of BillableOrderPlaced
        | AcknowledgmentSent of OrderAcknowledgmentSent

    // ----------------------------------------
    // パブリック API
    // ----------------------------------------
    type PlaceOrderWorkflow =
        CheckProductCodeExists
            -> CheckAddressExists
            -> GetProductPrice
            -> CreateOrderAcknowledgmentLetter
            -> SendOrderAcknowledgment
            -> UnvalidatedOrder
            -> PlaceOrderEvent list
