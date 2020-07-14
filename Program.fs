open Sketch.Db
open Sketch.Persistence
open Sketch.Aggregate
open Sketch.Events.Cart
open Sketch.Commands
open Microsoft.Data.Sqlite

// Prevent this is client input from the browser
let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }

let setup () =
    try
        let conn = Connection.mkOnDisk() 
        conn.Open()
        printfn "... Connected!"
        Ok <| conn.BeginTransaction()
    with
    | e -> Error e

let execute (work: SqliteConnection -> Result<'a, 'b>) =
    match setup() with
    | Error exn -> printfn "! Failed to connect to DB: %A" exn.Message
    | Ok txn -> 
        let conn = txn.Connection
        let result = work conn

        printfn "\n----\n"

        match result with
        | Ok _ -> txn.Commit()
        | Error _ -> txn.Rollback()

        conn.Close()

[<EntryPoint>]
let main argv =
    let simulateJourney cartId conn =
        let (>>=) m f = m |> Result.bind f
        
        let cart = Cart.Client conn

        let res = 
            Cart.hydrate cartId conn |> Ok
            >>= cart.CreateCart cartId
            >>= cart.AddItemToCart cartId fancyHat
            >>= cart.AddItemToCart cartId uglyHat
            >>= cart.RemoveItemFromCart cartId fancyHat
            >>= cart.CheckOut cartId
            >>= cart.Shipped cartId
        
        match res with
        | Ok s -> 
            printfn "Ok, state =\n\n %A" s
        | Error s -> 
            printfn "Rejected, error =\n\n %A" s

        res

    let loadCart cartId conn =
        Ok <| printfn "Loaded again, cart =\n\n %A" (Cart.hydrate cartId conn)

    execute <| simulateJourney 123
    execute <| loadCart 123

    printfn "Bye!"
    0 // return an integer exit code
