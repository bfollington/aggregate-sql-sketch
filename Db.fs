module Sketch.Db

open Microsoft.Data.Sqlite
open Dapper

module Connection =

    let private mkOnDiskConnectionString (dataSource: string) =
        sprintf "Data Source = %s;foreign keys=true;" dataSource

    let mkOnDisk () =
        new SqliteConnection(mkOnDiskConnectionString "./example.db")

type DbError = DbError of string

let trap (action: unit -> 'a) =
    try
        Ok(action ())
    with e -> Error(DbError e.Message)

let query<'Result> (connection: SqliteConnection) (query: string) =
    trap (fun () -> connection.Query<'Result>(query))

let queryP<'Result> (connection: SqliteConnection) (query: string) (param: obj) =
    trap (fun () -> connection.Query<'Result>(query, param))

let queryFirst<'Result> (connection: SqliteConnection) (query: string) =
    trap (fun () -> connection.QueryFirst<'Result>(query))

let queryFirstP<'Result> (connection: SqliteConnection) (query: string) (param: obj) =
    trap (fun () -> connection.QueryFirst<'Result>(query, param))
