module Book.WebApi.App

open Book.WebApi.DataAccess
open Book.WebApi.Handlers
open Book.WebApi.Models
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO
open Book.WebApi.DbCreator

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) = 
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model = { Text = greetings }
    json model

let webApp = 
    choose [ GET >=> choose [ route "/" >=> indexHandler "world"
                              route "/books" >=> booksHandler
                              routef "/book/%i" bookHandler ]
             POST >=> route "/book" >=> bookAddHandler
             PUT >=> routef "/book/%i" bookUpdateHandler
             DELETE >=> routef "/book/%i" bookDeleteHandler
             setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8081")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) = 
    let env = services.BuildServiceProvider().GetService<IHostingEnvironment>();

    services.AddDbContext<BooksContext>
        (fun (options : DbContextOptionsBuilder) -> 
        match env.IsEnvironment("Test") with
        | true -> options.UseSqlite(@"Data Source=:memory:;") |> ignore
        | false -> options.UseSqlite(@"Data Source=books.db") |> ignore)
    |> ignore
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
//----------------------------------------------------------------------
    // initialize db file, table - Books structure and insert test data
    let db = SqliteDbCreator()
    db.CreateDb
//----------------------------------------------------------------------
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0