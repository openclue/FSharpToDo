namespace OpenClue.FSharpToDo.Domain

type TodoPriority =
    | Low
    | Medium
    | High

module TodoPriority =
    let tryParse (value: string) =
        match value.ToLower() with
        | "low" -> Ok TodoPriority.Low
        | "medium" -> Ok TodoPriority.Medium
        | "high" -> Ok TodoPriority.High
        | _ -> Error $"Invalid TaskPriority value: [{value}]"

type UnassignedTodo =
    { Id: TodoId
      Author: UserId
      Title: NonEmptyString
      Priority: TodoPriority }

type AssignedTodo =
    { Id: TodoId
      Author: UserId
      Assignee: UserId
      Title: NonEmptyString
      Priority: TodoPriority }

type CompletedTodo =
    { Id: TodoId
      Author: UserId
      CompletedBy: UserId
      Title: NonEmptyString
      Priority: TodoPriority }

type Todo =
    | None
    | Unassigned of UnassignedTodo
    | Assigned of AssignedTodo
    | Completed of CompletedTodo



////// Commands //////////////////////////////////////////////////////////////////////////////////

type CreateTodoArgs =
    { Id: TodoId
      Author: UserId
      Title: NonEmptyString
      Priority: TodoPriority }

type AssignTodoArgs = { Assignee: UserId }

type UnassignTodoArgs = { Id: TodoId }

type CompleteTodoArgs = { Id: TodoId }

type TodoCommand =
    | CreateTodo of CreateTodoArgs
    | AssignTodo of AssignTodoArgs
    | UnassignTodo of UnassignTodoArgs
    | CompleteTodo of CompleteTodoArgs


////// Events //////////////////////////////////////////////////////////////////////////////////

type TodoCreatedArgs =
    { Id: TodoId
      Author: UserId
      Title: NonEmptyString
      Priority: TodoPriority }

type TodoAssignedArgs = { Id: TodoId; Assignee: UserId }

type TodoUnassignedArgs = { Id: TodoId }

type TodoCompletedArgs = { Id: TodoId; CompletedBy: UserId }

type TodoEvent =
    | TodoCreated of TodoCreatedArgs
    | TodoAssigned of TodoAssignedArgs
    | TodoUnassigned of TodoUnassignedArgs
    | TodoCompleted of TodoCompletedArgs

type TodoError =
    | AlreadyExists of TodoId
    | NotFound of TodoId
    | InvalidState of string
    | InvalidCommand of string
    
type TodoCommandResult = Result<TodoEvent list, TodoError>

module Todo =

    //// Command handlers //////////////////////////////////////////////////////////////////////
    let private handleCreateTaskCommand (cmd: CreateTodoArgs) state =
        match state with
        | None ->
            [ TodoEvent.TodoCreated
                  { Id = cmd.Id
                    Author = cmd.Author
                    Title = cmd.Title
                    Priority = cmd.Priority } ]
            |> Ok
        | _ -> TodoError.AlreadyExists cmd.Id |> Error

    let private handleAssignTaskCommand (cmd: AssignTodoArgs) state =
        match state with
        | Unassigned task ->
            [ TodoEvent.TodoAssigned
                  { Id = task.Id
                    Assignee = cmd.Assignee } ]
            |> Ok
        | Assigned task ->
            [ TodoEvent.TodoUnassigned { Id = task.Id }
              TodoEvent.TodoAssigned
                  { Id = task.Id
                    Assignee = cmd.Assignee } ]
            |> Ok
        | None -> TodoError.InvalidState "Todo not initialized" |> Error
        | _ -> TodoError.InvalidState "Todo cannot be assigned in its current state" |> Error

    let private handleUnassignTaskCommand (cmd: UnassignTodoArgs) state =
        match state with
        | Assigned task -> [ TodoEvent.TodoUnassigned { Id = task.Id } ] |> Ok
        | None -> TodoError.NotFound cmd.Id |> Error
        | _ -> TodoError.InvalidState "Todo is already unassigned" |> Error

    let private handleCompleteTaskCommand (cmd: CompleteTodoArgs) state =
        match state with
        | Assigned task ->
            [ TodoEvent.TodoCompleted
                  { Id = task.Id
                    CompletedBy = task.Assignee } ]
            |> Ok
        | Unassigned task ->
            [ TodoEvent.TodoCompleted
                  { Id = task.Id
                    CompletedBy = task.Author } ]
            |> Ok
        | None -> TodoError.NotFound cmd.Id |> Error
        | Completed _ -> TodoError.InvalidState "Todo is already completed" |> Error

    let private decide state command =
        match command with
        | CreateTodo c -> handleCreateTaskCommand c state
        | AssignTodo c -> handleAssignTaskCommand c state
        | UnassignTodo c -> handleUnassignTaskCommand c state
        | CompleteTodo c -> handleCompleteTaskCommand c state

    //// Event handlers //////////////////////////////////////////////////////////////////////
    let private createTask (event: TodoCreatedArgs) =
        Todo.Unassigned
            { Id = event.Id
              Author = event.Author
              Title = event.Title
              Priority = event.Priority }

    let private assignTask (event: TodoAssignedArgs) (task: UnassignedTodo) =
        Todo.Assigned
            { Id = task.Id
              Author = task.Author
              Assignee = event.Assignee
              Title = task.Title
              Priority = task.Priority }

    let private unassignTask (event: TodoUnassignedArgs) (task: AssignedTodo) =
        Todo.Unassigned
            { Id = task.Id
              Author = task.Author
              Title = task.Title
              Priority = task.Priority }

    let private completeAssignedTask (event: TodoCompletedArgs) (task: AssignedTodo) =
        Todo.Completed
            { Id = task.Id
              Author = task.Author
              CompletedBy = event.CompletedBy
              Title = task.Title
              Priority = task.Priority }

    let private completeUnassignedTask (event: TodoCompletedArgs) (t: UnassignedTodo) =
        Todo.Completed
            { Id = t.Id
              Author = t.Author
              CompletedBy = event.CompletedBy
              Title = t.Title
              Priority = t.Priority }

    let private evolve state event =
        match state, event with
        | None, TodoEvent.TodoCreated e -> createTask e
        | Todo.Unassigned task, TodoEvent.TodoAssigned e -> assignTask e task
        | Todo.Assigned task, TodoEvent.TodoUnassigned e -> unassignTask e task
        | Todo.Assigned task, TodoEvent.TodoCompleted e -> completeAssignedTask e task
        | Todo.Unassigned task, TodoEvent.TodoCompleted e -> completeUnassignedTask e task
        | _ -> state

    let decider =
        { decide = decide
          evolve = evolve
          initialState = Todo.None
          isComplete =
            function
            | Todo.Completed _ -> true
            | _ -> false }

    