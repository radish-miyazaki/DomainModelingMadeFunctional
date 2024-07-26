#load "DomainApi.fsx"
#load "Common.fsx"

open Common
open DomainApi

// ----------------------------------------
// 注文のライフサイクル
// https://scrapbox.io/radish-miyazaki/%E7%8A%B6%E6%85%8B%E3%81%AE%E9%9B%86%E5%90%88%E3%81%AB%E3%82%88%E3%82%8B%E6%B3%A8%E6%96%87%E3%81%AE%E3%83%A2%E3%83%87%E3%83%AA%E3%83%B3%E3%82%B0
// ----------------------------------------

type ProductCode = Undefined
type Price = Undefined

// 検証済みの状態
type ValidatedOrderLine = Undefined

type ValidatedOrder =
    { OrderId: OrderId
      CustomerInfo: CustomerInfo
      ShippingAddress: Address
      BillingAddress: Address
      OrderLine: ValidatedOrderLine list }

and OrderId = Undefined
and CustomerInfo = Undefined
and Address = Undefined

// 価格計算済みの状態
type PricedOrderLine = Undefined
type PricedOrder = Undefined

// 全状態の結合
type Oeder =
    | Unvalidated of UnvalidatedOrder
    | Validated of ValidatedOrder
    | Priced of PricedOrder


// ----------------------------------------
// 内部ステップの定義
// https://scrapbox.io/radish-miyazaki/%E5%9E%8B%E3%82%92%E4%BD%BF%E3%81%A3%E3%81%9F%E3%83%AF%E3%83%BC%E3%82%AF%E3%83%95%E3%83%AD%E3%83%BC%E3%81%AE%E5%90%84%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%81%AE%E3%83%A2%E3%83%87%E3%83%AA%E3%83%B3%E3%82%B0
// https://scrapbox.io/radish-miyazaki/%E3%82%A8%E3%83%95%E3%82%A7%E3%82%AF%E3%83%88%E3%81%AE%E6%96%87%E6%9B%B8%E5%8C%96
// ----------------------------------------

// ---------- 注文の検証 ----------

// 注文の検証が使用するサービス
type CheckProduceCodeExists = ProductCode -> bool

type CheckedAddress = Undefined
type AddressValidationError = Undefined
type CheckAddressExists = UnvalidatedAddress -> AsyncResult<CheckedAddress, AddressValidationError>

type ValidateOrder =
    CheckProduceCodeExists
        -> CheckAddressExists
        -> UnvalidatedOrder
        -> AsyncResult<ValidatedOrder, ValidationError list>

and ValidationError = Undefined

// ---------- 注文の価格計算 ----------
type GetProductPrice = ProductCode -> Price

type PricingError = Undefined

type PriceOrder = GetProductPrice -> ValidatedOrder -> Result<PricedOrder, PricingError>

// etc ...
