module OpenClue.FSharpToDo.Tests.Task.CreateTaskTests

open OpenClue.FSharpToDo.Domain
open FsUnit.Xunit
open Xunit
open OpenClue.FSharpToDo.Tests.Shared


let taskId = createGuid () |> createTaskIdOrFail
let author = createGuid () |> createUserIdOrFail
let title = createNonEmptyStringOrFail "Test task"
let priority = TodoPriority.High


[<Fact>]
let ``Given valid CreateTaskCommand When TaskDecider decide Then TaskCreatedEvent is created`` () =
    // Arrange
    let cmd =
        TodoCommand.CreateTodo
            { Id = taskId
              Author = author
              Title = title
              Priority = priority }

    let expectedEvent =
        TodoEvent.TodoCreated
            { Id = taskId
              Author = author
              Title = title
              Priority = priority }

    // Act
    let events = decideOrFail decider.initialState cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent

[<Fact>]
let ``Given TaskCreatedEvent When TaskDecider evolve Then Task state is Unassigned`` () =
    // Arrange
    let event =
        TodoEvent.TodoCreated
            { Id = taskId
              Author = author
              Title = title
              Priority = priority }

    let expectedState =
        Todo.Unassigned
            { Id = taskId
              Author = author
              Title = title
              Priority = priority }

    // Act
    let newState = decider.evolve decider.initialState event

    // Assert
    newState |> should equal expectedState
