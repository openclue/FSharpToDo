namespace OpenClue.FSharpToDo.Persistence

open System
open System.Collections.Generic
open FsToolkit.ErrorHandling
open Marten.Events.Aggregation
open Microsoft.AspNetCore.Identity
open OpenClue.FSharpToDo.Domain
open OpenClue.FSharpToDo.Infrastructure.Dto

[<CLIMutable>]
type TaskReadModel =
    { Id: Guid
      TaskId: Guid
      AuthorId: Guid
      Title: string
      AssigneeId: Guid option
      Status: string
      Priority: string
      CompletedById: Guid option }

type TaskReadModelProjection() =
    inherit SingleStreamProjection<TaskReadModel>()

    member this.Apply (e: obj) (rm: TaskReadModel) =
        match e with
        | :? TodoCreatedEventDto as evt ->
            { rm with
                Id = evt.TodoId
                TaskId = evt.TodoId
                AuthorId = evt.AuthorId
                Title = evt.Title
                AssigneeId = Option.None
                Status = "Unassigned"
                Priority = evt.Priority
                CompletedById = Option.None }
        | :? TodoAssignedEventDto as evt ->
            { rm with
                AssigneeId = evt.AssigneeId |> Some
                Status = "Assigned" }

        | :? TodoUnassignedEventDto as _ ->
            { rm with
                AssigneeId = Option.None
                Status = "Unassigned" }
        | :? TodoCompletedEventDto as evt ->
            { rm with
                CompletedById = evt.CompletedById |> Some
                Status = "Completed" }
        | _ -> rm

module Repository =

    open Marten
    open Marten.Events.Projections
    open Weasel.Core

    let boxDto (dto: TodoEventDto) =
        match dto with
        | TodoCreatedDto args -> box args
        | TodoAssignedDto args -> box args
        | TodoUnassignedDto args -> box args
        | TodoCompletedDto args -> box args

    let unboxDto (dto: obj) : TodoEventDto option =
        match dto with
        | :? TodoCreatedEventDto as args -> TodoCreatedDto args |> Some
        | :? TodoAssignedEventDto as args -> TodoAssignedDto args |> Some
        | :? TodoUnassignedEventDto as args -> TodoUnassignedDto args |> Some
        | :? TodoCompletedEventDto as args -> TodoCompletedDto args |> Some
        | _ -> Option.None

    let combineErrors (errors: string list) = errors |> String.concat "; "

    let mapEvents (iEvents: IReadOnlyList<Events.IEvent>) =
        iEvents
        |> Seq.cast<Events.IEvent>
        |> Seq.map (fun e -> e.Data |> unboxDto)
        |> Seq.choose id
        |> Seq.map EventDto.toDomainEvent
        |> Seq.toList
        |> List.traverseResultA id
        |> Result.mapError combineErrors


    let createStore (connectionString: string) =
        DocumentStore.For(connectionString) :> IDocumentStore

    let initMarten (connectionString: string) (opts: StoreOptions) =
        opts.Connection connectionString
        opts.AutoCreateSchemaObjects <- AutoCreate.All
        opts.Projections.Add<TaskReadModelProjection>(ProjectionLifecycle.Inline)

    let saveEvents (store: IDocumentStore) (taskId: TodoId, events: TodoEvent list) =
        async {
            let streamId = TodoId.toGuid taskId

            try
                let eventsArray =
                    events |> List.map EventDto.fromDomainEvent |> List.map boxDto |> List.toArray

                eventsArray |> printfn "Events: %A"

                use session = store.LightweightSession()
                session.Events.Append(streamId, eventsArray) |> ignore
                do! session.SaveChangesAsync() |> Async.AwaitTask
                return Ok taskId
            with _ ->
                return Error "Failed to save events"
        }

    let getEvents (store: IDocumentStore) (taskId: TodoId) =
        async {
            let streamId = TodoId.toGuid taskId

            try
                use session = store.QuerySession()
                let! events = session.Events.FetchStreamAsync(streamId) |> Async.AwaitTask

                return events |> mapEvents
            with _ ->
                return Error $"Failed to get events for Task {taskId}"
        }
