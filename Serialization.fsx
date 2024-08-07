#load "Common.fsx"
#r "nuget: Newtonsoft.Json" // F# スクリプトで NuGet パッケージを使う

open System
open Common
open Newtonsoft.Json

module Domain =
    type String50 = String50 of string

    module String50 =
        let create fieldName str =
            if String.IsNullOrEmpty(str) then
                Error(fieldName + " must not be null or empty")
            elif str.Length > 50 then
                Error(fieldName + " must not be more than 50 chars")
            else
                String50 str |> Ok

        let value (String50 value) = value

    type Birthdate = Birthdate of DateTime

    module Birthdate =
        let create (value: DateTime) =
            if value > DateTime.Now then
                Error "Birthdate must be in the past"
            else
                Birthdate value |> Ok

        let value (Birthdate value) = value

    type Person =
        { First: String50
          Last: String50
          Birthdate: Birthdate }

    type Name = { First: String50; Last: String50 }

    type Example =
        | A
        | B of int
        | C of string list
        | D of Name

module Dto =
    open Domain

    type Person =
        { First: string
          Last: string
          Birthdate: DateTime }

    module Person =
        let fromDomain (person: Domain.Person) : Person =
            { First = person.First |> String50.value
              Last = person.Last |> String50.value
              Birthdate = person.Birthdate |> Birthdate.value }

        let toDomain (dto: Person) : Result<Domain.Person, string> =
            result {
                let! first = dto.First |> String50.create "First"
                let! last = dto.Last |> String50.create "Last"
                let! birthdate = dto.Birthdate |> Birthdate.create

                return
                    { First = first
                      Last = last
                      Birthdate = birthdate }
            }

    type NameDto = { First: string; Last: string }

    type ExampleDto =
        { Tag: string
          BData: Nullable<int>
          CData: string[]
          DData: NameDto }

    type ResultDto<'OkData, 'ErrorData when 'OkData: null and 'ErrorData: null> =
        { IsError: bool
          OkData: 'OkData
          ErrorData: 'ErrorData }

    type PlaceOrderEventDto = Undefined
    type PlaceOrderErrorDto = Undefined

    type PlaceOrderResultDto =
        { IsError: bool
          OkData: PlaceOrderEventDto[]
          ErrorData: PlaceOrderErrorDto }


    module Example =
        open System.Collections.Generic

        // let nameDtoFromDomain (name: Domain.Name) : NameDto =
        //     { First = name.First |> String50.value
        //       Last = name.Last |> String50.value }

        // let fromDomain (domainObj: Domain.Example) : ExampleDto =
        //     let nullBData = Nullable()
        //     let nullCData = null
        //     let nullDData = Unchecked.defaultof<NameDto>

        //     match domainObj with
        //     | A ->
        //         { Tag = "A"
        //           BData = nullBData
        //           CData = nullCData
        //           DData = nullDData }
        //     | B i ->
        //         let bdata = Nullable i

        //         { Tag = "B"
        //           BData = bdata
        //           CData = nullCData
        //           DData = nullDData }
        //     | C strList ->
        //         let cdata = strList |> List.toArray

        //         { Tag = "B"
        //           BData = nullBData
        //           CData = cdata
        //           DData = nullDData }
        //     | D name ->
        //         let ddata = name |> nameDtoFromDomain

        //         { Tag = "D"
        //           BData = nullBData
        //           CData = nullCData
        //           DData = ddata }

        // let nameDtoToDomain (nameDto: NameDto) : Result<Domain.Name, string> =
        //     result {
        //         let! first = nameDto.First |> String50.create "First"
        //         let! last = nameDto.Last |> String50.create "Last"

        //         return { First = first; Last = last }
        //     }

        // let toDomain (dto: ExampleDto) : Result<Domain.Example, string> =
        //     match dto.Tag with
        //     | "A" -> Ok A
        //     | "B" ->
        //         if dto.BData.HasValue then
        //             dto.BData.Value |> B |> Ok
        //         else
        //             Error "B data not expired to be null"
        //     | "C" ->
        //         match dto.CData with
        //         | null -> Error "C data not expired to be null"
        //         | _ -> dto.CData |> Array.toList |> C |> Ok
        //     | "D" ->
        //         match box dto.DData with
        //         | null -> Error "D data not expired to be null"
        //         | _ -> dto.DData |> nameDtoToDomain |> Result.map D
        //     | _ ->
        //         let msg = sprintf "Tag: '%s' is not recognized" dto.Tag
        //         Error msg

        let nameDtoFromDomain (name: Domain.Name) : IDictionary<string, obj> =
            let first = name.First |> String50.value :> obj
            let last = name.Last |> String50.value :> obj
            [ ("First", first); ("Last", last) ] |> dict

        let fromDomain (domainObj: Domain.Example) : IDictionary<string, obj> =
            match domainObj with
            | A -> [ ("A", null) ] |> dict
            | B i ->
                let bdata = Nullable i :> obj
                [ ("B", bdata) ] |> dict
            | C strList ->
                let cdata = strList |> List.toArray :> obj
                [ ("C", cdata) ] |> dict
            | D name ->
                let ddata = name |> nameDtoFromDomain :> obj
                [ ("D", ddata) ] |> dict

        let getValue key (dict: IDictionary<string, obj>) : Result<'a, string> =
            match dict.TryGetValue key with
            | (true, value) -> // キーが見つかった
                try
                    // 'a にダウンキャストして Ok にラップ
                    (value :?> 'a) |> Ok
                with :? InvalidCastException ->
                    // キャストに失敗
                    let typeName = typeof<'a>.Name
                    let msg = sprintf "Value could not be cast to %s" typeName
                    Error msg
            | (false, _) -> // キーが見つからなかった
                let msg = sprintf "Key '%s' not found" key
                Error msg

        let nameDtoToDomain (nameDto: IDictionary<string, obj>) : Result<Name, string> =
            result {
                let! firstStr = nameDto |> getValue "First"
                let! first = firstStr |> String50.create "First"
                let! lastStr = nameDto |> getValue "Last"
                let! last = lastStr |> String50.create "Last"
                return { First = first; Last = last }
            }

        let toDomain (dto: IDictionary<string, obj>) : Result<Example, string> =
            if dto.ContainsKey "A" then
                Ok A
            elif dto.ContainsKey "B" then
                result {
                    let! bdata = dto |> getValue "B" // 失敗する可能性がある
                    return B bdata
                }
            elif dto.ContainsKey "C" then
                result {
                    let! cdata = dto |> getValue "C" // 失敗する可能性がある
                    return cdata |> Array.toList |> C
                }
            elif dto.ContainsKey "D" then
                result {
                    let! ddata = dto |> getValue "D" // 失敗する可能性がある
                    let! name = ddata |> nameDtoToDomain // ここも失敗する可能性がある
                    return name |> D
                }
            else
                let msg = sprintf "Tag not found in dictionary"
                Error msg


module Json =
    let serialize obj = JsonConvert.SerializeObject(obj)

    let deserialize<'a> str =
        try
            JsonConvert.DeserializeObject<'a>(str) |> Ok
        with ex ->
            Error ex

let jsonFromDomain (person: Domain.Person) =
    person |> Dto.Person.fromDomain |> Json.serialize

let person: Domain.Person =
    { First = Domain.String50 "Taro"
      Last = Domain.String50 "Yamada"
      Birthdate = Domain.Birthdate(DateTime(2000, 1, 1)) }

jsonFromDomain person |> printfn "%s"

type DtoError =
    | ValidationError of string
    | DeserialiozationException of exn

let jsonToDomain jsonString : Result<Domain.Person, DtoError> =
    result {
        let! deserializedValue = jsonString |> Json.deserialize |> Result.mapError DeserialiozationException

        let! domainValue = deserializedValue |> Dto.Person.toDomain |> Result.mapError ValidationError

        return domainValue
    }


let jsonPerson =
    """{"First":"Taro","Last":"Yamada","Birthdate":"2000-01-01T00:00:00"}"""

jsonToDomain jsonPerson |> printfn "%A"


let jsonPersonWithErrors =
    """{"First": "", "Last": "Yamada", "Birthdate": "2000-01-01T00:00:00"}"""

jsonToDomain jsonPersonWithErrors |> printfn "%A"
