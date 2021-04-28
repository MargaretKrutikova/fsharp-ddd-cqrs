module Api.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Microsoft.Extensions.Logging.Console

// ---------------------------------
// Web app
// ---------------------------------

let webApp () =
    choose [
        subRoute "/api/listings"
            (choose [   
                POST >=> choose [ route "/publish" >=> ApiHandlers.handlePublishListing ]
                POST >=> choose [ route "/queue" >=> ApiHandlers.handleQueueRequestListing ]
                POST >=> choose [ route "/return" >=> ApiHandlers.handleReturnListing ]
                POST >=> choose [ route "/borrow" >=> ApiHandlers.handleBorrowListing ]
                GET >=> choose [ route "/" >=> ApiHandlers.getPublishedListings ]
            ])
        subRoute "/api/user"
            (choose [
                GET >=> choose [ routef "/%O/listings" ApiHandlers.getUserListings ]
            ])
        setStatusCode 404 >=> text "Not Found"  ]
    
// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:5000")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let compose (container: IServiceProvider): CompositionRoot.CompositionRoot =
    let logger = container.GetRequiredService<ILogger<IStartup>>()
    // read app config to get db connections, message queue connections etc
    CompositionRoot.compose logger

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler))
        .UseCors(configureCors)
        .UseGiraffe(webApp ())

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

    services.AddSingleton<CompositionRoot.CompositionRoot>(compose)
    |> ignore

let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, true)
        .AddEnvironmentVariables()
    |> ignore

let configureLogging (context: WebHostBuilderContext) (logging: ILoggingBuilder) =
    logging.AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Information)
    |> ignore

type Startup() =
    member __.ConfigureServices(services: IServiceCollection) = configureServices services

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =
        configureApp app

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .ConfigureAppConfiguration(configureAppConfiguration)
                .ConfigureLogging(configureLogging)
                .UseStartup<Startup>()
            |> ignore)
        .Build()
        .Run()

    0
