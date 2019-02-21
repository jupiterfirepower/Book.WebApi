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
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open Microsoft.Extensions.Configuration;
open Book.WebApi.Constants

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name : string) = 
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let model = { Text = greetings }
    json model

let webApp = 
    choose [ GET >=> choose [ route "/" >=> indexHandler "world"
                              routex "/books(/?)" >=> booksHandler
                              route "/sbooks" >=> authorize >=> booksHandler
                              routef "/book/%i" bookHandler
                              routef "/sbook/%i" (fun id -> authorize >=> bookHandler id)
                            ]
             POST >=> choose [
                        route "/book" >=> bookAddHandler
                        route "/token" >=> handlePostToken
                      ]  
             PUT >=> routef "/book/%i" (fun id -> authorize >=> bookUpdateHandler id) 
             DELETE >=> routef "/book/%i" (fun id -> authorize >=> bookDeleteHandler id)  
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
    builder.WithOrigins("http://localhost:8080")
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
        .UseAuthentication()
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    config
        .AddJsonFile(appSettingsFileName,false,true)
        //.AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName ,true)
        .AddEnvironmentVariables() |> ignore

let authenticationOptions (o : AuthenticationOptions) =
    o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
    o.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme

let configureServices (services : IServiceCollection) = 
    let env = services.BuildServiceProvider().GetService<IHostingEnvironment>();

    services.AddDbContext<BooksContext>
        (fun (options : DbContextOptionsBuilder) -> 
        match env.IsEnvironment(envTests) with
        | true -> options.UseSqlite(sqliteDbConnectionStringInMemory) |> ignore
        | false -> options.UseSqlite(sqliteDbConnectionString) |> ignore)
    |> ignore

    let configuration = ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(appSettingsFileName).Build()
    printfn "%s" (configuration.GetSection(appKeyIssuer).Value)

    //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    services.AddAuthentication(authenticationOptions)
        .AddJwtBearer(fun options ->
            options.TokenValidationParameters <- TokenValidationParameters(
                ValidateActor = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration.GetSection(appKeyIssuer).Value,
                ValidAudience = configuration.GetSection(appKeyAudience).Value,
                IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection(appKeySecret).Value)))
            ) |> ignore

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
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0