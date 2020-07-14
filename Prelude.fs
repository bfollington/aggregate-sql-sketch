namespace Sketch.Prelude

[<RequireQualifiedAccess>]
module Result =
    let map2 (f: 'a -> 'b -> 'c) (a: Result<'a, _>) (b: Result<'b, _>) =
        a |> Result.bind(fun innerA -> b |> Result.map(f innerA))