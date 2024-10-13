namespace OpenClue.FSharpToDo.Web.Features

open OpenClue.FSharpToDo.Domain
open OpenClue.FSharpToDo.Persistence
open FsToolkit.ErrorHandling

type AppError =
    | BadRequest of string
    | NotFound of TodoId
    | InternalError of string

type Response =
    | Success of obj
    | Failure of AppError

type CreateTodoDto =
    { Title: string
      AuthorId: string
      Priority: string }

type AssignTodoDto = { AssigneeId: string }

module Utils =

    let toAppError (todoError: TodoError) =
        match todoError with
        | AlreadyExists id -> BadRequest $"Todo with id [{id}] already exists"
        | InvalidState err -> BadRequest err
        | TodoError.NotFound id -> NotFound id

    let todoDecider = Todo.decider

    let mapStringToInternalError (err: string) = InternalError err

    let getTodoId todo =
        match todo with
        | None -> AppError.InternalError "Todo not found" |> Error
        | Assigned task -> Ok task.Id
        | Unassigned task -> Ok task.Id
        | Completed task -> Ok task.Id


    let saveEvents store (id, events) =
        async {
            let! result = Repository.saveEvents store (id, events)

            return result |> Result.mapError InternalError
        }

    let loadEvents store id =
        async {
            let! events = Repository.getEvents store id
            let result = events |> Result.mapError InternalError
            return result
        }

    let loadTodo store id : Async<Result<Todo, AppError>> =

        asyncResult {
            let! todoId = TodoId.fromGuid id |> Result.mapError mapStringToInternalError
            let! result = loadEvents store todoId
            let todo = List.fold todoDecider.evolve todoDecider.initialState result

            return todo
        }

[<RequireQualifiedAccess>]
module CreateTodo =

    open Utils

    let private buildCreateTodoCommand (dto: CreateTodoDto) =
        let cmd =
            result {
                let! priority = TodoPriority.tryParse dto.Priority
                and! author = UserId.fromString dto.AuthorId
                and! title = NonEmptyString.create dto.Title

                let createTaskArgs: CreateTodoArgs =
                    { Id = TodoId.newId ()
                      Author = author
                      Title = title
                      Priority = priority }

                return TodoCommand.CreateTodo createTaskArgs
            }

        match cmd with
        | Ok cmd -> Ok cmd
        | Error err -> BadRequest err |> Error

    let handle store cmdDto =
        asyncResult {
            let! cmd = buildCreateTodoCommand cmdDto
            let! todo, events = Todo.applyCommand Todo.None cmd |> Result.mapError toAppError
            let! id = getTodoId todo
            let! saveResult = saveEvents store (id, events)

            return saveResult
        }

[<RequireQualifiedAccess>]
module AssignTodo =

    open Utils

    let private buildAssignTodoCommand (dto: AssignTodoDto) =
        let cmd =
            result {
                let! assignee = UserId.fromString dto.AssigneeId

                let assignTodoArgs: AssignTodoArgs = { Assignee = assignee }

                return TodoCommand.AssignTodo assignTodoArgs
            }

        match cmd with
        | Ok cmd -> Ok cmd
        | Error err -> BadRequest err |> Error

    let handle store todoId cmdDto =
        asyncResult {
            let! todo = loadTodo store todoId
            let! cmd = buildAssignTodoCommand cmdDto
            let! todo, events = Todo.applyCommand todo cmd |> Result.mapError toAppError
            let! id = getTodoId todo
            let! saveResult = saveEvents store (id, events)

            return saveResult
        }
