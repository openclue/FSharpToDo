module OpenClue.FSharpToDo.App

open Giraffe
open Microsoft.AspNetCore.Builder


[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()
    
    builder.Services.AddGiraffe() |> ignore
    
    let app = builder.Build()
    app.Run()
    
    0