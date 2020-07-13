namespace Sketch.Aggregate

open Sketch.Persistence
open Sketch.Events.Cart

[<AutoOpen>]
module ErrorWrapper =
    let formatResult onOk onError r : Result<string, string> =
        match r with
        | Ok _ -> onOk |> Ok
        | Error e -> sprintf "%s: %A" onError e |> Error

[<RequireQualifiedAccess>]
module Cart =
    let zero id =
        { Id = id
          Items = []
          Status = CartStatus.Initial }

    let persist event conn =
        match event with
        | CartCreated cartId ->
            Cart.create cartId conn
            |> formatResult
                (sprintf "Cart Created: %A" cartId)
                "Create Cart Error"

        | ItemAddedToCart item ->
            Cart.addItemToCart item.CartId item.Data.Sku conn
            |> formatResult
                (sprintf "Added %A to cart %A" item.Data.Sku item.CartId)
                "Item Add Error"

        | ItemRemovedFromCart item ->
            Cart.removeItemFromCart item.CartId item.Data.Sku conn
            |> formatResult
                (sprintf "Removed %A from cart: %A" item.Data.Sku item.CartId)
                "Item Remove Error"

        | CheckedOut cartId ->
            Cart.checkout cartId conn
            |> formatResult
                (sprintf "Checked out %A" cartId)
                "Checkout Error"

        | ShippedOrder cartId ->
            Cart.shipped cartId conn
            |> formatResult
                (sprintf "Shipped %A" cartId)
                "Shipping Error"

    // Read initial DB state -> Hydrate
    let hydrate (id: int) conn =
        Cart.loadCart id conn
        |> function
        | Ok c -> c
        | Error e ->
            printfn "Hydration failed: %A, starting from zero" e
            zero id
