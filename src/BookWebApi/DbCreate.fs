namespace Book.WebApi

module DbCreator=
    
    open Microsoft.Data.Sqlite
    open Book.WebApi.BookStoreAccess
    open Book.WebApi.Models

    type BookData = { 
         BookId: int32;
         Author:string; 
         Title:string; 
         PublishedYear:int32;
         Pages:int32 }

    // Sample Data
    let books = [
        {  BookId = 0; Author = "Arnold"; Title = "Programming F#"; PublishedYear = 2017; Pages = 260 };
        {  BookId = 0; Author = "Arnold1"; Title = "Programming F#1"; PublishedYear = 2018; Pages = 365 };
        {  BookId = 0; Author = "Arnold2"; Title = "Programming F#2"; PublishedYear = 2016; Pages = 480 };
        {  BookId = 0; Author = "Arnold3"; Title = "Programming F#3"; PublishedYear = 2015; Pages = 260 };
        {  BookId = 0; Author = "Arnold4"; Title = "Programming F#4"; PublishedYear = 2016; Pages = 260 };
        {  BookId = 0; Author = "Arnold5"; Title = "Programming F#5"; PublishedYear = 2017; Pages = 260 };
        {  BookId = 0; Author = "Arnold6"; Title = "Programming F#6"; PublishedYear = 2018; Pages = 730 }; 
        {  BookId = 0; Author = "Arnold7"; Title = "Programming F#7"; PublishedYear = 2018; Pages = 730 }; 
        {  BookId = 0; Author = "Arnold8"; Title = "Programming F#8"; PublishedYear = 2018; Pages = 730 }; 
    ]

    type SqliteDbCreator() =
        member x.CreateDb = 
            let databaseFilename = "books.db"
            let connectionString = sprintf "Data Source=%s;" databaseFilename
            let connectionStringMemory = sprintf "Data Source=:memory:;" 
            let connection = new SqliteConnection(connectionString)
            connection.Open()

            let structureSql =
                "CREATE TABLE IF NOT EXISTS Books (" +
                "BookId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                "Author text NOT NULL, " +
                "Title  text NOT NULL, " + 
                "PublishedYear int NOT NULL, " + 
                "Pages int NOT NULL)"

            let dropTableSql = "DROP TABLE IF EXISTS Books;"

            let dropCommand = new SqliteCommand(dropTableSql, connection)
            dropCommand.ExecuteNonQuery() |> ignore

            let structureCommand = new SqliteCommand(structureSql, connection)
            structureCommand.ExecuteNonQuery() |> ignore
    
            // Add records
            let insertSql = 
                "insert into Books(Author, Title, PublishedYear, Pages) " + 
                "values (@author, @title, @publishedyear, @pages)"

            books
            |> List.map(fun x ->
               use command = new SqliteCommand(insertSql, connection)
               command.Parameters.AddWithValue("@author", x.Author) |> ignore
               command.Parameters.AddWithValue("@title", x.Title) |> ignore
               command.Parameters.AddWithValue("@publishedyear", x.PublishedYear) |> ignore
               command.Parameters.AddWithValue("@pages", x.Pages) |> ignore
               command.ExecuteNonQuery())
            |> List.sum
            |> (fun recordsAdded -> printfn "Records added: %d" recordsAdded)


            let book = { Models.BookData.BookId = 0; Author = "Arnold11"; Title = "Programming F#11"; PublishedYear = 2019; Pages = 820 }
            BookStoreAccess.addBook(book) |> ignore

            let book = { Models.BookData.BookId = 0; Author = "Arnold12"; Title = "Programming F#12"; PublishedYear = 2019; Pages = 820 }
            BookStoreAccess.addBook(book) |> ignore

            let book = { Models.BookData.BookId = 1; Author = "ArnoldUpdated"; Title = "Programming F# Updated"; PublishedYear = 2019; Pages = 820 }
            BookStoreAccess.updateBook(book) |> ignore

            BookStoreAccess.deleteBook(book) |> ignore

            let selectSql = "select * from Books order by Pages desc"
            let selectCommand = new SqliteCommand(selectSql, connection)
            let reader = selectCommand.ExecuteReader()
            while reader.Read() do
               printfn "%-7s %-19s %d [%d]" 
                  (reader.["Author"].ToString()) 
                  (reader.["Title"].ToString())
                  (System.Convert.ToInt32(reader.["PublishedYear"])) 
                  (System.Convert.ToInt32(reader.["Pages"]))

            connection.Close()

            

            

