module Sketch.Aggregate

open Sketch.Persistence

type Product = { Sku: string; Name: string; Price: decimal }

type CartItem = { Product: Product; Quantity: int }

type CustomerCart = 
  {
    Id: string option
    Items: CartItem list
  }
  with
    static member OfDataRecord (r: Cart.CartRecord) = 
      let cartContents (r: Cart.CartRecord) =
        Cart.loadProducts r.CartItems
        |> List.map (fun r -> { Product = { Sku = r.Sku; Name = r.Name; Price = r.Price; }; Quantity = 1; })

      {
        Id = Some r.Id
        Items = r |> cartContents
      }


let zero = { Id = None; Items = [] }

type CustomerCartEvent =
  | CartCreated
  | ItemAddedToCart of CartItem

// CONSIDER: apply should be returning a result here, does that mean we need to split it up? 
let apply event state =
  match event with
  | CartCreated -> { state with Id = Some "123" }
  | ItemAddedToCart item -> 
    // TODO: handle quantities
    { state with Items = state.Items @ [item] }

let persist event =
  match event with
  | CartCreated -> 
    match Cart.create "123" with
    | Ok id -> Ok (sprintf "Cart ID: %A" id)
    | Error e -> Error (sprintf "Could create cart: %A" e)
  | ItemAddedToCart item -> 
    // TODO: handle quantities
    match Cart.addItemToCart "123" item.Product.Sku with
    | Ok items -> Ok (sprintf "Cart contains: %A" items)
    | Error e -> Error (sprintf "Item Add Error: %A" e)

// Read initial DB state -> Hydrate
let hydrate id = 
  Cart.loadCart id
  |> Result.map CustomerCart.OfDataRecord
  |> function
  | Ok c -> c
  | Error e -> 
    printfn "Hydration error: %A, starting from zero" e
    zero