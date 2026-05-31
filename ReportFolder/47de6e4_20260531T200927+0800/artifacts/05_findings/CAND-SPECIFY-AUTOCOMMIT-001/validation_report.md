# Validation Report: CAND-SPECIFY-AUTOCOMMIT-001

## Finding

Spec Kit auto-commit stages the entire worktree with `git add .`.

## Candidate Metadata

- Candidate id: `CAND-SPECIFY-AUTOCOMMIT-001`
- Instance key: `speckit-auto-commit-git-add-dot`
- Ledger rows: `WR-0001`, `WR-0002`, `WR-0004`, `WR-0005`, `WR-0008`, `WR-0275`, `WR-0276`, `WR-0277`, `WR-0278`
- Root-control lines:
  - `.specify/extensions.yml:3`
  - `.specify/extensions/git/git-config.yml:13-14`
  - `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138`
  - `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160`
  - `.specify/extensions/git/scripts/bash/initialize-repo.sh:49-52`
  - `.specify/extensions/git/scripts/powershell/initialize-repo.ps1:56-62`

## Validation Rubric

- [x] Establish whether Spec Kit hooks can execute automatically from repository configuration.
- [x] Establish whether auto-commit is enabled by default or for relevant hook events.
- [x] Trace the exact staging sink and confirm whether it is scoped or whole-worktree.
- [x] Check countercontrols, especially `.gitignore`, that could defeat sensitive-file staging.
- [x] Calibrate whether the path is runtime-remote, local developer-only, or out of scope.

## Evidence Observed

- `.specify/extensions.yml:3` sets `auto_execute_hooks: true`, and the same file enables multiple `speckit.git.commit` before/after hooks.
- `.specify/extensions/git/git-config.yml:13-14` sets `auto_commit.default: true`; per-command entries also set `enabled: true`.
- `.specify/extensions/git/scripts/bash/auto-commit.sh:137` executes `git add .`, and line 138 commits the result.
- `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158` executes `git add .`, and line 160 commits the result.
- `.specify/extensions/git/scripts/bash/initialize-repo.sh:51` and `.specify/extensions/git/scripts/powershell/initialize-repo.ps1:60` use the same broad staging operation during repository initialization.
- `.gitignore:6-8` ignores `.env` and `.env*`; `.gitignore:41-42` ignores `bin/` and `obj/`; `.gitignore:74` ignores `artifacts/`. These are useful countercontrols but do not constrain staging to expected Spec Kit outputs.

## What Was Tested

No exploit PoC was run because executing the hook would intentionally create or stage repository changes, which conflicts with the scan rule not to modify code or repository state. Validation used focused static trace and repository counterevidence review instead.

## Remaining Uncertainty

The scan does not prove that a sensitive unignored file currently exists in this checkout, nor that a developer will push a generated auto-commit. The behavior is still supported by direct code evidence and can sweep future unignored sensitive files without an explicit review gate.

## Validation Closure Table

| Ledger row id | Instance key | Root-control file:line | Entrypoint/source | Sink/control | Disposition | Counterevidence or proof gap | Survives |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `WR-0001` | `speckit-auto-commit-git-add-dot` | `.specify/extensions.yml:3` | Spec Kit hook execution | `auto_execute_hooks: true` | reportable | Local developer-only path; still enables automatic hook execution. | yes |
| `WR-0004` | `speckit-auto-commit-git-add-dot` | `.specify/extensions/git/git-config.yml:13-14` | `speckit.git.commit` hook config | `auto_commit.default: true` | reportable | Per-command config also enables commit hooks; no review gate. | yes |
| `WR-0005` | `speckit-auto-commit-git-add-dot` | `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138` | Bash auto-commit hook | `git add .` then `git commit` | reportable | `.gitignore` blocks common files but not arbitrary unignored secrets. | yes |
| `WR-0275` | `speckit-auto-commit-git-add-dot` | `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160` | PowerShell auto-commit hook | `git add .` then `git commit` | reportable | Same counterevidence as Bash variant. | yes |
| `WR-0008` | `speckit-auto-commit-git-add-dot` | `.specify/extensions/git/scripts/bash/initialize-repo.sh:49-52` | Bash initialize command | `git init`, `git add .`, `git commit` | reportable | Runs only when not already in a Git repository; still broad-stages initial contents. | yes |
| `WR-0276` | `speckit-auto-commit-git-add-dot` | `.specify/extensions/git/scripts/powershell/initialize-repo.ps1:56-62` | PowerShell initialize command | `git init`, `git add .`, `git commit` | reportable | Runs only when not already in a Git repository; still broad-stages initial contents. | yes |

## Minimal Next Step

Limit staging to known Spec Kit output paths or replace automatic commit with a reviewed staging step that shows `git status --short` and requires explicit confirmation before `git add`.
