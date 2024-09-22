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
