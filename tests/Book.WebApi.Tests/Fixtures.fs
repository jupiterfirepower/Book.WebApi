module Fixtures

open Book.WebApi.Models
open Book.WebApi.DataAccess

open Xunit
open Microsoft.EntityFrameworkCore
open Giraffe
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open System.IO
open Giraffe.Serialization.Json
open NSubstitute
open Microsoft.Data.Sqlite

[<Literal>]
let public dbName = "Data Source=:memory:;"

let mutable id: int = -1
let public nextId() = 
    id <- id + 1
    id
    
let getTestBook = 
    { 
       BookId = 0
       Author = "Test Author" 
       Title = "Test Title" 
       PublishedYear = 2019
       Pages = 365 
    }

let initializeInMemoryContext = 
    let connectionStringMemory = dbName 
    let connection = new SqliteConnection(connectionStringMemory)
    connection.Open()

    let structureSql =
                "CREATE TABLE IF NOT EXISTS Books (" +
                "BookId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                "Author text NOT NULL, " +
                "Title  text NOT NULL, " + 
                "PublishedYear int NOT NULL, " + 
                "Pages int NOT NULL)"

    //let dropTableSql = "DROP TABLE IF EXISTS Books;"

    //let dropCommand = new SqliteCommand(dropTableSql, connection)
    //dropCommand.ExecuteNonQuery() |> ignore

    let structureCommand = new SqliteCommand(structureSql, connection)
    structureCommand.ExecuteNonQuery() |> ignore

    let builder = new DbContextOptionsBuilder<BooksContext>()
    let context = new BooksContext(builder.UseSqlite(connection).Options)
    context

let populateContext (context : BooksContext) (book : BookData) =
      let connection = new SqliteConnection(dbName)
      connection.Open()
      let structureSql =
                "CREATE TABLE IF NOT EXISTS Books (" +
                "BookId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                "Author text NOT NULL, " +
                "Title  text NOT NULL, " + 
                "PublishedYear int NOT NULL, " + 
                "Pages int NOT NULL)"

    //let dropTableSql = "DROP TABLE IF EXISTS Books;"

    //let dropCommand = new SqliteCommand(dropTableSql, connection)
    //dropCommand.ExecuteNonQuery() |> ignore

      let structureCommand = new SqliteCommand(structureSql, connection)
      structureCommand.ExecuteNonQuery() |> ignore
      let builder = new DbContextOptionsBuilder<BooksContext>()
      let context = new BooksContext(builder.UseSqlite(connection).Options)
      book |> context.Books.Add |> ignore
      context.SaveChanges() |> ignore
      context

let initializeAndPopulateContext (book : BookData) =  
        initializeInMemoryContext |> populateContext <| book

let next : HttpFunc = Some >> Task.FromResult

let getBody (ctx : HttpContext) =
    ctx.Response.Body.Position <- 0L
    use reader = new StreamReader(ctx.Response.Body, System.Text.Encoding.UTF8)
    reader.ReadToEnd()
    
let assertFailf format args =
    let msg = sprintf format args
    Assert.True(false, msg)

let configureContext (dbContext : BooksContext) = 
        let context = Substitute.For<HttpContext>();
        context.RequestServices.GetService(typeof<BooksContext>).Returns(dbContext) |> ignore
        context.RequestServices.GetService(typeof<IJsonSerializer>).Returns(NewtonsoftJsonSerializer(NewtonsoftJsonSerializer.DefaultSettings)) |> ignore
        context.Response.Body <- new MemoryStream()
        context.Request.Headers.ReturnsForAnyArgs(new HeaderDictionary()) |> ignore
        context
    

let shouldContains actual expected = Assert.Contains(actual, expected) 
let shouldEqual expected actual = Assert.Equal(expected, actual)
let shouldNotNull expected = Assert.NotNull(expected)


