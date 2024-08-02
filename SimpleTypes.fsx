// ----------------------------------------
// 共通の単純型
// https://scrapbox.io/radish-miyazaki/%E5%90%84%E3%82%B9%E3%83%86%E3%83%83%E3%83%97%E3%82%92%E5%AE%9F%E8%A3%85%E3%81%99%E3%82%8B%E5%89%8D%E3%81%AB%E3%80%81%E5%8D%98%E7%B4%94%E5%9E%8B%E3%82%92%E5%AE%9F%E8%A3%85%E3%81%99%E3%82%8B
// ----------------------------------------
module SimpleTypes =
    open System

    module ConstrainedType =
        let createString name ctro maxLen str =
            if String.IsNullOrEmpty(str) then
                let msg = sprintf "%s must not be null or empty" name
                failwith msg
            elif str.Length > maxLen then
                let msg = sprintf "%s must not be more than %i chars" name maxLen
                failwith msg
            else
                ctro str

        let createStringOption name ctor maxLen str =
            if String.IsNullOrEmpty(str) then
                None
            elif str.Length > maxLen then
                let msg = sprintf "%s must not be more than %i chars" name maxLen
                failwith msg
            else
                ctor str |> Some

        let createInt name ctor minVal maxVal i =
            if i < minVal then
                let msg = sprintf "%s must not be less than %i" name minVal
                failwith msg
            elif i > maxVal then
                let msg = sprintf "%s must not be more than %i" name maxVal
                failwith msg
            else
                ctor i

        let createFloat name ctor minVal maxVal f =
            if f < minVal then
                let msg = sprintf "%s must not be less than %f" name minVal
                failwith msg
            elif f > maxVal then
                let msg = sprintf "%s must not be more than %f" name maxVal
                failwith msg
            else
                ctor f

        let createLike name ctor pattern str =
            if String.IsNullOrEmpty(str) then
                let msg = sprintf "%s must not be null or empty" name
                failwith msg
            elif System.Text.RegularExpressions.Regex.IsMatch(str, pattern) then
                ctor str
            else
                let msg = sprintf "%s: %s must match the patten %s" name str pattern
                failwith msg

    // 50 文字以下の文字列
    type String50 = private String50 of string

    module String50 =
        let value (String50 str) = str

        let create str =
            ConstrainedType.createString "String50" String50 50 str

        let createOption str =
            ConstrainedType.createStringOption "String50" String50 50 str

    type EmailAddress = private EmailAddress of string

    module EmailAddress =
        let value (EmailAddress str) = str

        let create str =
            let pattern = ".+@.+"
            ConstrainedType.createLike "EmailAddress" EmailAddress pattern str

    type ZipCode = private ZipCode of string

    module ZipCode =
        let value (ZipCode str) = str

        let create str =
            let pattern = "\d{7}"
            ConstrainedType.createLike "ZipCode" ZipCode pattern str

    type OrderId = private OrderId of string

    module OrderId =
        let value (OrderId str) = str

        let create str =
            ConstrainedType.createString "OrderId" OrderId 50 str

    type OrderLineId = private OrderLineId of string

    module OrderLineId =
        let value (OrderLineId str) = str

        let create str =
            ConstrainedType.createString "OrderLineId" OrderLineId 50 str

    type WidgetCode = private WidgetCode of string

    module WidgetCode =
        let value (WidgetCode code) = code

        let create code =
            let pattern = "W\d{4}"
            ConstrainedType.createLike "WidgetCode" WidgetCode pattern code

    type GizmoCode = private GizmoCode of string

    module GizmoCode =
        let value (GizmoCode code) = code

        let create code =
            let pattern = "G\d{3}"
            ConstrainedType.createLike "GizmoCode" GizmoCode pattern code

    type ProductCode =
        | Widget of WidgetCode
        | Gizmo of GizmoCode

    module ProductCode =
        let value productCode =
            match productCode with
            | Widget(WidgetCode wc) -> wc
            | Gizmo(GizmoCode gc) -> gc

        let create code =
            if String.IsNullOrEmpty(code) then
                let msg = sprintf "ProductCode: must not be null or empty"
                failwith msg
            else if code.StartsWith("W") then
                WidgetCode code |> Widget
            else if code.StartsWith("G") then
                GizmoCode code |> Gizmo
            else
                let msg = sprintf "ProductCode: format not recognized '%s'" code
                failwith msg

    type UnitQuantity = private UnitQuantity of int

    module UnitQuantity =
        let value (UnitQuantity v) = v

        let create v =
            ConstrainedType.createInt "UnitQuantity" UnitQuantity 1 1000 v

    type KilogramQuantity = private KilogramQuantity of float

    module KilogramQuantity =
        let value (KilogramQuantity v) = v

        let create v =
            ConstrainedType.createFloat "KilogramQuantity" KilogramQuantity 0.5 100.0 v

    type OrderQuantity =
        | Unit of UnitQuantity
        | Kilogram of KilogramQuantity

    module OrderQuantity =
        let value qty =
            match qty with
            | Unit uq -> uq |> UnitQuantity.value |> float
            | Kilogram kq -> kq |> KilogramQuantity.value

        let create v =
            ConstrainedType.createInt "OrderQuantity" UnitQuantity 1 1000 v

    type Price = private Price of float

    module Price =
        let value (Price v) = v

        let create v =
            ConstrainedType.createFloat "Price" Price 0.0 1000.0 v

        let multiply qty (Price p) = create (qty * p)


    type BillingAmount = private BillingAmount of float

    module BillingAmount =
        let value (BillingAmount v) = v

        let create v =
            ConstrainedType.createFloat "BillingAmount" BillingAmount 0.0 10000.0 v

        let sumPrices prices =
            prices |> List.map Price.value |> List.sum |> create

    type PdfAttachment = { Name: string; Bytes: byte[] }

    type HtmlString = HtmlString of string
