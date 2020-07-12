open Sketch.Db
open Sketch.Persistence
open Sketch.Aggregate
open Sketch.Events.Cart
open Sketch.Commands

// Prevent this is client input from the browser
let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }

let setup =
    try
        let conn = Connection.mkOnDisk() 
        conn.Open()
        Ok <| conn.BeginTransaction()
    with
    | e -> Error e

[<EntryPoint>]
let main argv =
    match setup with
    | Error exn -> printfn "! Failed to connect to DB: %A" exn.Message
    | Ok txn -> 
        let conn = txn.Connection
        let myCartId = 123
        let cart = Cart.Client conn

        let state = 
            Cart.hydrate myCartId conn |> Ok
            |> Result.bind(cart.CreateCart myCartId)
            |> Result.bind(cart.AddItemToCart myCartId fancyHat)
            |> Result.bind(cart.AddItemToCart myCartId uglyHat)
            |> Result.bind(cart.RemoveItemFromCart myCartId fancyHat)
            |> Result.bind(cart.CheckOut myCartId)
            |> Result.bind(cart.Shipped myCartId)

        printfn "\n----\n"

        match state with
        | Ok s -> 
            printfn "Ok, state =\n\n %A" s
            txn.Commit()
        | Error s -> 
            printfn "Rejected, error =\n\n %A" s
            txn.Rollback()

        printfn "\n----\n"

        printfn "Loaded again, cart =\n\n %A" (Cart.hydrate myCartId conn)

        conn.Close()

    printfn "Bye!"
    0 // return an integer exit code
