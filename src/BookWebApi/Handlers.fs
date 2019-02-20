module Book.WebApi.Handlers

open Book.WebApi.DataAccess
open Book.WebApi.RequestModels
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2

let booksHandler = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        use context = ctx.RequestServices.GetService(typeof<BooksContext>) :?> BooksContext
        getAll context |> ctx.WriteJsonAsync

let bookHandler (id : int) = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        use context = ctx.RequestServices.GetService(typeof<BooksContext>) :?> BooksContext
        getBook context id |> function 
        | Some l -> ctx.WriteJsonAsync l
        | None -> (setStatusCode 404 >=> json "Book not found") next ctx

let bookAddHandler : HttpHandler = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task { 
            use context = ctx.RequestServices.GetService(typeof<BooksContext>) :?> BooksContext
            let! book = ctx.BindJsonAsync<CreateUpdateBookRequest>()
            match book.HasErrors with
            | Some msg -> return! (setStatusCode 400 >=> json msg) next ctx
            | None -> 
                return! addBookAsync context book.GetBook
                        |> Async.RunSynchronously
                        |> function 
                        | Some l -> (setStatusCode 200 >=> json l) next ctx
                        | None -> (setStatusCode 400 >=> json "Book not added") next ctx
        }

let bookUpdateHandler (id : int) = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task { 
            use context = ctx.RequestServices.GetService(typeof<BooksContext>) :?> BooksContext
            let! book = ctx.BindJsonAsync<CreateUpdateBookRequest>()
            match book.HasErrors with
            | Some msg -> return! (setStatusCode 400 >=> json msg) next ctx
            | None -> 
                return! updateBook context book.GetBook id |> function 
                        | Some l -> ctx.WriteJsonAsync l
                        | None -> (setStatusCode 400 >=> json "Book not updated") next ctx
        }

let bookDeleteHandler (id : int) = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        use context = ctx.RequestServices.GetService(typeof<BooksContext>) :?> BooksContext
        deleteBook context id |> function 
        | Some l -> ctx.WriteJsonAsync l
        | None -> (setStatusCode 404 >=> json "Book not deleted") next ctx