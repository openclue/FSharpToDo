module OpenClue.FSharpToDo.Persistence.Repository

open Marten
open Marten.Events.Projections
open OpenClue.FSharpToDo.Persistence.Projections
open Weasel.Core
open OpenClue.FSharpToDo.Domain

let createStore (connectionString: string) =

    DocumentStore.For(fun options ->
        options.Connection(connectionString)
        options.Events.AddEventType(typeof<TaskEvent>)
        options.AutoCreateSchemaObjects <- AutoCreate.All
        options.Projections.Add<TaskReadModelProjection>(ProjectionLifecycle.Inline))
    

let saveEvents (store: IDocumentStore) (taskId: TaskId) (events: TaskEvent list) =
    let streamId = TaskId.toGuid taskId
    use session = store.LightweightSession()
    let eventsArray = events |> List.map box |> List.toArray
    session.Events.Append(streamId, eventsArray) |> ignore
    session.SaveChanges()
    
let getEvents (store: IDocumentStore) (taskId: TaskId) =
    let streamId = TaskId.toGuid taskId
    use session = store.QuerySession()
    session.Events.FetchStream(streamId)
    |> Seq.map (fun e -> e.Data:?> TaskEvent)
    |> Seq.toList

