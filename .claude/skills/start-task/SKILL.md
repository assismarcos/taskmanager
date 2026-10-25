---
name: start-task
description: Starts a backlog task from docs/tasks by its ID, following Test-Driven Development and the task's Definition of Done
disable-model-invocation: true
argument-hint: <task-id>
---

Open the file in `docs/tasks/` whose name starts with $ARGUMENTS.

1. Check that every task listed under "Depends on" is already merged into `main`
   (`git log --oneline main`). If one is missing, stop and tell me which.
2. Summarize the goal, the Scope (files you may change), the Out of scope list and the tests to write first.
3. Ask me to confirm before writing any code.
4. After confirmation, create the branch `feat/<task-id>-<slug>` and follow the `tdd-cycle` skill.
5. Finish with the Definition of Done checklist from the task file, each item ticked or explained.
