open Sketch.Events
open Sketch.Aggregate
open Sketch.Db

let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }

[<EntryPoint>]
let main argv =
    let conn = Connection.mkOnDisk()
    
    conn.Open()
    let txn = conn.BeginTransaction()

    let write ev state =
        persist ev conn
        |> function
        | Ok msg -> 
            printfn "%s" msg
            Ok <| hydrate state.Id conn
        | Error msg -> 
            printfn "%s" msg
            Error <| state

    let myCartId = 123

    let state = 
        hydrate myCartId conn
        |> Ok
        |> Result.bind(write <| CartCreated myCartId)
        |> Result.bind(write <| ItemAddedToCart (cartData myCartId fancyHat))
        |> Result.bind(write <| ItemAddedToCart (cartData myCartId uglyHat))
        |> Result.bind(write <| ItemRemovedFromCart (cartData myCartId fancyHat))
        |> Result.bind(write <| CheckedOut myCartId)
        |> Result.bind(write <| ShippedOrder myCartId)

    match state with
    | Ok s -> 
        printfn "Ok, state = %A" s
        txn.Commit()
    | Error s -> 
        printfn "Rejected, state = %A" s
        txn.Rollback()

    printfn "Loaded again, cart = %A" (hydrate myCartId conn)

    conn.Close()
    printfn "Bye!"
    0 // return an integer exit code
