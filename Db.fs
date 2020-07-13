module Sketch.Db

open Microsoft.Data.Sqlite
open Dapper

module Connection =

    let private mkOnDiskConnectionString (dataSource: string) =
        sprintf "Data Source = %s;foreign keys=true;" dataSource

    let mkOnDisk () =
        new SqliteConnection(mkOnDiskConnectionString "./example.db")

type DbError = DbError of string

let trap f x = try f x |> Ok with e -> e.Message |> DbError |> Error
let trap2 f x y = try f x y |> Ok with e -> e.Message |> DbError |> Error

let query<'Result> (connection: SqliteConnection) =
    trap <| fun (query: string) -> connection.Query<'Result>(query)

let queryP<'Result> (connection: SqliteConnection) =
    trap2 <| fun query param -> connection.Query<'Result>(query, param)

let queryFirst<'Result> (connection: SqliteConnection) =
    trap <| fun (query: string) -> connection.QueryFirst<'Result>(query)

let queryFirstP<'Result> (connection: SqliteConnection) =
    trap2 <| fun query param -> connection.QueryFirst<'Result>(query, param)
