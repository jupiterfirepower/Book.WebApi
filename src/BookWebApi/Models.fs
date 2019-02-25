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

[<CLIMutable>]
type User = { 
         UserId: int32
         Name:string 
         Password:string 
         Email:string 
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
    