#load "Common.fsx"

open Common
open System

// ----------------------------------------
// 入力データ
// https://scrapbox.io/radish-miyazaki/%E3%83%AF%E3%83%BC%E3%82%AF%E3%83%95%E3%83%AD%E3%83%BC%E3%81%AE%E5%85%A5%E5%8A%9B#669d007175d04f00000e97f8
// ----------------------------------------
type UnvalidatedOrder =
    { OrderId: string
      CustomerInfo: UnvalidatedCustomer
      ShippingAddress: UnvalidatedAddress }

and UnvalidatedCustomer = { Name: string; Email: string }
and UnvalidatedAddress = Undefined

// ----------------------------------------
// 入力コマンド
// https://scrapbox.io/radish-miyazaki/%E3%83%AF%E3%83%BC%E3%82%AF%E3%83%95%E3%83%AD%E3%83%BC%E3%81%AE%E5%85%A5%E5%8A%9B#669d02b475d04f00000e9815
// ----------------------------------------
type Command<'data> =
    { Data: 'data
      TimeStamp: DateTime
      UserId: string }

type PlacedOrderCommand = Command<UnvalidatedOrder>

// ----------------------------------------
// パブリック API
// https://scrapbox.io/radish-miyazaki/%E5%9E%8B%E3%82%92%E4%BD%BF%E3%81%A3%E3%81%9F%E3%83%AF%E3%83%BC%E3%82%AF%E3%83%95%E3%83%AD%E3%83%BC%E3%81%AE%E5%90%84%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%81%AE%E3%83%A2%E3%83%87%E3%83%AA%E3%83%B3%E3%82%B0#66a0870875d04f000015259e
// https://scrapbox.io/radish-miyazaki/%E4%BE%9D%E5%AD%98%E9%96%A2%E4%BF%82%E3%82%92%E3%83%91%E3%83%A9%E3%83%A1%E3%83%BC%E3%82%BF%E3%81%AB%E5%90%AB%E3%82%81%E3%82%8B%E3%81%8B#66a2ffc275d04f0000d651f1
// ----------------------------------------
type OrderPlaced = Undefined
type BillableOrderPlaced = Undefined
type OrderAcknowledgmentSent = Undefined

type PlacedOrderEvent =
    | OrderPlaced of OrderPlaced
    | BillableOrderPlaced of BillableOrderPlaced
    | OrderAcknowledgmentSent of OrderAcknowledgmentSent

type PlaceOrderError = Undefined

type PlacedOrderWorkflow = PlacedOrderCommand -> AsyncResult<PlacedOrderEvent list, PlaceOrderError>
