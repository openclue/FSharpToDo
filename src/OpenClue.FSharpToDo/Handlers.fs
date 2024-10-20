module OpenClue.FSharpToDo.Web.Handlers

open System
open System.Security.Claims
open Marten
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.DependencyInjection
open OpenClue.FSharpToDo.Domain
open OpenClue.FSharpToDo.Web.Features

type Response =
    | Success of obj
    | Failure of AppError

// For future use - to get the command's initiator
let private getLoggedUserId (ctx: HttpContext) =
    ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value |> UserId.fromString

let getStore (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<IDocumentStore>()

let createTodoHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let store = getStore ctx
            let! cmdDto = ctx.BindJsonAsync<CreateTodoDto>()

            let! result = CreateTodo.handle store cmdDto

            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err

            return! json response next ctx
        }

let assignTodoHandler (todoId: Guid) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let store = getStore ctx
            let! cmdDto = ctx.BindJsonAsync<AssignTodoDto>()

            let! result = AssignTodo.handle store todoId cmdDto

            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err

            return! json response next ctx
        }
