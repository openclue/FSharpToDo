module OpenClue.FSharpToDo.Tests.Task.UnassignTaskTests

open OpenClue.FSharpToDo.Domain.TaskDecider
open OpenClue.FSharpToDo.Tests.Shared
open OpenClue.FSharpToDo.Domain
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers

let private taskId = createGuid () |> createTaskIdOrFail
let private author = createGuid () |> createUserIdOrFail
let private title = createNonEmptyStringOrFail "Assign task tests"
let private priority = TaskPriority.High

let private unassignedTask =
    Task.Unassigned
        { Id = taskId
          Author = author
          Title = title
          Priority = priority }

let private assignedTask =
    Task.Assigned
        { Id = taskId
          Author = author
          Assignee = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

let private completedTask =
    Task.Completed
        { Id = taskId
          Author = author
          CompletedBy = createGuid () |> createUserIdOrFail
          Title = title
          Priority = priority }

[<Fact>]
let ``Given UnassignTaskCommand and assigned Task When TaskDecider decide Then TaskUnassignedEvent is created`` () =
    // Arrange
    let cmd = TaskCommand.UnassignTask { Id = taskId }
    let expectedEvent = TaskEvent.TaskUnassigned { Id = taskId }

    // Act
    let events = decideOrFail assignedTask cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent

[<Fact>]
let ``Given UnassignTaskCommand and unassigned Task When TaskDecider decide Then error is returned`` () =
    // Arrange
    let cmd = TaskCommand.UnassignTask { Id = taskId }

    // Act
    let result = decide unassignedTask cmd

    // Assert
    result |> should be (ofCase <@ TaskCommandResult.Error @>)
