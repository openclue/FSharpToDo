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


type TodoId = private Todo of Guid

module TodoId =

    let toString (Todo id) = id.ToString()

    let toGuid (Todo id) = id

    let fromGuid id =
        match id with
        | id when id = Guid.Empty -> "Empty Guid" |> Error
        | _ -> Todo id |> Ok


    let newId () = Guid.NewGuid() |> Todo

type UserId = private UserId of Guid

module UserId =
    let toGuid (UserId id) = id

    let fromGuid id =
        match id with
        | id when id = Guid.Empty -> "Empty Guid" |> Error
        | _ -> UserId id |> Ok

    let fromString (id: string) =
        try
            Guid.Parse id |> fromGuid
        with _ ->
            $"Invalid UserId: [{id}]" |> Error


type Decider<'C, 'E, 'S, 'ERR> =
    { decide: 'S -> 'C -> Result<'E list, 'ERR>
      evolve: 'S -> 'E -> 'S
      initialState: 'S
      isComplete: 'S -> bool }
