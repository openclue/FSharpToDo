module OpenClue.FSharpToDo.App

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open OpenClue.FSharpToDo.Web.Handlers
open Serilog


let createLogger (builder: ILoggingBuilder) =
    let logger = LoggerConfiguration().WriteTo.Console().CreateLogger()
    builder.ClearProviders() |> ignore
    builder.AddSerilog(logger) |> ignore

let configureApp (app: IApplicationBuilder) = app.UseGiraffe Api.routes

let configureServices (services: IServiceCollection) =
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
