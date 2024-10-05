module OpenClue.FSharpToDo.Tests.Task.AssignTaskTests

open OpenClue.FSharpToDo.Tests.Shared
open OpenClue.FSharpToDo.Domain
open Xunit
open FsUnit.Xunit

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
let ``Given AssignTaskCommand and unassigned Task When TaskDecider decide Then TaskAssignedEvent is created`` () =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TaskCommand.AssignTask { Id = taskId; Assignee = assignee }
    let expectedEvent = TaskEvent.TaskAssigned { Id = taskId; Assignee = assignee }

    // Act
    let events = decideOrFail unassignedTask cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent


[<Fact>]
let ``Given AssignTaskCommand and assigned Task When TaskDecider decide Then TaskUnassignedEvent and TaskAssignedEvent are created``
    ()
    =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TaskCommand.AssignTask { Id = taskId; Assignee = assignee }

    let expectedEvents =
        [ TaskEvent.TaskUnassigned { Id = taskId }
          TaskEvent.TaskAssigned { Id = taskId; Assignee = assignee } ]

    // Act
    let events = decideOrFail assignedTask cmd

    // Assert
    List.length events |> should equal 2
    events |> should equal expectedEvents

[<Fact>]
let ``Given AssignTaskCommand and completed Task When TaskDecider decide Then error is returned`` () =
    // Arrange
    let assignee = createGuid () |> createUserIdOrFail
    let cmd = TaskCommand.AssignTask { Id = taskId; Assignee = assignee }

    // Act
    let result = decide completedTask cmd

    // Assert
    result |> shouldBeError
