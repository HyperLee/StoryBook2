# Security Review: StoryBook2

## Scope

- 掃描範圍：整個 `/Users/qiuzili/StoryBook2` repository，commit `47de6e4`。
- 掃描模式：Codex Security repository-wide scan。
- 掃描輸出：`/Users/qiuzili/StoryBook2/ReportFolder/47de6e4_20260531T200927+0800/`。
- 主要檢視資產：`StoryBook/` ASP.NET Core Razor Pages runtime、`StoryBook.Tests/` 測試、`StoryBook/Data/` catalog JSON、`StoryBook/wwwroot/` static assets、`.specify/` 與 `.agents/` local tooling、`specs/` 規格文件。
- 明確排除或降級處理：`bin/`、`obj/`、`artifacts/010-merge/` 產物列入 worklist 但以 generated/build artifact 關閉；vendored Bootstrap/jQuery 以 dependency/static-asset 風險檢視。
- Phase 1 threat model 是本次掃描依 repository 程式碼與專案文件產生，路徑為 `artifacts/01_context/threat_model.md`。
- Baseline verification：`dotnet test StoryBook2.sln` 通過，總計 204 tests passed。第一次 sandboxed 執行因 MSBuild IPC permission denied 失敗，改以核准的 elevated execution 完成。
- Dependency advisory verification：`dotnet list StoryBook2.sln package --vulnerable --include-transitive` 透過 `https://api.nuget.org/v3/index.json` 查詢，`StoryBook` 與 `StoryBook.Tests` 沒有回報 vulnerable NuGet packages。
- Validation limitation：未實際執行 Spec Kit auto-commit PoC，因為該 PoC 會 stage/commit repository 變更，與本次不得修改程式碼或 repository state 的限制衝突；採 focused static trace 與 counterevidence review。
- Final worktree check：`git status --short` 顯示 `ReportFolder/` scan artifacts，以及 `dotnet test` 造成的三個已追蹤 `StoryBook/obj/Debug/net10.0/*` generated file diffs；未修改應用 source code，且未 staging 或 commit 任何檔案。

### Scan Summary

| Field | Value |
| --- | --- |
| Reportable findings | 1 |
| Severity mix | 1 low |
| Confidence mix | 1 high |
| Worklist coverage | 274 deterministic rows plus 4 manual adjacency rows closed in `work_ledger.jsonl` |
| Validation mode | Static source/control/sink trace, repository counterevidence review, baseline tests, NuGet advisory check |
| Final Markdown | `/Users/qiuzili/StoryBook2/ReportFolder/47de6e4_20260531T200927+0800/report.md` |
| Final HTML | `/Users/qiuzili/StoryBook2/ReportFolder/47de6e4_20260531T200927+0800/report.html` |

## Threat Model

### Overview

StoryBook2 is a single ASP.NET Core Razor Pages web application that serves bilingual child-friendly nature storybook content. The runtime product lives primarily under `StoryBook/`; `StoryBook.Tests/`, `specs/`, `.specify/`, and `.agents/` are development, specification, and tooling assets. The app renders server-side Razor Pages, reads local JSON catalogs from `StoryBook/Data/`, serves static images and first-party JavaScript/CSS from `StoryBook/wwwroot/`, and vendors Bootstrap/jQuery under `StoryBook/wwwroot/lib/`.

Public runtime routes include `/`, `/Privacy`, `/Error`, `/dinosaurs`, `/dinosaurs/{slug}`, `/aquarium`, `/aquarium/{slug}`, `/explore`, `/compare`, `/journeys`, `/journeys/{slug}`, `/passport`, and `/quiz`. The application does not define login, accounts, database access, external API calls, file uploads, administrative mutation endpoints, or server-side persistence of user choices.

### Trust Boundaries And Assumptions

