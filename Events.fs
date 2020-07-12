namespace Sketch.Events

module Cart = 
  [<CLIMutable>]
  type Product = { Sku: string; Name: string; Price: decimal }
  type CartStatus = | Initial = 0 | Ready = 1 | CheckedOut = 2 | Shipped = 3

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