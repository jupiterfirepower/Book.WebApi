module Book.WebApi.Handlers

open Book.WebApi.Models
open Book.WebApi.DataAccess
open Book.WebApi.RequestModels
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open System
open System.Text
open System.Security.Claims
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.Extensions.Configuration
open Book.WebApi.Constants

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


let authorize: HttpHandler = requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let generateToken email issuer audience (secret:string) =
    let claims = [|
        Claim(JwtRegisteredClaimNames.Email, email);
        Claim(JwtRegisteredClaimNames.Sub, email);
        Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]

    let expires = Nullable(DateTime.UtcNow.AddHours(1.0))
    let notBefore = Nullable(DateTime.UtcNow)
    let securityKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    let signingCredentials = SigningCredentials(key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)

    let token =
        JwtSecurityToken(
            issuer = issuer,
            audience = audience,
            claims = claims,
            expires = expires,
            notBefore = notBefore,
            signingCredentials = signingCredentials)

    let tokenResult = {
        Token = JwtSecurityTokenHandler().WriteToken(token)
    }

    tokenResult

let handleGetSecured =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let email = ctx.User.FindFirst ClaimTypes.NameIdentifier
            
        text ("User " + email.Value + " is authorized to access this resource.") next ctx

let handlePostToken =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! model = ctx.BindJsonAsync<LoginModel>()

            // authenticate user
            let configuration = ctx.GetService<IConfiguration>()
            
            let tokenResult = generateToken model.Email (configuration.GetSection(appKeyIssuer).Value) (configuration.GetSection(appKeyAudience).Value) (configuration.GetSection(appKeySecret).Value)

            return! json tokenResult next ctx
        }