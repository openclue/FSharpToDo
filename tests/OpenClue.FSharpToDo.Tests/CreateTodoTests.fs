module OpenClue.FSharpToDo.Tests.Todo.CreateTodoTests

open OpenClue.FSharpToDo.Domain
open FsUnit.Xunit
open Xunit
open OpenClue.FSharpToDo.Tests.Shared


let todoId = createGuid () |> createTodoIdOrFail
let author = createGuid () |> createUserIdOrFail
let title = createNonEmptyStringOrFail "Test todo"
let priority = TodoPriority.High


[<Fact>]
let ``Given valid CreateTodoCommand When TodoDecider decide Then TodoCreatedEvent is created`` () =
    // Arrange
    let cmd =
        TodoCommand.CreateTodo
            { Id = todoId
              Author = author
              Title = title
              Priority = priority }

    let expectedEvent =
        TodoEvent.TodoCreated
            { Id = todoId
              Author = author
              Title = title
              Priority = priority }

    // Act
    let events = decideOrFail decider.initialState cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent

[<Fact>]
let ``Given TodoCreatedEvent When TodoDecider evolve Then Todo state is Unassigned`` () =
    // Arrange
    let event =
        TodoEvent.TodoCreated
            { Id = todoId
              Author = author
              Title = title
              Priority = priority }

    let expectedState =
        Todo.Unassigned
            { Id = todoId
              Author = author
              Title = title
              Priority = priority }

    // Act
    let newState = decider.evolve decider.initialState event

    // Assert
    newState |> should equal expectedState
