namespace OpenClue.FSharpToDo.Persistence


open System
open Marten.Events.Aggregation
open OpenClue.FSharpToDo.Domain

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

    member this.Apply (e: TodoEvent) (rm: TaskReadModel) =
        match e with
        | TodoCreated args ->
            { rm with
                Id = TodoId.toGuid args.Id
                TaskId = TodoId.toGuid args.Id
                AuthorId = UserId.toGuid args.Author
                Title = NonEmptyString.value args.Title
                AssigneeId = Option.None
                Status = "Unassigned"
                Priority = args.Priority.ToString()
                CompletedById = Option.None }
        | TodoAssigned args ->
            { rm with
                AssigneeId = UserId.toGuid args.Assignee |> Some
                Status = "Assigned" }

        | TodoUnassigned _ ->
            { rm with
                AssigneeId = Option.None
                Status = "Unassigned" }
        | TodoCompleted args ->
            { rm with
                CompletedById = UserId.toGuid args.CompletedBy |> Some
                Status = "Completed" }

module Repository =

    open Marten
    open Marten.Events.Projections
    open Weasel.Core

    let createStore (connectionString: string) =

        DocumentStore.For(fun options ->
            options.Connection(connectionString)
            options.Events.AddEventType(typeof<TodoEvent>)
            options.AutoCreateSchemaObjects <- AutoCreate.All
            options.Projections.Add<TaskReadModelProjection>(ProjectionLifecycle.Inline))

    let saveEvents (store: IDocumentStore) (taskId: TodoId, events: TodoEvent list) =
        async {
            let streamId = TodoId.toGuid taskId
            let eventsArray = events |> List.map box |> List.toArray

            try
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

                let events = events |> Seq.map (fun e -> e.Data :?> TodoEvent) |> Seq.toList

                return events |> Ok
            with _ ->
                return Error $"Failed to get events for Task {taskId}"
        }
