#load "Common.fsx"
open Common

type Item = Undefined

type ActiveCartData = { UnpaidItems: Item list }

type PaidCartData =
    { PaidItems: Item list; Payment: float }

type ShoppingCart =
    | EmptyCart
    | ActiveCart of ActiveCartData
    | PaidCart of PaidCartData

let addItem cart item =
    match cart with
    | EmptyCart -> ActiveCart { UnpaidItems = [ item ] }
    | ActiveCart { UnpaidItems = existingItems } -> ActiveCart { UnpaidItems = item :: existingItems }
    | PaidCart _ -> cart

let makePayment cart payment =
    match cart with
    | EmptyCart -> cart
    | ActiveCart { UnpaidItems = existingItems } ->
        PaidCart
            { PaidItems = existingItems
              Payment = payment }
    | PaidCart _ -> cart
