# Attack Path Analysis: CAND-SPECIFY-AUTOCOMMIT-001

## Title

Spec Kit auto-commit stages the entire worktree with `git add .`.

## Affected Lines

- `.specify/extensions.yml:3` - enables automatic hook execution.
- `.specify/extensions/git/git-config.yml:13-14` - enables auto-commit by default.
- `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138` - stages all files and commits.
- `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160` - stages all files and commits.
- `.specify/extensions/git/scripts/bash/initialize-repo.sh:49-52` - initializes and stages all files.
- `.specify/extensions/git/scripts/powershell/initialize-repo.ps1:56-62` - initializes and stages all files.

## Attack Path Steps

1. A developer has an unignored local file in the repository, such as an ad-hoc credentials file, local export, generated report, or other sensitive material not covered by `.gitignore`.
2. The developer runs a Spec Kit workflow with hooks enabled.
3. `.specify/extensions.yml` allows hooks to run automatically, and `.specify/extensions/git/git-config.yml` enables auto-commit by default.
4. The Bash or PowerShell auto-commit script executes `git add .` and then commits all staged content.
5. The sensitive or unintended file can be included in the commit and later pushed or shared with the repository history.

## Attack Path Facts

- Assumptions: The path requires local developer workflow execution and an unignored sensitive or unintended file. No current secret file was found during the scan.
- Context: The affected component is development tooling, not public ASP.NET Core runtime request handling.
- In-scope status: In scope for this repository-wide scan because `.specify` and `.agents` files are tracked tooling used by project workflows.
- Exposure: Local developer machine or automation context. No public port, ingress, or browser endpoint reaches this path.
- Identity: Runs with the developer or automation account privileges that execute Spec Kit commands.
- Cross-boundary behavior: The boundary is local workspace content to Git history. It can become cross-boundary if the auto-created commit is pushed or shared.
- Vector: `localhost` / local developer workflow.
- Preconditions: Spec Kit hook execution must occur, auto-commit must be enabled, and an unintended unignored file must be present.
- Attacker input control: No direct remote attacker input is proven. Risk is accidental exposure or social/operational misuse of local tooling.
- Category: Sensitive data exposure through unsafe auto-staging in developer tooling.
- Mitigations already present: `.gitignore` excludes `.env*`, `bin/`, `obj/`, and `artifacts/`; no current committed secret was found.
- Auth scope: Developer-only.
- Impact surface: Build/development workflow and Git history.
- Target reach: Single repository/workspace.
- Secrets references: No concrete secret file was found. The finding concerns future or local unignored sensitive files.
- Counterevidence: `.gitignore` blocks common sensitive and generated paths, and the path is not remotely reachable. This lowers severity but does not defeat the broad-staging behavior.
- Blindspots: The scan did not execute Spec Kit hooks or inspect developer-local files outside the repository.
- Controls: No allowlist of expected files, no pre-commit secret scanning gate, and no explicit review step before broad staging.
- Confidence: High for behavior, lower for occurrence of actual secret exposure.

## Severity Calibration

Impact is medium if sensitive files are accidentally committed, but the likelihood is low because the path is local, developer-only, and partly mitigated by `.gitignore`. Applying the policy matrix yields a low final severity rather than medium or high. The finding remains reportable because it is a tracked repository workflow that can create persistent Git history exposure without a review gate.

## Final Policy Decision

Reportable, low severity, high confidence in the code behavior. It should not be presented as a remote runtime vulnerability.
