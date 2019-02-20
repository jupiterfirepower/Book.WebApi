module Book.WebApi.RequestModels
open Book.WebApi.Models

type CreateUpdateBookRequest =
    {
        BookId: int32
        Author: string 
        Title: string 
        PublishedYear: int32
        Pages: int32
    }

    member this.HasErrors =
        if this.Author = null || this.Author = "" then Some "Author is required"
        else if this.Title = null || this.Title = "" then Some "Title is required"
        else if this.Author.Length > 255  then Some "Author is too long"
        else if this.Title.Length > 255  then Some "Title is too long"
        else if this.PublishedYear <= 0 then Some "PublishedYear must be greater than zero"
        else if this.Pages <= 0 then Some "Pages must be greater than zero"
        else None

    member this.GetBook = {     
                                BookData.BookId = this.BookId;
                                Author = this.Author;
                                Title = this.Title;
                                PublishedYear = this.PublishedYear;
                                Pages = this.Pages
                           }