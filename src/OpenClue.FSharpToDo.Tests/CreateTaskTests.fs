module OpenClue.FSharpToDo.Tests.Task.CreateTaskTests

open FsToolkit.ErrorHandling
open OpenClue.FSharpToDo.Domain
open System
open FsUnit.Xunit
open Xunit


let decider = TaskDecider.create ()
let taskGuid = Guid.NewGuid()
let authorGuid = Guid.NewGuid()

let buildCreateTaskArgs title priority =
    match
        result {
            let! id = taskGuid |> TaskId.fromGuid
            let! author = authorGuid |> UserId.fromGuid
            let! title = title |> NonEmptyString.create
            let priority = priority

            let args: CreateTaskArgs =
                { Id = id
                  Author = author
                  Title = title
                  Priority = priority }

            return args
        }
    with
    | Ok args -> args
    | Error err -> failwith err

let buildExpectedEvent (cmdArgs: CreateTaskArgs) =
    TaskEvent.TaskCreated
        { Id = cmdArgs.Id
          Author = cmdArgs.Author
          Title = cmdArgs.Title
          Priority = cmdArgs.Priority }

let decide createTaskCommand =
    match (decider.decide decider.initialState createTaskCommand) with
    | Ok events -> events
    | Error err -> failwith err


[<Fact>]
let ``Given valid CreateTask command When TaskDecider decide Then TaskCreated event is created`` () =
    // Arrange
    let cmdArgs = buildCreateTaskArgs "Title" TaskPriority.Low
    let expectedEvent = buildExpectedEvent cmdArgs
    let cmd = TaskCommand.CreateTask cmdArgs

    // Act
    let events = decide cmd

    // Assert
    List.length events |> should equal 1
    List.head events |> should equal expectedEvent
