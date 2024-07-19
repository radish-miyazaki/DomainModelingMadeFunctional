#load "Common.fsx"
open Common

module PrimitiveValues =
    type CustomerId = CustomerId of int

    type WidgetCode = WidgetCode of string
    type UnitQuantity = UnitQuantity of int
    type KilogramQuantity = KilogramQuantity of float

module WorkingWithScus =
    type CustomerId = CustomerId of int
    type OrderId = OrderId of int

    let customerId = CustomerId 42
    let orderId = OrderId 42

    let (CustomerId innerValue) = customerId

    printfn "%i" innerValue

    let processCustomerId (CustomerId innerValue) = printfn "innerValue is %i" innerValue

module ScuPerformance =
    // type UnitQuantity = int

    [<Struct>]
    type UnitQuantity = UnitQuantity of int

    type UnitQuantities = UnitQuantities of int list

module ModelingWithRecords =
    type CustomerInfo = Undefined
    type ShippingAddress = Undefined
    type BillingAddress = Undefined
    type OrderLine = Undefined
    type Price = Undefined

    type Order =
        { CustomerInfo: CustomerInfo
          ShippingAddress: ShippingAddress
          BillingAddress: BillingAddress
          OrderLines: OrderLine list
          AmountToBill: Undefined }

module ModelingWithChoice =
    type WidgetCode = PrimitiveValues.WidgetCode
    type GizmoCode = Undefined
    type UnitQuantity = PrimitiveValues.UnitQuantity
    type KilogramQuantity = PrimitiveValues.KilogramQuantity

    type ProductCode =
        | Widget of WidgetCode
        | Gizmo of GizmoCode

    type OrderQuantity =
        | Unit of UnitQuantity
        | Kilogram of KilogramQuantity

module ModelingWithFunctions =
    type UnvalidatedOrder = Undefined
    type ValidatedOrder = Undefined

    type ValidateOrder = UnvalidatedOrder -> ValidatedOrder

    type AcknowledgmentSend = Undefined
    type OrderPlaced = Undefined
    type BillabledOrderPlaced = Undefined

    type PlaceOrderEvents =
        { AcknowledgmentSend: AcknowledgmentSend
          OrderPlaced: OrderPlaced
          BillabledOrderPlaced: BillabledOrderPlaced }

    type PlaceOrder = UnvalidatedOrder -> PlaceOrderEvents

    type OrderForm = Undefined
    type QuoteForm = Undefined

    type EnvelopeContents = EnvelopeContents of string

    type CategorizedMail =
        | Quote of QuoteForm
        | Order of OrderForm

    type CategorizeInboundMail = EnvelopeContents -> CategorizedMail

    type ProductCatalog = Undefined
    type PlacedOrder = Undefined

    type CalculatePrices = OrderForm -> ProductCatalog -> PlacedOrder

    type CalculatePricesInput =
        { OrderForm: OrderForm
          ProductCatalog: ProductCatalog }

    type CalculatePrices2 = CalculatePricesInput -> PlacedOrder

module DocumentingEffects =
    type UnvalidatedOrder = Undefined
    type ValidatedOrder = Undefined

    // type ValidateOrder = UnvalidatedOrder -> ValidatedOrder
    // type ValidateOrder = UnvalidatedOrder -> Result<ValidatedOrder, ValidationError list>

    // and ValidationError =
    //     { FieldName: string
    //       ErrorDescription: string }

    type ValidationError = Undefined
    // type ValidateOrder = UnvalidatedOrder -> Async<Result<ValidatedOrder, ValidationError list>>

    type ValidationResponse<'a> = Async<Result<'a, ValidationError list>>

    type ValidateOrder = UnvalidatedOrder -> ValidationResponse<ValidatedOrder>

module ValueObjects =
    type WidgetCode = WidgetCode of string

    let widgetCode1 = WidgetCode "W1234"
    let widgetCode2 = WidgetCode "W1234"
    printfn "%b" (widgetCode1 = widgetCode2)

    type PersonalName = { FirstName: string; LastName: string }

    let name1 =
        { FirstName = "Alex"
          LastName = "Adams" }

    let name2 =
        { FirstName = "Alex"
          LastName = "Adams" }

    printfn "%b" (name1 = name2)

    type UsPostalAddress =
        { StreetAddress: string
          City: string
          Zip: string }

    let address1 =
        { StreetAddress = "123 Main St"
          City = "NewYork"
          Zip = "90001" }

    let address2 =
        { StreetAddress = "123 Main St"
          City = "NewYork"
          Zip = "90001" }

    printfn "%b" (address1 = address2)

module Entities =
    type ContactId = ContactId of int

    type Contact =
        { ContactId: ContactId
          PhoneNumber: Undefined
          EmailAddress: Undefined }

    module IdOutside =
        type UnpaidInvoiceInfo = Undefined
        type PaidInvoiceInfo = Undefined

        type InvoiceInfo =
            | Unpaid of UnpaidInvoiceInfo
            | Paid of PaidInvoiceInfo

        type InvoiceId = InvoiceId of string

        type Invoice =
            { InvoiceId: InvoiceId
              InvoiceInfo: InvoiceInfo }

    module IdInside =
        type InvoiceId = InvoiceId of string
        type UnpaidInvoice = { InvoicedId: InvoiceId }
        type PaidInvoice = { InvoicedId: InvoiceId }

        type Invoice =
            | Unpaid of UnpaidInvoice
            | Paid of PaidInvoice

        let invoice = Paid { InvoicedId = InvoiceId "123" }

        match invoice with
        | Unpaid unpaidInvoice -> printfn "The unpaid invoice id is %A" unpaidInvoice.InvoicedId
        | Paid paidInvoice -> printfn "The paid invoice id is %A" paidInvoice.InvoicedId

