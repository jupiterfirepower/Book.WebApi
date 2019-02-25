module Book.WebApi.DataAccess

open Book.WebApi.Models
open Microsoft.EntityFrameworkCore

type BooksContext(options : DbContextOptions<BooksContext>) = 
     inherit DbContext(options)
        
     override __.OnModelCreating modelBuilder = 
         let _ = modelBuilder.Entity<BookData>().HasKey(fun book -> (book.BookId) :> obj)
         modelBuilder.Entity<BookData>().Property(fun book -> book.BookId).ValueGeneratedOnAdd() |> ignore

         let _ = modelBuilder.Entity<User>().HasKey(fun user -> (user.UserId) :> obj)
         modelBuilder.Entity<User>().Property(fun user -> user.UserId).ValueGeneratedOnAdd() |> ignore

        
     [<DefaultValue>]
     val mutable books : DbSet<BookData>

     [<DefaultValue>]
     val mutable users : DbSet<User>
       
     member x.Books 
         with get () = x.books
         and set v = x.books <- v

     member x.Users 
         with get () = x.users
         and set v = x.users <- v

module BooksRepository = 
    let getAll (context : BooksContext) = context.Books

    let getAllUsers (context : BooksContext) = context.Users
    
    let getBook (context : BooksContext) id = context.Books |> Seq.tryFind (fun f -> f.BookId = id)
    
    let addBookAsync (context : BooksContext) (entity : BookData) = 
        async { 
            context.Books.AddAsync(entity) |> Async.AwaitTask |> ignore
            let! result = context.SaveChangesAsync true |> Async.AwaitTask
            return match result with
                   | x when x >= 1 -> Some(entity)
                   | _ -> None
        }
    
    let updateBook (context : BooksContext) (entity : BookData) (id : int) = 
        let current = context.Books.Find(id)
        let updated = { entity with BookId = id }
        context.Entry(current).CurrentValues.SetValues(updated)
        let result = match (context.SaveChanges true) with
                     | x when x >= 1 -> Some(updated)
                     | _ -> None
        result
    
    let deleteBook (context : BooksContext) (id : int) = 
        let current = context.Books.Find(id)
        context.Books.Remove(current) |> ignore
        if context.SaveChanges true >= 1 then Some(current)
        else None

let getAll = BooksRepository.getAll
let getAllUsers = BooksRepository.getAllUsers
let getBook = BooksRepository.getBook
let addBookAsync = BooksRepository.addBookAsync
let updateBook = BooksRepository.updateBook
let deleteBook = BooksRepository.deleteBook
