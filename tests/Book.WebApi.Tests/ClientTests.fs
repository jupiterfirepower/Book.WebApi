module ClientTests

open System.Net.Http
open System.Text
open Microsoft.AspNetCore.TestHost
open Xunit
open Newtonsoft.Json
open Fixtures
open HttpFunc
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open System
open System.IO
open Book.WebApi.DbCreator

let createHost() =
    //----------------------------------------------------------------------
    // initialize db file, table - Books structure and insert test data
    let db = SqliteDbCreator()
    db.CreateDb
//----------------------------------------------------------------------
    WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory()) 
        //.UseEnvironment("Test")
        .Configure(Action<IApplicationBuilder> Book.WebApi.App.configureApp)
        .ConfigureServices(Action<IServiceCollection> Book.WebApi.App.configureServices)


[<Fact>]
let ``GET /book should respond empty`` () =
    use server = new TestServer(createHost()) 
    use client = server.CreateClient()

    let text = get client "books"
               |> ensureSuccess
               |> readText
    Assert.True(text.Length > 2)

[<Fact>]
let ``POST /book/ should add a new book`` () =
    use server =  new TestServer(createHost()) 
    use client = server.CreateClient()
    let current = { getTestBook with BookId = 0; Author="Test Author"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    
    let text = post client "book" content
                |> ensureSuccess
                |> readText
    text |> shouldContains "\"author\":\"Test Author\""

    let current = { getTestBook with BookId = 0; Author="Test Author 2"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    post client "book" content
    |> ensureSuccess
    |> readText
    |> shouldContains "\"author\":\"Test Author 2\""

    let current = { getTestBook with BookId = 0; Author="Test Author 3"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    post client "book" content
    |> ensureSuccess
    |> readText
    |> shouldContains "\"author\":\"Test Author 3\""

    let current = { getTestBook with BookId = 0; Author="Test Author 4"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    post client "book" content
    |> ensureSuccess
    |> readText
    |> shouldContains "\"author\":\"Test Author 4\""

    let current = { getTestBook with BookId = 0; Author="Test Author 5"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    post client "book" content
    |> ensureSuccess
    |> readText
    |> shouldContains "\"author\":\"Test Author 5\""

[<Fact>]
let ``POST /book/ should add a new book 2`` () =
    use server =  new TestServer(createHost()) 
    use client = server.CreateClient()
    
    let current = { getTestBook with BookId = 0; Author="Test Author 10"}
    use content = new StringContent(JsonConvert.SerializeObject(current), Encoding.UTF8, "application/json");
    
    post client "book" content
    |> ensureSuccess
    |> readText
    |> shouldContains "\"author\":\"Test Author 10\""