module Immutability =
    type PersonId = PersonId of int

    type Person = { PersonId: PersonId; Name: string }

    let initialPerson =
        { PersonId = PersonId 42
          Name = "Joseph" }

    let updatedPerson = { initialPerson with Name = "Joe" }

    type Name = string

    type UpdateMutableName = Person -> Name -> unit

    type UpdateImmutableName = Person -> Name -> Person

module EntityEqualityA =
    type ContactId = ContactId of int
    type PhoneNumber = PhoneNumber of string
    type EmailAddress = EmailAddress of string

    [<CustomEquality; NoComparison>]
    type Contact =
        { ContactId: ContactId
          PhoneNumber: PhoneNumber
          EmailAddress: EmailAddress }

        override this.Equals(obj) =
            match obj with
            | :? Contact as c -> this.ContactId = c.ContactId
            | _ -> false

        override this.GetHashCode() = hash this.ContactId

    let contactId = ContactId 1

    let contact1 =
        { ContactId = contactId
          PhoneNumber = PhoneNumber "123-456-789"
          EmailAddress = EmailAddress "bob@example.com" }

    let contact2 =
        { ContactId = contactId
          PhoneNumber = PhoneNumber "123-456-789"
          EmailAddress = EmailAddress "bob@example.com" }

    printfn "%b" (contact1 = contact2)

module EntityEqualityB =
    type ContactId = ContactId of int
    type PhoneNumber = PhoneNumber of string
    type EmailAddress = EmailAddress of string

    [<NoEquality; NoComparison>]
    type Contact =
        { ContactId: ContactId
          PhoneNumber: PhoneNumber
          EmailAddress: EmailAddress }

    let contactId = ContactId 1

    let contact1 =
        { ContactId = contactId
          PhoneNumber = PhoneNumber "123-456-789"
          EmailAddress = EmailAddress "bob@example.com" }

    let contact2 =
        { ContactId = contactId
          PhoneNumber = PhoneNumber "123-456-789"
          EmailAddress = EmailAddress "bob@example.com" }

    // printfn "%b" (contact1 = contact2)

    printfn "%b" (contact1.ContactId = contact2.ContactId)

module EntityEqualityC =
    type OrderId = OrderId of int
    type ProductId = ProductId of int

    [<NoEquality; NoComparison>]
    type OrderLine =
        { OrderId: OrderId
          ProductId: ProductId
          Qty: int }

        member this.Key = (this.OrderId, this.ProductId)

    let line1 =
        { OrderId = OrderId 1
          ProductId = ProductId 42
          Qty = 99 }

    let line2 =
        { OrderId = OrderId 1
          ProductId = ProductId 42
          Qty = 100 }

    // printfn "%b" (line1 = line2)

    printfn "%b" (line1.Key = line2.Key)

module Aggregates =
    type OrderLine = { OrderLineId: int; Price: float }
    type Order = { OrderLines: OrderLine list }

    let findOrderLine orderLineId (lines: OrderLine list) =
        lines |> List.find (fun ol -> ol.OrderLineId = orderLineId)

    let replaceOrderLine orderLineId newOrderLine lines = lines

    let changeOrderLinePrice order orderLineId newPrice =
        let orderLine = order.OrderLines |> findOrderLine orderLineId

        let newOrderLine = { orderLine with Price = newPrice }

        let newOrderLines = order.OrderLines |> replaceOrderLine orderLineId newOrderLine

        let newOrder =
            { order with
                OrderLines = newOrderLines }

        newOrder

module BadAggregateReference =
    type OrderId = Undefined
    type Customer = Undefined
    type OrderLine = Undefined

    type Order =
        { OrderId: OrderId
          Customer: Customer
          OrderLines: OrderLine list }

module AggregateReferences =
    type OrderId = Undefined
    type CustomerId = Undefined
    type OrderLineId = Undefined

    type Order =
        { OrderId: OrderId
          CustomerId: CustomerId
          OrderLineIds: OrderLineId list }

module PuttingItAllTogether =
    type WidgetCode = WidgetCode of string
    type GizmoCode = GizmoCode of string

    type ProductCode =
        | Widget of WidgetCode
        | Gizmo of GizmoCode

    type UnitQuantity = UnitQuantity of int
    type KilogramQuantity = KilogramQuantity of float

    type OrderQuantity =
        | Unit of UnitQuantity
        | Kilos of KilogramQuantity

    type OrderId = Undefined
    type OrderLineId = Undefined
    type CustomerId = Undefined

    type CustomerInfo = Undefined
    type ShippingAddress = Undefined
    type BillingAddress = Undefined
    type Price = Undefined
    type BillingAmount = Undefined

    type Order =
        { Id: OrderId
          CustomerId: CustomerId
          ShippingAddress: ShippingAddress
          BillingAddress: BillingAddress
          OrderLines: OrderLine list
          AmountToBill: BillingAmount }

    and OrderLine =
        { Id: OrderLineId
          OrderId: OrderId
          ProductCode: ProductCode
          OrderQuantity: OrderQuantity
          Price: Price }

    type UnvalidatedOrder =
        { OrderId: string
          CustomerInfo: Undefined
          ShippingAddress: Undefined }

    type PlaceOrderEvents =
        { AcknowledgmentSent: Undefined
          OrderPlaced: Undefined
          BillableOrderPlaced: Undefined }

    type PlaceOrderError =
        | ValidationError of ValidationError list
        | Undefined

    and ValidationError =
        { FieldName: string
          ErrorDescription: string }

    type PlaceOrder = UnvalidatedOrder -> Result<PlaceOrderEvents, PlaceOrderError>
