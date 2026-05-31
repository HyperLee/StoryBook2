# Finding Discovery Report

## Scope And Worklist

- `rank_input.csv` and `deep_review_input.csv` each contain 274 deterministic rows from the repository worklist.
- `work_ledger.jsonl` contains 278 closure rows. Rows `WR-0275` through `WR-0278` were added as manual adjacency rows because relevant PowerShell and local skill documentation files were outside the deterministic file extension set but directly supported the promoted candidate.
- `bin/`, `obj/`, and `artifacts/010-merge/` generated rows were closed as not applicable to runtime source review.

## Discovery Strategy

本階段使用 repository-wide deep review，依以下 shard 檢視：

- Services/Models：catalog loading、validators、search/projection、path handling、logging、DTO deserialization。
- Pages/Program/config：Razor Pages route handlers、Razor encoding、antiforgery、static assets、error handling、hosting configuration。
- JS/CSS/Data/static assets：DOM sinks、localStorage handling、image/path/data constraints、vendored assets。
- Tests/specs/.specify/generated docs：test-only behavior、specification/tooling assets、generated artifact closure。

## Promoted Candidates

| Candidate | Title | Initial disposition | Evidence |
| --- | --- | --- | --- |
| `CAND-SPECIFY-AUTOCOMMIT-001` | Spec Kit auto-commit stages the entire worktree with `git add .` | Promoted for validation | `.specify/extensions.yml:3`, `.specify/extensions/git/git-config.yml:13-14`, `.specify/extensions/git/scripts/bash/auto-commit.sh:137-138`, `.specify/extensions/git/scripts/powershell/auto-commit.ps1:158-160` |

## Suppressed Or Closed During Discovery

| Surface | Closure |
| --- | --- |
| Catalog `ContentPath` file loading | `File.ReadAllText` is reached only from operator/developer-controlled configuration, not public request input. JSON must deserialize into concrete DTOs and pass validators before rendering. |
| Razor/DOM XSS | No `Html.Raw`, `IHtmlContent`, `innerHTML`, `insertAdjacentHTML`, `document.write`, or `eval` sink survived review. Razor templates use normal encoding, and first-party JS uses text/attribute APIs with validated server data. |
| Search/query injection | Search and quiz lookup are in-memory operations with no SQL, NoSQL, LDAP, shell, or expression-evaluation sink. |
| CSRF | The observed quiz POST records no server-side state and relies on Razor Pages antiforgery for form posts. No sensitive authenticated action exists. |
| Auth/authz | No login, roles, tenants, private objects, or protected mutation boundary exists in the current application. |
| Static asset exposure | Static files are under `wwwroot` as intended; content JSON remains under `StoryBook/Data/`, and no request-controlled file serving helper was found. |
| Secrets/config | No hardcoded production secrets, connection strings, API keys, tokens, or credentials were found in reviewed source/config rows. |
| NuGet advisory | `dotnet list StoryBook2.sln package --vulnerable --include-transitive` returned no vulnerable package advisories for the projects. |

## Discovery Closure

所有 promoted candidates have corresponding validation and attack-path receipts under `artifacts/05_findings/`. Non-promoted rows are closed in `work_ledger.jsonl` and summarized in `artifacts/03_coverage/repository_coverage_ledger.md`.
