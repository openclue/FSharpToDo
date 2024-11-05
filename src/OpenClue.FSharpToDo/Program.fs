module OpenClue.FSharpToDo.App

open Giraffe
open Marten
open Marten.Events.Daemon.Resiliency
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open OpenClue.FSharpToDo.Persistence
open OpenClue.FSharpToDo.Web.Handlers
open Serilog
open Serilog.Core
open Serilog.Events


let createLogger (builder: ILoggingBuilder) =
    let warningLevel = LoggingLevelSwitch(LogEventLevel.Warning)

    let logger =
        LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Npgsql.Command", warningLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger()

    builder.ClearProviders() |> ignore
    builder.AddSerilog(logger) |> ignore

let configureApp (app: IApplicationBuilder) = app.UseGiraffe Api.routes

let configureServices (context: WebHostBuilderContext) (services: IServiceCollection) =

    let connectionString = context.Configuration.GetConnectionString("Marten")

    services.AddSingleton<IDocumentStore>(Repository.createStore connectionString)
    |> ignore

    let martenBuilder = services.AddMarten(Repository.initMarten connectionString)
    martenBuilder.AddAsyncDaemon(DaemonMode.Solo) |> ignore

    services.AddGiraffe() |> ignore
    services.AddLogging(createLogger) |> ignore

let configureHost (builder: IWebHostBuilder) =
    builder.Configure(configureApp).ConfigureServices(configureServices) |> ignore

[<EntryPoint>]
let main _ =

    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(configureHost)
        .Build()
        .Run()

    0
