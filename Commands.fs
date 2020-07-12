namespace Sketch.Commands

open Microsoft.Data.Sqlite

open Sketch.Db
open Sketch.Persistence
open Sketch.Aggregate
open Sketch.Events.Cart

[<RequireQualifiedAccess>]
module Cart =
    type DomainError =
        | CartAlreadyExists of CustomerCart
        | CartCannotBeChanged of CustomerCart
        | CartCannotBeCheckedOut of CustomerCart
        | CartCannotBeShipped of CustomerCart
        | HydrationError of CustomerCart
        | PersistenceError of DbError

    // Mutate the underlying data and refresh state
    let write ev state conn =
        Cart.persist ev conn
        |> function
        | Ok msg ->
            printfn "> %s" msg
            Ok <| Cart.hydrate state.Id conn
        | Error msg ->
            printfn "< %s" msg
            Error(HydrationError state)

    type Client(conn: SqliteConnection) =
        let write ev state = write ev state conn

        member __.CreateCart cartId (state: CustomerCart) =
            if state.Status <> CartStatus.Initial then
                Error(CartAlreadyExists state)
            else
                write <| CartCreated cartId <| state

        member __.AddItemToCart cartId product (state: CustomerCart) =
            if state.Status <> CartStatus.Ready then
                Error(CartCannotBeChanged state)
            else
                write
                <| ItemAddedToCart(cartData cartId product)
                <| state

        member __.RemoveItemFromCart cartId product (state: CustomerCart) =
            if state.Status <> CartStatus.Ready then
                Error(CartCannotBeChanged state)
            else
                write
                <| ItemRemovedFromCart(cartData cartId product)
                <| state

        member __.CheckOut cartId (state: CustomerCart) =
            if state.Status <> CartStatus.Ready then
                Error(CartCannotBeCheckedOut state)
            else
                write <| CheckedOut cartId <| state

        member __.Shipped cartId (state: CustomerCart) =
            if state.Status <> CartStatus.CheckedOut then
                Error(CartCannotBeShipped state)
            else
                write <| ShippedOrder cartId <| state
