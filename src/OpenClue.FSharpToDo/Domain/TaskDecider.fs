module OpenClue.FSharpToDo.Domain.TaskDecider

open OpenClue.FSharpToDo.Domain

let private handleCreateTaskCommand (cmd: CreateTaskArgs) state =
    match state with
    | None ->
        [ TaskEvent.TaskCreated
              { Id = cmd.Id
                Author = cmd.Author
                Title = cmd.Title
                Priority = cmd.Priority } ]
        |> Ok
    | _ -> "Task already exists" |> Error


//// Command handlers //////////////////////////////////////////////////////////////////////
let private handleAssignTaskCommand (cmd: AssignTaskArgs) state =
    match state with
    | Unassigned task ->
        [ TaskEvent.TaskAssigned
              { Id = task.Id
                Assignee = cmd.Assignee } ]
        |> Ok
    | Assigned task ->
        [ TaskEvent.TaskUnassigned { Id = task.Id }
          TaskEvent.TaskAssigned
              { Id = task.Id
                Assignee = cmd.Assignee } ]
        |> Ok
    | None -> "Task does not exist" |> Error
    | _ -> "Task cannot be assigned in its current state" |> Error

let private handleUnassignTaskCommand (cmd: UnassignTaskArgs) state =
    match state with
    | Assigned task -> [ TaskEvent.TaskUnassigned { Id = task.Id } ] |> Ok
    | None -> "Task does not exist" |> Error
    | _ -> "Task cannot be unassigned in its current state" |> Error

let private handleCompleteTaskCommand (cmd: CompleteTaskArgs) state =
    match state with
    | Assigned task ->
        [ TaskEvent.TaskCompleted
              { Id = task.Id
                CompletedBy = task.Assignee } ]
        |> Ok
    | Unassigned task ->
        [ TaskEvent.TaskCompleted
              { Id = task.Id
                CompletedBy = task.Author } ]
        |> Ok
    | None -> "Task does not exist" |> Error
    | _ -> "Task cannot be completed in its current state" |> Error

let private decide state command =
    match command with
    | CreateTask c -> handleCreateTaskCommand c state
    | AssignTask c -> handleAssignTaskCommand c state
    | UnassignTask c -> handleUnassignTaskCommand c state
    | CompleteTask c -> handleCompleteTaskCommand c state


//// Event handlers //////////////////////////////////////////////////////////////////////
let private createTask (event: TaskCreatedArgs) =
    Task.Unassigned
        { Id = event.Id
          Author = event.Author
          Title = event.Title
          Priority = event.Priority }

let private assignTask (event: TaskAssignedArgs) (task: UnassignedTask) =
    Task.Assigned
        { Id = task.Id
          Author = task.Author
          Assignee = event.Assignee
          Title = task.Title
          Priority = task.Priority }

let private unassignTask (event: TaskUnassignedArgs) (task: AssignedTask) =
    Task.Unassigned
        { Id = task.Id
          Author = task.Author
          Title = task.Title
          Priority = task.Priority }

let private completeAssignedTask (event: TaskCompletedArgs) (task: AssignedTask) =
    Task.Completed
        { Id = task.Id
          Author = task.Author
          CompletedBy = event.CompletedBy
          Title = task.Title
          Priority = task.Priority }


let private completeUnassignedTask (event: TaskCompletedArgs) (t: UnassignedTask) =
    Task.Completed
        { Id = t.Id
          Author = t.Author
          CompletedBy = event.CompletedBy
          Title = t.Title
          Priority = t.Priority }

let private evolve state event =
    match state, event with
    | None, TaskEvent.TaskCreated e -> createTask e
    | Task.Unassigned task, TaskEvent.TaskAssigned e -> assignTask e task
    | Task.Assigned task, TaskEvent.TaskUnassigned e -> unassignTask e task
    | Task.Assigned task, TaskEvent.TaskCompleted e -> completeAssignedTask e task
    | Task.Unassigned task, TaskEvent.TaskCompleted e -> completeUnassignedTask e task
    | _ -> state


//// Task Decider //////////////////////////////////////////////////////////////////////
let create () =
    { decide = decide
      evolve = evolve
      initialState = Task.None
      isComplete =
        function
        | Task.Completed _ -> true
        | _ -> false }
