// Learn more about F# at http://fsharp.org

open System

open Sketch.Aggregate

let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }

[<EntryPoint>]
let main argv =
    let write ev state =
        persist ev
        |> function
        | Ok msg -> 
            printfn "%s" msg
            Ok <| apply ev state
        | Error msg -> 
            printfn "%s" msg
            Error <| state

    let state = 
        hydrate "123"
        |> Ok
        |> Result.bind(write CartCreated)
        |> Result.bind(write (ItemAddedToCart { Product = fancyHat; Quantity = 1 }))
        |> Result.bind(write (ItemAddedToCart { Product = uglyHat; Quantity = 1 }))

    match state with
    | Ok s -> printfn "Ok, state = %A" s
    | Error s -> printfn "Rejected, state = %A" s

    printfn "Loaded again, cart = %A" (hydrate "123").Items

    printfn "Bye!"
    0 // return an integer exit code
