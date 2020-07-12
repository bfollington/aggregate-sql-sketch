namespace Sketch.Persistence

open Sketch.Events
open Sketch

[<RequireQualifiedAccess>]
module Cart = 
  [<CLIMutable>]
  type CartRecord = { Id: int; Status: CartStatus }
  [<CLIMutable>]
  type CartProductRecord = { Sku: string; Name: string; Price: decimal }

  let loadCart id conn = 
    let cart = 
      conn |> Db.queryFirstP<CartRecord>
        """
          SELECT Id, Status FROM Cart
          WHERE Id = @CartId
        """
        {| CartId = id |}
    
    let items =
      conn |> Db.queryP<Product>
        """
          SELECT p.Sku, p.Name, p.Price FROM CartProduct
          JOIN Product as p On p.Sku = CartProduct.Sku
          WHERE CartProduct.CartId = @CartId
        """
        {| CartId = id |}

    let mkCart (cart: CartRecord) (items: Product seq) =
      { CustomerCart.Id = cart.Id; Items = items |> Seq.toList; Status = cart.Status }

    cart
    |> Result.bind(fun c -> items |> Result.map(mkCart c))


  let create (id: int) conn = 
    match id with
    | -1 -> Error "DB: Invalid Id provided"
    | id -> 
      conn |> Db.queryP 
        """
          INSERT INTO Cart 
              (Id, Status)
          VALUES 
              (@Id, @Status)
        """
        {| Id = id; Status = CartStatus.Pending |}

  let addItemToCart (cartId: int) (productSku: string) conn =
    conn |> Db.queryP
      """
        INSERT INTO CartProduct
            (CartId, Sku)
        VALUES 
            (@CartId, @Sku)
      """
      {| CartId = cartId; Sku = productSku |}
        

  let removeItemFromCart (cartId: int) (productSku: string) conn = 
    conn |> Db.queryP
      """
        DELETE FROM CartProduct
        WHERE CartId = @CartId
        AND Sku = @Sku
      """
      {| CartId = cartId; Sku = productSku |}

  let checkout (cartId: int) conn =
    conn |> Db.queryP
      """
        UPDATE Cart
        SET Status = @Status
        WHERE Id = @CartId
      """
      {| CartId = cartId; Status = CartStatus.CheckedOut |}