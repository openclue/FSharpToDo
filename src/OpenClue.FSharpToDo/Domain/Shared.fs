namespace OpenClue.FSharpToDo.Domain

open System

type NonEmptyString = private NonEmptyString of string

module NonEmptyString =
    let create value =
        match value with
        | null -> "Null value" |> Error
        | "" -> "Empty string" |> Error
        | _ -> NonEmptyString value |> Ok

    let value (NonEmptyString value) = value


type TaskId = private TaskId of Guid

module TaskId =
    let toGuid (TaskId id) = id

    let fromGuid id =
        match id with
        | id when id = Guid.Empty -> "Empty Guid" |> Error
        | _ -> TaskId id |> Ok


type UserId = private UserId of Guid

module UserId =
    let toGuid (UserId id) = id

    let fromGuid id =
        match id with
        | id when id = Guid.Empty -> "Empty Guid" |> Error
        | _ -> UserId id |> Ok
        
