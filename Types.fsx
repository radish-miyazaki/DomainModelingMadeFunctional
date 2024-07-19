#load "Common.fsx"
open Common

module DefiningFunctions =
    let add1 x = x + 1
    let add x y = x + y

    let squarePlusOne x =
        let square = x * x
        square + 1

    let areEqual x y = (x = y)

module CompositionOfTypes =
    type AppleVariety =
        | GoldenDelicious
        | GrannySmith
        | Fuji

    type BananaVariety =
        | Cavendish
        | GrosMichel
        | Manzano

    type CherryVariety =
        | Montmorency
        | Bing

    type FruitSalad =
        { Apple: AppleVariety
          Banana: BananaVariety
          Cherries: CherryVariety }

    type FruitSnack =
        | Apple of AppleVariety
        | Banana of BananaVariety
        | Cherries of CherryVariety

    type ProductCode = ProductCode of string

module WorkingWithTypes =
    type Person = { First: string; Last: string }
    let aPerson = { First = "Alex"; Last = "Adams" }

    let { First = first; Last = last } = aPerson

    type OrderQuantity =
        | UnitQuantity of int
        | KilogramQuantity of float

    let anOrderQtyInUnits = UnitQuantity 5
    let anOrderQtyInKg = KilogramQuantity 2.5

    let printQuantity aOrderQty =
        match aOrderQty with
        | UnitQuantity uQty -> printfn "Order quantity: %d" uQty
        | KilogramQuantity kgQty -> printfn "Order quantity: %g kg" kgQty

    printQuantity anOrderQtyInUnits
    printQuantity anOrderQtyInKg

    module AlternatePropertySyntax =
        let first = aPerson.First
        let last = aPerson.Last

module BuildingADomainModel =
    type CheckNumber = CheckNumber of int
    type CardNumber = CardNumber of string

    type CardType =
        | Visa
        | MasterCard

    type CreditCardInfo =
        { CardType: CardType
          CardNumber: CardNumber }

    type PaymentMethod =
        | Cash
        | Check of CheckNumber
        | Card of CreditCardInfo

    type PaymentAmount = PaymentAmount of float

    type Currency =
        | EUR
        | USD

    type Payment =
        { Amount: PaymentAmount
          Currency: Currency
          Method: PaymentMethod }

    type UnpaidInvoice = Undefined
    type PaidInvoice = Undefined

    type PayInvoce = UnpaidInvoice -> Payment -> PaidInvoice

module ModelingOptionalValues =
    type PersonalName =
        { FirstName: string
          MiddleName: string option
          LastName: string }

module ModelingErrors =
    type UnpaidInvoice = Undefined
    type PaidInvoice = Undefined
    type Payment = Undefined

    type PaymentError =
        | CardTypeNotRecognized
        | PaymentRejected
        | PaymentProviderOffline

    type PayInvoice = UnpaidInvoice -> Payment -> Result<PaidInvoice, PaymentError>

module ModelingNoValue =
    type Customer = Undefined

    type SaveCustomer = Customer -> unit

    type NextRandom = unit -> int

module ModelingLists =
    type OrderId = Undefined
    type OrderLine = Undefined

    type Order =
        { OrderId: OrderId
          Lines: OrderLine list }

    let aList = [ 1; 2; 3 ]
    let aNewList = 0 :: aList

    let printList1 aList =
        match aList with
        | [] -> printfn "list is empty"
        | [ x ] -> printfn "list has one element: %A" x
        | [ x; y ] -> printfn "list has two elements: %A and %A" x y
        | _ -> printfn "list has more than two elements"

    let printList2 aList =
        match aList with
        | [] -> printfn "list is empty"
        | first :: rest -> printfn "list is non-empty with first element begin: %A" first

    printList1 aList
    printList2 aList

module FileOrganizationInOrder =
    module Payments =
        type CheckNumber = Checknumber of int

        type PaymentMethod =
            | Cash
            | Check of CheckNumber
            | Card of Undefined

        type Payment =
            { Amount: Undefined
              Currency: Undefined
              Method: PaymentMethod }

module FileOrganizationUsingRec =
    module rec Payments =
        type Payment =
            { Amount: Undefined
              Currency: Undefined
              Method: PaymentMethod }

        type PaymentMethod =
            | Cash
            | Check of CheckNumber
            | Card of Undefined

        type CheckNumber = Checknumber of int
