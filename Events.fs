module Sketch.Events


[<CLIMutable>]
type Product = { Sku: string; Name: string; Price: decimal }

type CartItem = { Product: Product; Quantity: int }

type CartStatus = | Pending = 0 | CheckedOut = 1 | Shipped = 2

type CustomerCart = 
  {
    Id: int
    Items: Product list
    Status: CartStatus
  }

type CartData<'a> = { CartId: int; Data: 'a; }
let cartData id data = { CartId = id; Data = data; }

type CustomerCartEvent =
  | CartCreated of int
  | ItemAddedToCart of CartData<Product>
  | ItemRemovedFromCart of CartData<Product>
  | CheckedOut of int
  | ShippedOrder of int