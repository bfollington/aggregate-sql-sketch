module Sketch.Db

open Microsoft.Data.Sqlite
open Dapper

module Connection =

    let private mkOnDiskConnectionString (dataSource: string) =
        sprintf
            "Data Source = %s;foreign keys=true;"
            dataSource

    let mkOnDisk () = new SqliteConnection (mkOnDiskConnectionString "./example.db")

let query<'Result> (query:string) (connection: SqliteConnection) =
    try 
        Ok <| connection.Query<'Result>(query)
    with 
    | e -> Error e.Message

let queryP<'Result> (query: string) (param: obj) (connection: SqliteConnection) =
    try 
        Ok <| connection.Query<'Result>(query, param)
    with 
    | e -> Error e.Message
        
let queryFirst<'Result> (query:string) (connection: SqliteConnection) =
    try 
        Ok <| connection.QueryFirst<'Result>(query)
    with 
    | e -> Error e.Message

let queryFirstP<'Result> (query:string) (param: obj) (connection: SqliteConnection) =
    try 
        Ok <| connection.QueryFirst<'Result>(query, param)
    with 
    | e -> Error e.Message