- Public browser to Razor Pages: request paths, route values, query strings, form fields, headers, and cookies are attacker-controlled.
- Server app to local catalog files: `ContentPath` values are operator/developer-controlled configuration, not public user input.
- Repository content to rendered HTML: JSON catalog text, alt text, slugs, and image paths cross into HTML, attributes, and JavaScript-enhanced UI.
- Browser storage to client scripts: `localStorage` keys such as `storybook.language`, `storybook.theme`, and reading-passport state are fully user-controlled and must be validated or safely ignored.
- Static assets and vendored libraries are part of the application distribution; dependency and supply-chain issues are relevant when they have a reachable use in this app.
- Spec Kit Git automation is a local developer/tooling surface. It is not a remote runtime web surface, but it can affect repository history and accidental sensitive-data exposure.

### Relevant Attacker Stories

- A remote unauthenticated visitor manipulates route slugs, query parameters, or quiz form fields to trigger unexpected server behavior, information disclosure, or unsafe rendering.
- A remote visitor tampers with localStorage values to test client-side trust assumptions, DOM injection paths, or state confusion.
- A malicious or compromised content contributor commits unsafe JSON content that later renders into HTML, attributes, links, or client-side state.
- An operator misconfigures catalog `ContentPath` to an unintended absolute path or sensitive file and then exposes data through runtime rendering or logs.
- A dependency or vendored static library contains a known vulnerability relevant to how the app uses it.
- A developer or automation workflow runs Spec Kit Git hooks while unreviewed local files are present, creating a commit that may later be pushed or shared.

### Existing Controls

- Razor Pages and Tag Helpers provide default HTML and attribute encoding for normal model values.
- Catalog file loading uses local application content paths by default, not request-selected file paths.
- Services and validators enforce slug, image path, bilingual text, content length, and fallback rules across catalog areas.
- Client-side localStorage state is used for convenience and is not trusted as a server-side security boundary.
- `Program.cs` enables HTTPS redirection and HSTS outside Development.
- `.gitignore` excludes common sensitive/generated paths such as `.env*`, `bin/`, `obj/`, and `artifacts/`.

## Findings

