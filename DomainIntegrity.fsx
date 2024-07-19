#load "Common.fsx"
open Common

module SimpleValues =
    type UnitQuantity = private UnitQuantity of int

    module UnitQuantity =
        let create qty =
            if qty < 1 then
                Error "UnitQuantity can not be negative"
            else if qty > 1000 then
                Error "UnitQuantity can not be more than 1000"
            else
                Ok(UnitQuantity qty)

        let value (UnitQuantity qty) = qty

    let unitQtyResult = UnitQuantity.create 1

    match unitQtyResult with
    | Error msg -> printfn "Failure, Message is %s" msg
    | Ok uQty ->
        printfn "Success. Value is %A" uQty
        let innerValue = UnitQuantity.value uQty
        printfn "innerValue is %i" innerValue

module UnitsOfMeasure =
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    let fiveKilos = 5.0<kg>
    let fiveMeters = 5.0<m>

    // fiveKilos = fiveMeters

    let listOfWeights =
        [ fiveKilos
          // fiveMeters
          ]

    type KilogramQuantity = KilogramQuantity of float<kg>

module Invariants =
    type NonEmptyList<'a> = { First: 'a; Rest: 'a list }

    type OrderLine = Undefined

    type Order = { OrderLines: NonEmptyList<OrderLine> }

module BussinessRuleImplementation =
    type EmailAddress = Undefined

    // type CustomerEmail =
    //     { EmailAddress: EmailAddress
    //       IsVerified: bool }

    // type CustomerEmail =
    //     | Unverified of EmailAddress
    //     | Verified of EmailAddress

    type VerifiedEmailAddress = Undefined

    type CustomerEmail =
        | Unverified of EmailAddress
        | Verified of VerifiedEmailAddress

    type SendPasswordResetEmail = VerifiedEmailAddress -> Undefined

module ContactRuleImplementation =
    type Name = Undefined
    type EmailContactInfo = Undefined
    type PostalContactInfo = Undefined

    // type Contact = {
    //     Name: Name
    //     Email: EmailContactInfo
    //     Address: PostalContactInfo
    // }

    // type Contact = {
    //     Name: Name
    //     Email: EmailContactInfo option
    //     Address: PostalContactInfo option
    // }

    type BothContactMethods =
        { Email: EmailContactInfo
          Address: PostalContactInfo }

    type ContactInfo =
        | EmailOnly of EmailContactInfo
        | AddressOnly of PostalContactInfo
        | EmailAndAddr of BothContactMethods

    type Contact =
        { Name: Name; ContactInfo: ContactInfo }

module IllegalStatesInOurDomain =
    type UnvalidatedAddress = Undefined
    type ValidatedAddress = Undefined

    type AddressValidationService = UnvalidatedAddress -> ValidatedAddress option

    type UnvalidatedOrder = { ShippingAddress: UnvalidatedAddress }

    type ValidatedOrder = { ShippingAddress: ValidatedAddress }


module Consistency =
    type OrderLine = { OrderLineId: int; Price: float }

    type Order =
        { OrderLines: OrderLine list
          AmountToBill: float }

    let findOrderLine orderLineId (lines: OrderLine list) =
        lines |> List.find (fun ol -> ol.OrderLineId = orderLineId)

    let replaceOrderLine orderLineId newOrderLine lines = lines

    let changeOrderLinePrice order orderLineId newPrice =
        let orderLine = order.OrderLines |> findOrderLine orderLineId

        let newOrderLine = { orderLine with Price = newPrice }

        let newOrderLines = order.OrderLines |> replaceOrderLine orderLineId newOrderLine

        let newAmountToBill = newOrderLines |> List.sumBy (fun ol -> ol.Price)

        let newOrder =
            { order with
                OrderLines = newOrderLines
                AmountToBill = newAmountToBill }

        newOrder

    type MoneyTransferId = Undefined
    type AccountId = Undefined
    type Amount = Undefined

    type MoneyTransfer =
        { Id: MoneyTransferId
          ToAccount: AccountId
          FromAccount: AccountId
          Amount: Amount }
