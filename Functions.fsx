#load "Common.fsx"
open Common

module FunctionsAsThings =
    let plus3 x = x + 3
    let times2 x = x * 2
    let square = (fun x -> x * x)
    let addThree = plus3

    let listOfFunctions = [ addThree; times2; square ]

    for fn in listOfFunctions do
        let result = fn 100
        printfn "If 100 is the input, the output is %i" result

module LetDefinedFunction =
    let myString = "hello"
    let square x = x * x

module ValueDefinedFunction =
    let square = (fun x -> x * x)

module FunctionsAsInput =
    let evalWith5ThenAdd2 fn = fn (5) + 2

    let add1 x = x + 1
    evalWith5ThenAdd2 add1

    let square x = x * x
    evalWith5ThenAdd2 square

module FunctionsAsOutput =
    // let add1 x = x + 1
    let add2 x = x + 2
    let add3 x = x + 3

    // let adderGenerator numberToAdd = fun x -> numberToAdd + x
    let adderGenerator numberToAdd =
        let innerFn x = numberToAdd + x

        innerFn

    let add1 = adderGenerator 1
    add1 2

    let add100 = adderGenerator 100
    add100 2

module Currying =
    let add x y = x + y
    let adderGenerator x = fun y -> x + y

module PartialApplication =
    let sayGreeting greeting name = printfn "%s %s" greeting name

    let sayHello = sayGreeting "Hello"
    let sayGoodbye = sayGreeting "Goodbye"

    sayHello "Alex"
    sayGoodbye "Alex"

module TotalFunctions =
    module WithException =
        let twelveDividedBy n =
            match n with
            | 6 -> 2
            | 5 -> 2
            | 4 -> 3
            | 3 -> 4
            | 2 -> 6
            | 1 -> 12
            | 0 -> failwith "Can't divide by zero"

    module RestrictedInput =
        type NonZeroInteger = private NonZeroInteger of int

        let twelveDividedBy (NonZeroInteger n) =
            match n with
            | 6 -> 2
            | 5 -> 2
            | 4 -> 3
            | 3 -> 4
            | 2 -> 6
            | 1 -> 12
    // | 0 -> ...

    module ExtendedOutput =
        let twelveDividedBy n =
            match n with
            | 6 -> Some 2
            | 5 -> Some 2
            | 4 -> Some 3
            | 3 -> Some 4
            | 2 -> Some 6
            | 1 -> Some 12
            | 0 -> None

module Pipe =
    let add1 x = x + 1
    let square x = x * x

    let add1ThenSquare x = x |> add1 |> square

    add1ThenSquare 5

    let isEven x = (x % 2) = 0

    let printBool x = sprintf "value is %b" x

    let isEvenThenPrint x = x |> isEven |> printBool

    isEvenThenPrint 2


module CompositionChallenge =
    let add1 x = x + 1

    let printOption x =
        match x with
        | Some i -> printfn "The int is %i" i
        | None -> printfn "No value"

    5 |> add1 |> Some |> printOption
