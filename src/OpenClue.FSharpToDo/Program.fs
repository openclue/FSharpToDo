module OpenClue.FSharpToDo.App

open Giraffe
open Microsoft.AspNetCore.Builder
open OpenClue.FSharpToDo.Web

let routes : HttpHandler =
    choose [
        POST >=> route "/todos" >=> Handlers.createTodoHandler
        POST >=> routef "/todos/%O/assign" Handlers.assignTodoHandler
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()
    app.UseGiraffe routes
    app.Run()

    0
