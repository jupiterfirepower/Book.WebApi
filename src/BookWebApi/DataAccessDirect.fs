namespace Book.WebApi

open System.IO
open Book.WebApi.Models
open Microsoft.Data.Sqlite
open NPoco

module BookStoreAccess =
    let private connString = sprintf "Data Source=%s;" (Path.Combine(Directory.GetCurrentDirectory(), "books.db"))

    let addBook (book: BookData) =
        let connectionStringBuilder = new SqliteConnectionStringBuilder();
        connectionStringBuilder.DataSource <- "./books.db";
        
        use conn = new SqliteConnection(connectionStringBuilder.ConnectionString)
        conn.Open()
        
        use txn: SqliteTransaction = conn.BeginTransaction()

        let cmd = conn.CreateCommand()
        cmd.Transaction <- txn
        cmd.CommandText <- @"insert into Books(Author, Title, PublishedYear, Pages) " + 
                           "values (@author, @title, @publishedyear, @pages)"

        cmd.Parameters.AddWithValue("@author", book.Author) |> ignore
        cmd.Parameters.AddWithValue("@title", book.Title) |> ignore
        cmd.Parameters.AddWithValue("@publishedyear", book.PublishedYear) |> ignore
        cmd.Parameters.AddWithValue("@pages", book.Pages) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        txn.Commit()

    let updateBook (book: BookData) =
        use conn = new SqliteConnection(connString)
        conn.Open()
        
        use txn: SqliteTransaction = conn.BeginTransaction()

        let cmd = conn.CreateCommand()
        cmd.Transaction <- txn
        cmd.CommandText <- @"update Books set Author=@author, Title=@title, PublishedYear=@publishedyear, Pages= @pages where BookId=@bookId"

        cmd.Parameters.AddWithValue("@author", book.Author) |> ignore
        cmd.Parameters.AddWithValue("@title", book.Title) |> ignore
        cmd.Parameters.AddWithValue("@publishedyear", book.PublishedYear) |> ignore
        cmd.Parameters.AddWithValue("@pages", book.Pages) |> ignore
        cmd.Parameters.AddWithValue("@bookId", book.BookId) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        txn.Commit()
     
    let deleteBook (book: BookData) =
        use conn = new SqliteConnection(connString)
        conn.Open()
        
        use txn: SqliteTransaction = conn.BeginTransaction()

        let cmd = conn.CreateCommand()
        cmd.Transaction <- txn
        cmd.CommandText <- @"delete from Books where BookId=@bookId"

        cmd.Parameters.AddWithValue("@bookId", book.BookId) |> ignore

        cmd.ExecuteNonQuery() |> ignore

        txn.Commit()

    let removeBookById (id: int) =
        let book = { BookId = id; Author = "empty"; Title = "empty"; PublishedYear = 2010; Pages = 100 }
        deleteBook(book)

    let getBooks () =
        let query = "select * from Books"  
        use conn = new SqliteConnection(connString)
        conn.Open()

        use db = new Database(conn)
        db.Fetch<BookData>(query)