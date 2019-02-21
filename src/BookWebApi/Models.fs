module Book.WebApi.Models

[<CLIMutable>]
type BookData = 
    { 
        BookId: int32
        Author: string
        Title: string
        PublishedYear: int32
        Pages: int32 
    }

type Message =
    {
        Text: string
    }

[<CLIMutable>]
type TokenResult =
    {
        Token : string
    }

[<CLIMutable>]
type LoginModel =
    {
        Email : string
        Password : string
    }
    