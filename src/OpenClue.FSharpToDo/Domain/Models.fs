namespace OpenClue.FSharpToDo.Domain

type TaskPriority =
    | Low
    | Medium
    | High

type UnassignedTask =
    { Id: TaskId
      Author: UserId
      Title: NonEmptyString
      Priority: TaskPriority }

type AssignedTask =
    { Id: TaskId
      Author: UserId
      Assignee: UserId
      Title: NonEmptyString
      Priority: TaskPriority }

type CompletedTask =
    { Id: TaskId
      Author: UserId
      CompletedBy: UserId
      Title: NonEmptyString
      Priority: TaskPriority }

type Task =
    | None
    | Unassigned of UnassignedTask
    | Assigned of AssignedTask
    | Completed of CompletedTask



////// Commands //////////////////////////////////////////////////////////////////////////////////

type CreateTaskArgs =
    { Id: TaskId
      Author: UserId
      Title: NonEmptyString
      Priority: TaskPriority }

type AssignTaskArgs = { Id: TaskId; Assignee: UserId }

type UnassignTaskArgs = { Id: TaskId }

type CompleteTaskArgs = { Id: TaskId }

type TaskCommand =
    | CreateTask of CreateTaskArgs
    | AssignTask of AssignTaskArgs
    | UnassignTask of UnassignTaskArgs
    | CompleteTask of CompleteTaskArgs


////// Events //////////////////////////////////////////////////////////////////////////////////

type TaskCreatedArgs =
    { Id: TaskId
      Author: UserId
      Title: NonEmptyString
      Priority: TaskPriority }

type TaskAssignedArgs = { Id: TaskId; Assignee: UserId }

type TaskUnassignedArgs = { Id: TaskId }

type TaskCompletedArgs = { Id: TaskId; CompletedBy: UserId }

type TaskEvent =
    | TaskCreated of TaskCreatedArgs
    | TaskAssigned of TaskAssignedArgs
    | TaskUnassigned of TaskUnassignedArgs
    | TaskCompleted of TaskCompletedArgs
