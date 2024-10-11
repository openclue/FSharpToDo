module OpenClue.FSharpToDo.Tests.Todo.AssignTodoTests

open OpenClue.FSharpToDo.Tests.Shared
open OpenClue.FSharpToDo.Domain
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

let private todoId = createGuid () |> createTodoIdOrFail
let private author = createGuid () |> createUserIdOrFail
let private title = createNonEmptyStringOrFail "Assign todo tests"
let private priority = TodoPriority.High

let private unassignedTodo =
    Todo.Unassigned
        { Id = todoId
          Author = author
          Title = title
          Priority = priority }

let private assignedTodo =
    Todo.Assigned
        { Id = todoId
          Author = author
          Assignee = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

let private completedTodo =
    Todo.Completed
        { Id = todoId
          Author = author
          CompletedBy = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

[<Fact>]
let ``Given AssignTodoCommand and unassigned Todo When TodoDecider decide Then TodoAssignedEvent is created`` () =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TodoCommand.AssignTodo { Id = todoId; Assignee = assignee }
    let expectedEvent = TodoEvent.TodoAssigned { Id = todoId; Assignee = assignee }

    // Act
    let events = decideOrFail unassignedTodo cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent


[<Fact>]
let ``Given AssignTodoCommand and assigned Todo When TodoDecider decide Then TodoUnassignedEvent and TodoAssignedEvent are created``
    ()
    =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TodoCommand.AssignTodo { Id = todoId; Assignee = assignee }

    let expectedEvents =
        [ TodoEvent.TodoUnassigned { Id = todoId }
          TodoEvent.TodoAssigned { Id = todoId; Assignee = assignee } ]

    // Act
    let events = decideOrFail assignedTodo cmd

    // Assert
    List.length events |> should equal 2
    events |> should equal expectedEvents

[<Fact>]
let ``Given AssignTodoCommand and completed Todo When TodoDecider decide Then error is returned`` () =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TodoCommand.AssignTodo { Id = todoId; Assignee = assignee }

    // Act
    let result = decide completedTodo cmd

    // Assert
    result |> should be (ofCase <@ TodoCommandResult.Error @>)
