open Sketch.Db
open Sketch.Persistence
open Sketch.Aggregate
open Sketch.Events.Cart
open Sketch.Commands

let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }

[<EntryPoint>]
let main argv =
    let conn = Connection.mkOnDisk()
    
    conn.Open()
    let txn = conn.BeginTransaction()

    let myCartId = 126
    let cart = Cart.Client conn

    let state = 
        Cart.hydrate myCartId conn
        |> Ok
        |> Result.bind(cart.CreateCart myCartId)
        |> Result.bind(cart.AddItemToCart myCartId fancyHat)
        |> Result.bind(cart.AddItemToCart myCartId uglyHat)
        |> Result.bind(cart.RemoveItemFromCart myCartId fancyHat)
        |> Result.bind(cart.CheckOut myCartId)
        |> Result.bind(cart.Shipped myCartId)

    match state with
    | Ok s -> 
        printfn "Ok, state = %A" s
        txn.Commit()
    | Error s -> 
        printfn "Rejected, error = %A" s
        txn.Rollback()

    printfn "Loaded again, cart = %A" (Cart.hydrate myCartId conn)

    conn.Close()
    printfn "Bye!"
    0 // return an integer exit code
