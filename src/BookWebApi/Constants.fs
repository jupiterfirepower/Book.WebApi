module Book.WebApi.Constants

[<Literal>]
let public appKeyIssuer = "Jwt:Issuer"

[<Literal>]
let public appKeyAudience = "Jwt:Audience"

[<Literal>]
let public appKeySecret = "Jwt:Secret"

[<Literal>]
let public appSettingsFileName = "appsettings.json"

[<Literal>]
let public sqliteDbConnectionString = @"Data Source=books.db"

[<Literal>]
let public sqliteDbConnectionStringInMemory = @"Data Source=:memory:;"


