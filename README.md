# BookWebApi

.net core 2.2, web api 2, F#, Giraffe

dotnet new -i "giraffe-template::*"

dotnet new giraffe -h to see all the options with the Giraffe template.
dotnet new giraffe -lang F# -V none -I true -o Book.WebApi


A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

## Build and test the application

### Windows

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

### Linux/macOS

Run the `build.sh` script in order to restore, build and test (if you've selected to include tests) the application:

```
$ ./build.sh
```

## Run the application

After a successful build you can start the web application by executing the following command in your terminal:

```
dotnet run -p src/BookWebApi
or go to directory src/BookWebApi and execute command: dotnet run
(before that use build.sh)
```

After the application has started visit [http://localhost:x](http://localhost:x) in your preferred browser.

