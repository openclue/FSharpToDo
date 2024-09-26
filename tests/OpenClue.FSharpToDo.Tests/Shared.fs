module OpenClue.FSharpToDo.Tests.Shared

open OpenClue.FSharpToDo.Domain
open System

let unwrapOrFail result =
    match result with
    | Ok value -> value
    | Error err -> failwith err

let decider = TaskDecider.create ()
let createGuid () = Guid.NewGuid()
let createTaskIdOrFail id = id |> TaskId.fromGuid |> unwrapOrFail
let createUserIdOrFail id = id |> UserId.fromGuid |> unwrapOrFail

let createNonEmptyStringOrFail value =
    value |> NonEmptyString.create |> unwrapOrFail

let decide state command = decider.decide state command

let decideOrFail state command =
    decider.decide state command |> unwrapOrFail