| # | Finding | Severity | Confidence |
| --- | --- | --- | --- |
| 1 | [Spec Kit 自動提交會以 git add . 暴露未預期檔案](#1-spec-kit-自動提交會以-git-add--暴露未預期檔案) | low | high |

### Confidence Scale

| Label | Meaning |
| --- | --- |
| high | direct source, configuration, or runtime evidence supports the finding, with no material unresolved reachability or exploitability blocker. |
| medium | source evidence supports a plausible issue, but runtime behavior, deployment configuration, role reachability, type constraints, or exploit reliability still need proof. |
| low | weak or incomplete evidence; include only when the user explicitly wants follow-up candidates in the final report. |

### [1] Spec Kit 自動提交會以 git add . 暴露未預期檔案

| Field | Value |
| --- | --- |
| Severity | low |
| Confidence | high |
| Confidence rationale | 直接的設定與 script 證據證明 auto hooks 和 auto-commit 預設啟用且使用 `git add .`；實際 secret exposure 未重現，因為觸發 PoC 會修改 repository state。 |
| Category | Sensitive data exposure / unsafe auto-staging in local developer tooling |
| CWE | CWE-200: Exposure of Sensitive Information to an Unauthorized Actor |
| Affected lines | `.specify/extensions.yml:3`; `.specify/extensions/git/git-config.yml:13-14`; `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138`; `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160`; `.specify/extensions/git/scripts/bash/initialize-repo.sh:49-52`; `.specify/extensions/git/scripts/powershell/initialize-repo.ps1:56-62` |

#### Summary

Spec Kit Git hooks are configured to execute automatically and auto-commit is enabled by default. The auto-commit scripts then run `git add .` before committing, which stages the entire worktree instead of only known Spec Kit outputs. If a developer has an unignored local secret, export, diagnostic file, or generated artifact in the repository, a Spec Kit workflow can sweep it into Git history without an explicit review step.

This is not a remote ASP.NET Core runtime vulnerability. It is a low severity local tooling issue because the path requires a developer or automation workflow and a sensitive file that is not already excluded by `.gitignore`.

#### Validation

Validation used focused static trace and counterevidence review.

- [x] `.specify/extensions.yml:3` sets `auto_execute_hooks: true`.
- [x] `.specify/extensions/git/git-config.yml:13-14` sets `auto_commit.default: true`, with per-command enabled entries following it.
- [x] `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138` stages all files with `git add .` and then commits.
- [x] `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160` performs the same broad staging and commit flow.
- [x] `.gitignore` blocks common paths such as `.env*`, `bin/`, `obj/`, and `artifacts/`, but does not restrict staging to expected Spec Kit paths or enforce a review gate.

Remaining uncertainty: no current unignored secret was found, and the hook was not executed because doing so would stage or commit repository changes. The vulnerable behavior itself is directly evidenced by tracked configuration and scripts.

#### Dataflow

Spec Kit workflow execution -> `.specify/extensions.yml` permits automatic hook execution -> `.specify/extensions/git/git-config.yml` enables auto-commit -> `auto-commit.sh` or `auto-commit.ps1` runs `git add .` -> `git commit` persists all staged files -> a later push/share can expose unintended local content.

The initialize scripts provide the same broad-staging pattern for a new repository: initialize command -> `git init` -> `git add .` -> initial commit.

#### Reachability

The reachable actor is a local developer or automation account running Spec Kit workflows, not a remote web visitor. Preconditions are plausible in development: hooks are enabled, auto-commit is enabled, and an unignored sensitive or unintended file exists in the workspace. The impact crosses from local workspace content into Git history; it becomes externally visible if the auto-created commit is pushed or shared.

Counterevidence lowers the likelihood: `.gitignore` excludes the most common local secret and build-output patterns, and no current secret file was found. That counterevidence does not defeat the issue because `git add .` still stages any other unignored path and commits without a scoped allowlist.

#### Severity

Final severity is low. The potential impact can include sensitive data exposure through Git history, but exploitation is local/developer-only, requires an unignored file, and has partial `.gitignore` mitigation. Evidence of an attacker-controlled remote trigger or an actual committed secret would raise severity; an allowlisted staging implementation plus enforced secret scanning would lower or eliminate it.

#### Remediation

Use a scoped staging strategy instead of `git add .`. Minimal safe options include:

- Set `auto_commit.default` to `false` and require explicit opt-in per command.
- Stage only expected Spec Kit paths, for example relevant `specs/`, `.specify/`, and generated checklist/task files.
- Print `git status --short` and require explicit confirmation before staging.
- Add a pre-commit secret scan or policy check that fails closed before the commit is created.
- Add a small test or shellcheck-style assertion that auto-commit scripts do not contain broad `git add .` staging.

## Reviewed Surfaces

| Surface | Risk Area | Outcome | Notes |
| --- | --- | --- | --- |
| `StoryBook/Services/` and `StoryBook/Models/` | Catalog loading, validation, search projections, DTO parsing | No issue found | Full shard review found no public request-controlled path to file reads, deserialization RCE, command/query injection, or unsafe content projection. |
| `StoryBook/Pages/`, `Program.cs`, appsettings, launch settings | Razor routes, handlers, antiforgery, hosting configuration | No issue found | Razor encoding and normal form protections hold for observed paths; no auth boundary or sensitive state mutation exists. |
| `StoryBook/wwwroot/js/`, `StoryBook/wwwroot/css/`, `StoryBook/Data/` | DOM XSS, localStorage, static asset/data constraints | Rejected | LocalStorage is not trusted as a server security boundary; JS uses safe DOM APIs; data paths and slugs are constrained. |
| `StoryBook/wwwroot/lib/` and NuGet packages | Dependency/advisory risk | No issue found | NuGet vulnerable-package check returned no advisories; vendored assets are not dynamically fetched and no vulnerable reachable feature was identified. |
| `.specify/` and `.agents/skills/speckit-*` | Local tooling, auto-commit, secret exposure risk | Reported | `CAND-SPECIFY-AUTOCOMMIT-001` reports broad auto-staging in Spec Kit Git workflows. |
| `StoryBook.Tests/`, `specs/`, generated `bin/obj` and `artifacts/010-merge/` rows | Test/spec/generated source review | Not applicable | Reviewed for reusable security patterns or tooling risk; generated/test-only rows are not runtime attack surface. |
