module Sketch.Aggregate

open Sketch.Events
open Sketch.Persistence

let zero id = { Id = id; Items = []; Status = CartStatus.Pending }

let persist event conn =
  match event with
  | CartCreated cartId -> 
    match Cart.create cartId conn with
    | Ok id -> Ok (sprintf "Cart ID: %A" id)
    | Error e -> Error (sprintf "Could create cart: %A" e)

  | ItemAddedToCart item -> 
    match Cart.addItemToCart item.CartId item.Data.Sku conn with
    | Ok items -> Ok (sprintf "Cart contains: %A" items)
    | Error e -> Error (sprintf "Item Add Error: %A" e)

  | ItemRemovedFromCart item -> 
    match Cart.removeItemFromCart item.CartId item.Data.Sku conn with
    | Ok items -> Ok (sprintf "Cart contains: %A" items)
    | Error e -> Error (sprintf "Item Remove Error: %A" e)
    
  | CheckedOut cartId -> 
    match Cart.checkout cartId conn with
    | Ok status -> Ok (sprintf "Cart is: %A" status)
    | Error e -> Error (sprintf "Checkout Error: %A" e)

// Read initial DB state -> Hydrate
let hydrate (id: int) conn = 
  Cart.loadCart id conn
  |> function
  | Ok c -> c
  | Error e -> 
    printfn "Hydration error: %A, starting from zero" e
    zero id