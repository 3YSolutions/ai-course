# AGENTS.md

## Working rules

- Test command: `dotnet test` from the repository root. Run the whole solution. `dotnet format --verify-no-changes` must also pass; CI runs both.
- Do not run the AppHost. It needs a container runtime this worktree does not have. `dotnet build` and `dotnet test` are enough to verify a change here.
- Always work on a feature branch, never on `main`.
- Before exiting a heartbeat: commit, `git push -u origin HEAD`, `gh pr create --fill --base main`.
- Paste the PR URL in your hand-over comment and hand over (status `done`). Paperclip sends it to review.
- Never `--no-verify`. Never force-push a shared branch. Never `gh pr merge --admin`.

## Architecture and structure:

- Vertical slices: one folder per feature under src/Store.Api/<Feature>/ (endpoints, catalog, store, rules). Tests mirror it under tests/Store.Api.Tests/<Feature>/.
- Keep the blast radius inside the feature. Do not add or change shared base classes (GenericRepository, BaseService) to solve a single feature's need.
- Naming by role: <Service>Client for an external service, <Entity>Store for persistence, <Entity>Catalog for a feature that coordinates reads, writes and cache state.
- No new project unless the code is used by more than one module and you can state what the project boundary separates. Say it in the PR.
- Shared touch points (Program.cs, Directory.Build.props, ServiceDefaults) are listed in the PR description when touched.