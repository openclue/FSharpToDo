namespace OpenClue.FSharpToDo.Infrastructure.Dto

open System
open OpenClue.FSharpToDo.Domain
open FsToolkit.ErrorHandling

type CreateTodoCommandDto =
    { TodoId: Guid
      AuthorId: Guid
      Title: string
      Priority: string }

type TodoCreatedEventDto =
    { TodoId: Guid
      AuthorId: Guid
      Title: string
      Priority: string }

type TodoAssignedEventDto = { TodoId: Guid; AssigneeId: Guid }

type TodoUnassignedEventDto = { TodoId: Guid }

type TodoCompletedEventDto = { TodoId: Guid; CompletedById: Guid }

type TodoEventDto =
    | TodoCreatedDto of TodoCreatedEventDto
    | TodoAssignedDto of TodoAssignedEventDto
    | TodoUnassignedDto of TodoUnassignedEventDto
    | TodoCompletedDto of TodoCompletedEventDto

module EventDto =

    let fromDomainEvent (event: TodoEvent) =
        match event with
        | TodoCreated args ->
            { TodoId = TodoId.toGuid args.Id
              AuthorId = UserId.toGuid args.Author
              Title = NonEmptyString.value args.Title
              Priority = args.Priority.ToString() }
            |> TodoCreatedDto
        | TodoAssigned args ->
            { TodoId = TodoId.toGuid args.Id
              AssigneeId = UserId.toGuid args.Assignee }
            |> TodoAssignedDto
        | TodoUnassigned args -> { TodoId = TodoId.toGuid args.Id } |> TodoUnassignedDto
        | TodoCompleted args ->
            { TodoId = TodoId.toGuid args.Id
              CompletedById = UserId.toGuid args.CompletedBy }
            |> TodoCompletedDto

    let toDomainEvent (dto: TodoEventDto) =
        match dto with
        | TodoCreatedDto args ->
            result {
                let! todoId = TodoId.fromGuid args.TodoId
                and! authorId = UserId.fromGuid args.AuthorId
                and! title = NonEmptyString.create args.Title
                and! priority = TodoPriority.tryParse args.Priority

                return
                    TodoCreated
                        { Id = todoId
                          Author = authorId
                          Title = title
                          Priority = priority }
            }
        | TodoAssignedDto args ->
            result {
                let! todoId = TodoId.fromGuid args.TodoId
                and! assigneeId = UserId.fromGuid args.AssigneeId
                return TodoAssigned { Id = todoId; Assignee = assigneeId }
            }
        | TodoUnassignedDto args ->
            result {
                let! todoId = TodoId.fromGuid args.TodoId
                return TodoUnassigned { Id = todoId }
            }
        | TodoCompletedDto args ->
            result {
                let! todoId = TodoId.fromGuid args.TodoId
                and! completedBy = UserId.fromGuid args.CompletedById

                return
                    TodoCompleted
                        { Id = todoId
                          CompletedBy = completedBy }
            }
