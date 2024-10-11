module OpenClue.FSharpToDo.Tests.Todo.UnassignTodoTests

open OpenClue.FSharpToDo.Tests.Shared
open OpenClue.FSharpToDo.Domain
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

let private todoId = createGuid () |> createTodoIdOrFail
let private author = createGuid () |> createUserIdOrFail
let private title = createNonEmptyStringOrFail "Assign task tests"
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
let ``Given UnassignTodoCommand and assigned Todo When TodoDecider decide Then TodoUnassignedEvent is created`` () =
    // Arrange
    let cmd = TodoCommand.UnassignTodo { Id = todoId }
    let expectedEvent = TodoEvent.TodoUnassigned { Id = todoId }

    // Act
    let events = decideOrFail assignedTodo cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent

[<Fact>]
let ``Given UnassignTodoCommand and unassigned Todo When TodoDecider decide Then error is returned`` () =
    // Arrange
    let cmd = TodoCommand.UnassignTodo { Id = todoId }

    // Act
    let result = decide unassignedTodo cmd

    // Assert
    result |> should be (ofCase <@ TodoCommandResult.Error @>)
