module OpenClue.FSharpToDo.Web.Handlers

open System
open System.Net
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

type ErrorDto = { Message: string }

// For future use - to get the command's initiator
let private getLoggedUserId (ctx: HttpContext) =
    ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value |> UserId.fromString

let getStore (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<IDocumentStore>()

let setStatusCode (code: HttpStatusCode) (ctx: HttpContext) = ctx.Response.StatusCode <- (int code)

let handleError (err: AppError) (next: HttpFunc) (ctx: HttpContext) =
    match err with
    | AppError.NotFound id ->
        setStatusCode HttpStatusCode.NotFound ctx
        json { Message = $"Todo with id [{id}] not found" } next ctx
    | AppError.BadRequest msg ->
        setStatusCode HttpStatusCode.BadRequest ctx
        json { Message = msg } next ctx
    | AppError.InternalError _ ->
        setStatusCode HttpStatusCode.InternalServerError ctx
        json { Message = "Internal server error" } next ctx

let createTodoHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let store = getStore ctx
            let! cmdDto = ctx.BindJsonAsync<CreateTodoDto>()

            let! result = CreateTodo.handle store cmdDto

            return!
                match result with
                | Ok id ->
                    setStatusCode HttpStatusCode.Created ctx
                    json id next ctx
                | Error err -> handleError err next ctx

        }

let assignTodoHandler (todoId: Guid) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let store = getStore ctx
            let! cmdDto = ctx.BindJsonAsync<AssignTodoDto>()

            let! result = AssignTodo.handle store todoId cmdDto

            return!
                match result with
                | Ok id ->
                    setStatusCode HttpStatusCode.OK ctx
                    json id next ctx
                | Error err -> handleError err next ctx
        }
