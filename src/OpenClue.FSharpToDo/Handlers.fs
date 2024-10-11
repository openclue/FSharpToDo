module OpenClue.FSharpToDo.Web.Handlers

open System.Security.Claims
open Marten
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.DependencyInjection
open OpenClue.FSharpToDo.Domain
open OpenClue.FSharpToDo.Domain.Features
open OpenClue.FSharpToDo.Persistence
open FsToolkit.ErrorHandling

type AppError =
    | BadRequest of string
    | NotFound of TodoId
    | InternalError of string

type Response =
    | Success of obj
    | Failure of AppError

let mapToAppError (todoError: TodoError) =
    match todoError with
    | InvalidCommand err -> BadRequest err
    | AlreadyExists id -> BadRequest $"Todo with id [{id}] already exists"
    | InvalidState err -> BadRequest err
    | TodoError.NotFound id -> NotFound id

// For future use - to get the command's initiator
let private getLoggedUserId (ctx: HttpContext) =
    ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value |> UserId.fromString

let private saveEvents store (id, events) =
    async {
        let! result = Repository.saveEvents store (id, events)

        return result |> Result.mapError InternalError
    }

let createTodoHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! result =
                asyncResult {
                    let store = ctx.RequestServices.GetRequiredService<IDocumentStore>()
                    let! dto = ctx.BindJsonAsync<CreateTodo.Dto>()
                    let! cmdResult = CreateTodo.handle dto |> Result.mapError mapToAppError
                    let! saveResult = saveEvents store cmdResult
                    
                    return saveResult
                }

            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err

            return! json response next ctx
        }
