namespace Sketch.Persistence

open Sketch.Events.Cart
open Sketch

[<RequireQualifiedAccess>]
module Cart = 
  [<CLIMutable>]
  type CartRecord = { Id: int; Status: CartStatus }

  let loadCart id conn = 
    let cart = 
      Db.queryFirstP<CartRecord> 
        conn
        """
          SELECT Id, Status FROM Cart
          WHERE Id = @CartId
        """
        {| CartId = id |}
    
    let items =
      Db.queryP<Product>
        conn
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
    Db.queryP 
      conn
      """
        INSERT INTO Cart 
            (Id, Status)
        VALUES 
            (@Id, @Status)
      """
      {| Id = id; Status = CartStatus.Ready |}

  let addItemToCart (cartId: int) (productSku: string) conn =
    Db.queryP
      conn
      """
        INSERT INTO CartProduct
            (CartId, Sku)
        VALUES 
            (@CartId, @Sku)
      """
      {| CartId = cartId; Sku = productSku |}
        

  let removeItemFromCart (cartId: int) (productSku: string) conn = 
    Db.queryP
      conn
      """
        DELETE FROM CartProduct
        WHERE CartId = @CartId
        AND Sku = @Sku
      """
      {| CartId = cartId; Sku = productSku |}

  let checkout (cartId: int) conn =
    Db.queryP
      conn
      """
        UPDATE Cart
        SET Status = @Status
        WHERE Id = @CartId
      """
      {| CartId = cartId; Status = CartStatus.CheckedOut |}

  let shipped (cartId: int) conn =
    Db.queryP
      conn
      """
        UPDATE Cart
        SET Status = @Status
        WHERE Id = @CartId
      """
      {| CartId = cartId; Status = CartStatus.Shipped |}