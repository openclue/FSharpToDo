module OpenClue.FSharpToDo.Domain.Features

open FsToolkit.ErrorHandling


let handleCommand (state: Todo) (cmd: TodoCommand) = Todo.decider.decide state cmd

let private handleEvents (state: Todo) (events: TodoEvent list) =
    
    let state = List.fold Todo.decider.evolve state events
    
    match state with
    | None -> TodoError.InvalidState "Todo have no state" |> Error
    | Assigned task -> Ok (task.Id, events)
    | Unassigned task -> Ok (task.Id, events)
    | Completed task -> Ok (task.Id, events)
    
    
[<RequireQualifiedAccess>] 
module CreateTask =
    
    type Dto =
        { Title: string
          AuthorId: string
          Priority: string }
    
    let private buildCreateTaskCommand (dto: Dto) =
        let cmd =
            result {
            let! priority = TodoPriority.tryParse dto.Priority
            and! author = UserId.fromString dto.AuthorId
            and! title = NonEmptyString.create dto.Title

            let createTaskArgs: CreateTodoArgs =
                { Id = TodoId.newId ()
                  Author = author
                  Title = title
                  Priority = priority }
                
            return TodoCommand.CreateTodo createTaskArgs
        }
        match cmd with
        | Ok cmd -> Ok cmd
        | Error err -> TodoError.InvalidCommand err |> Error

    let handle dto =
        dto
        |> buildCreateTaskCommand
        |> Result.bind (handleCommand Todo.decider.initialState)
        |> Result.bind (handleEvents Todo.decider.initialState)


