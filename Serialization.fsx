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

module Dto =
    open Domain

    type Person =
        { First: string
          Last: string
          Birthdate: DateTime }

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

module Json =
    let serialize obj = JsonConvert.SerializeObject(obj)

    let deserialize<'a> str =
        try
            JsonConvert.DeserializeObject<'a>(str) |> Ok
        with ex ->
            Error ex

let jsonFromDomain (person: Domain.Person) =
    person |> Dto.fromDomain |> Json.serialize

let person: Domain.Person =
    { First = Domain.String50 "Taro"
      Last = Domain.String50 "Yamada"
      Birthdate = Domain.Birthdate(DateTime(2000, 1, 1)) }

printf "%s" (jsonFromDomain person)
