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

let mapTodoError (todoError: TodoError) =
    match todoError with
    | InvalidCommand err -> BadRequest err
    | AlreadyExists id -> BadRequest $"Todo with id [{id}] already exists"
    | InvalidState err -> BadRequest err
    | TodoError.NotFound id -> NotFound id

let getLoggedUserId (ctx: HttpContext) =
    ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value |> UserId.fromString

let saveEvents store (id, events) =
    async {
        let! result = Repository.saveEvents store (id, events)
        
        return result |> Result.mapError InternalError
    }
    

let createTodoAsync store commandDto : Async<Result<TodoId, AppError>> =
    asyncResult {
        let! crateResult = CreateTask.handle commandDto |> Result.mapError mapTodoError
        let! saveResult = saveEvents store crateResult
        return saveResult
    }


let createTaskHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let store = ctx.RequestServices.GetRequiredService<IDocumentStore>()
            let! dto = ctx.BindJsonAsync<CreateTask.Dto>()
            
            let! result = createTodoAsync store dto
            
            let response =
                match result with
                | Ok id -> Success id
                | Error err -> Failure err
            
            return! json response next ctx
        }
