module OpenClue.FSharpToDo.Tests.Task.UnassignTaskTests

open OpenClue.FSharpToDo.Tests.Shared
open OpenClue.FSharpToDo.Domain
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

let private taskId = createGuid () |> createTaskIdOrFail
let private author = createGuid () |> createUserIdOrFail
let private title = createNonEmptyStringOrFail "Assign task tests"
let private priority = TodoPriority.High

let private unassignedTask =
    Todo.Unassigned
        { Id = taskId
          Author = author
          Title = title
          Priority = priority }

let private assignedTask =
    Todo.Assigned
        { Id = taskId
          Author = author
          Assignee = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

let private completedTask =
    Todo.Completed
        { Id = taskId
          Author = author
          CompletedBy = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

[<Fact>]
let ``Given UnassignTaskCommand and assigned Task When TaskDecider decide Then TaskUnassignedEvent is created`` () =
    // Arrange
    let cmd = TodoCommand.UnassignTodo { Id = taskId }
    let expectedEvent = TodoEvent.TodoUnassigned { Id = taskId }

    // Act
    let events = decideOrFail assignedTask cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent

[<Fact>]
let ``Given UnassignTaskCommand and unassigned Task When TaskDecider decide Then error is returned`` () =
    // Arrange
    let cmd = TodoCommand.UnassignTodo { Id = taskId }

    // Act
    let result = decide unassignedTask cmd

    // Assert
    result |> should be (ofCase <@ TodoCommandResult.Error @>)
