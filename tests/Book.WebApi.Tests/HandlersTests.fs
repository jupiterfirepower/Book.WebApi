module HandlersTests

open Xunit
open Book.WebApi
open Book.WebApi.RequestModels
open Fixtures
open Giraffe
open Microsoft.AspNetCore.Http
open NSubstitute
open Newtonsoft.Json
open System.IO
open System.Text
open FSharp.Control.Tasks.V2
open System.Linq;

[<Fact>]
let ``POST /book/ should add a new book`` () =

    let book = { 
       BookId = nextId()
       Author = "Test Author 3" 
       Title = "Test Title 3" 
       PublishedYear = 2017
       Pages = 362 
    }
                
    let postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(book))
    let contexttmp = initializeInMemoryContext
    let context = contexttmp |> configureContext
    let last = contexttmp.Books.FirstOrDefault(fun x -> x.Author="Test Author 3")
    
    
    context.Request.Method.ReturnsForAnyArgs "POST" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/book")) |> ignore
    context.Request.Body <- new MemoryStream(postData)

  
    task {
          let! result = App.webApp next context
          match result with
                  | None -> assertFailf "Result was expected to be %s" "[]"
                  | Some ctx ->
                      getBody ctx
                            |> shouldContains "\"author\":\"Test Author 3\""
          
        }  

[<Fact>]
let ``GET /book/id should returns not found when id does not exists`` () =
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId}
    let context =  initializeAndPopulateContext entity |> configureContext;
    
    context.Request.Method.ReturnsForAnyArgs "GET" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/book/999")) |> ignore
  
    task {
          let! result = App.webApp next context
          match result with
                  | None -> assertFailf "Result was expected to be %s" "[]"
                  | Some ctx ->
                         getBody ctx
                                  |> shouldContains "Book not found"
          
        } 

[<Fact>]
let ``GET /book/id should returns the correct response with correct id`` () =
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId}
    let contexttmp = initializeAndPopulateContext entity
    let context = contexttmp |> configureContext;
    let first = contexttmp.Books.FirstOrDefault(fun x-> x.BookId = if currentId = 0 then 1 else currentId )
    
    context.Request.Method.ReturnsForAnyArgs "GET" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/book/" + (first.BookId |> string) )) |> ignore
                   
  
    task {
          let! result = App.webApp next context
          match result with
                  | None -> assertFailf "Result was expected to be %s" "[]"
                  | Some ctx ->
                             getBody ctx
                                  |> shouldContains ("\"bookId\":" + (first.BookId |> string))
        }
    

[<Fact>]
let ``PUT /book/id should modify a book`` () =
    let currentId = nextId()
    let book = { 
       BookId = currentId
       Author = "Test Author 5" 
       Title = "Test Title 5" 
       PublishedYear = 2015
       Pages = 369 
    }
                    
    let postData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(book))
    let entity = { getTestBook with BookId = nextId(); Author = "Test Author 5" }
    let contexttmp = initializeAndPopulateContext entity
    let context = contexttmp |> configureContext
    let first = contexttmp.Books.FirstOrDefault(fun x -> x.Author="Test Author 5")
    
    
    context.Request.Method.ReturnsForAnyArgs "PUT" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/book/" + (first.BookId |> string) )) |> ignore
    context.Request.Body <- new MemoryStream(postData)

  
    task {
          let! result = App.webApp next context
          match result with
                  | None -> assertFailf "Result was expected to be %s" "[]"
                  | Some ctx ->
                      getBody ctx
                         |> shouldContains "\"author\":\"Test Author 5\""
          
        }
  
[<Fact>]
let ``GET /books should returns the correct response`` () =
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId }
    let context =  initializeAndPopulateContext entity
                    |> configureContext;
    
    context.Request.Method.ReturnsForAnyArgs "GET" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/books")) |> ignore
  
    task {
          let! result = App.webApp next context
          
          match result with
                  | None -> assertFailf "Result was expected to be %s" "[]"
                  | Some ctx ->
                       getBody ctx
                               |> shouldContains "\"author\":\"Test Author\""
        }

[<Fact>]
let ``DELETE /book/ should delete the book correctly`` () =
    let currentId = nextId()
    let entity = { getTestBook with BookId = currentId }
    let contexttmp = initializeAndPopulateContext entity
    let context = contexttmp |> configureContext
    let first = contexttmp.Books.FirstOrDefault(fun x-> x.BookId = if currentId = 0 then 1 else currentId )
    
      
    context.Request.Method.ReturnsForAnyArgs "DELETE" |> ignore
    context.Request.Path.ReturnsForAnyArgs (PathString("/book/" + (first.BookId |> string))) |> ignore
   
    task {
           let! result =  App.webApp next context
           match result with
                   | None -> assertFailf "Result was expected to be %s" "[]"
                   | Some ctx ->
                       getBody ctx
                           |> shouldContains (sprintf "\"bookId\":%d" first.BookId)
           
         }
        
  
