namespace Sketch.Persistence

[<RequireQualifiedAccess>]
module Cart = 
  type CartRecord = 
    {
      Id: string
      CartItems: string list
    }

  type ProductRecord = { Sku: string; Name: string; Price: decimal }

  let fancyHat = { Sku = "123"; Name = "Fancy Hat"; Price = 999.0M }
  let uglyHat = { Sku = "456"; Name = "Ugly Hat"; Price = 1.0M }
  let productsTable = 
    Map.empty
      .Add("123", fancyHat)
      .Add("456", uglyHat)
  
  let mutable mutableFakeFb: CartRecord option = None

  let loadProducts (skuList: string list) =
    skuList
    |> List.map (fun sku -> Map.tryFind sku productsTable)
    |> List.choose id

  let loadCart id = 
    match mutableFakeFb with
    | Some m -> 
      printfn "Cart Loaded: %A" m
      Ok m
    | None -> Error "DB: Cart does not exist"

  let create id = 
    match id with
    | "" -> Error "DB: Invalid Id provided"
    | id -> 
      mutableFakeFb <- Some { Id = id; CartItems = [] }
      Ok id

  let addItemToCart cartId productSku =
    // Look up cartId, if cart doesn't exist then throw error
    match mutableFakeFb with
    | Some m -> 
      // TODO: if product exists in productsTable
      let draft = { m with CartItems = m.CartItems @ [productSku] }
      mutableFakeFb <- Some draft
      Ok draft.CartItems
    | None -> Error "DB: Cart does not exist"