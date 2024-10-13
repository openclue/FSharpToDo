module OpenClue.FSharpToDo.Web.Handlers

open System
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

let todoDecider = Todo.decider

let mapStringToInternalError (err: string) = InternalError err

// For future use - to get the command's initiator
let private getLoggedUserId (ctx: HttpContext) =
    ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value |> UserId.fromString

let getStore (ctx: HttpContext) =
    ctx.RequestServices.GetRequiredService<IDocumentStore>()

let private saveEvents store (id, events) =
    async {
        let! result = Repository.saveEvents store (id, events)

        return result |> Result.mapError InternalError
    }

let private loadEvents store id =
    async {
        let! events = Repository.getEvents store id
        let result = events |> Result.mapError InternalError
        return result
    }

let private loadTodo store id : Async<Result<Todo, AppError>> =

    asyncResult {
        let! todoId = TodoId.fromGuid id |> Result.mapError mapStringToInternalError
        let! result = loadEvents store todoId
        let todo = List.fold todoDecider.evolve todoDecider.initialState result
        
        return todo
    }

let createTodoHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! result =
                asyncResult {
                    let store = getStore ctx
                    let! cmdDto = ctx.BindJsonAsync<CreateTodo.Dto>()
                    let! cmdResult = CreateTodo.handle cmdDto |> Result.mapError mapToAppError
                    let! saveResult = saveEvents store cmdResult
                    
                    return saveResult
                }

            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err

            return! json response next ctx
        }

let assignTodoHandler (todoId: Guid) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! result =
                asyncResult {
                    let store = getStore ctx
                    let! cmdDto = ctx.BindJsonAsync<AssignTodo.Dto>()
                    let! todo = loadTodo store todoId 
                    let! cmdResult = AssignTodo.handle todo cmdDto |> Result.mapError mapToAppError
                    let! saveResult = saveEvents store cmdResult
                    
                    return saveResult
                }
            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err
            
            return! json response next ctx
        }