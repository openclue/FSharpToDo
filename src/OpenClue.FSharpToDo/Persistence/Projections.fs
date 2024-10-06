module OpenClue.FSharpToDo.Persistence.Projections


open System
open Marten.Events.Aggregation
open OpenClue.FSharpToDo.Domain

[<CLIMutable>]
type TaskReadModel =
    { TaskId: Guid
      AuthorId: Guid
      Title: string
      AssigneeId: Guid option
      Status: string
      Priority: string
      CompletedById: Guid option }

type TaskReadModelProjection() =
    inherit SingleStreamProjection<TaskReadModel>()

    member this.Apply (e: TaskEvent) (rm: TaskReadModel) =
        match e with
        | TaskCreated args ->
            { rm with
                TaskId = TaskId.toGuid args.Id
                AuthorId = UserId.toGuid args.Author
                Title = NonEmptyString.value args.Title
                AssigneeId = Option.None
                Status = "Unassigned"
                Priority = args.Priority.ToString()
                CompletedById = Option.None }
        | TaskAssigned args ->
            { rm with
                AssigneeId = UserId.toGuid args.Assignee |> Some
                Status = "Assigned" }

        | TaskUnassigned _ ->
            { rm with
                AssigneeId = Option.None
                Status = "Unassigned" }
        | TaskCompleted args ->
            { rm with
                CompletedById = UserId.toGuid args.CompletedBy |> Some
                Status = "Completed" }
