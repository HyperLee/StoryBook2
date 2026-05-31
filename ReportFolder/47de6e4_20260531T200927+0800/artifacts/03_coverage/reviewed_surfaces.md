# Reviewed Surfaces

| Surface | Risk Area | Outcome | Notes |
| --- | --- | --- | --- |
| `StoryBook/Services/` and `StoryBook/Models/` | Catalog loading, validation, search projections, DTO parsing | No issue found | Full shard review found no public request-controlled path to file reads, deserialization RCE, command/query injection, or unsafe content projection. |
| `StoryBook/Pages/`, `Program.cs`, appsettings, launch settings | Razor routes, handlers, antiforgery, hosting configuration | No issue found | Razor encoding and normal form protections hold for observed paths; no auth boundary or sensitive state mutation exists. |
| `StoryBook/wwwroot/js/`, `StoryBook/wwwroot/css/`, `StoryBook/Data/` | DOM XSS, localStorage, static asset/data constraints | Rejected | LocalStorage is not trusted as a server security boundary; JS uses safe DOM APIs; data paths and slugs are constrained. |
| `StoryBook/wwwroot/lib/` and NuGet packages | Dependency/advisory risk | No issue found | NuGet vulnerable-package check returned no advisories; vendored assets are not dynamically fetched and no vulnerable reachable feature was identified. |
| `.specify/` and `.agents/skills/speckit-*` | Local tooling, auto-commit, secret exposure risk | Reported | `CAND-SPECIFY-AUTOCOMMIT-001` reports broad auto-staging in Spec Kit Git workflows. |
| `StoryBook.Tests/`, `specs/`, generated `bin/obj` and `artifacts/010-merge/` rows | Test/spec/generated source review | Not applicable | Reviewed for reusable security patterns or tooling risk; generated/test-only rows are not runtime attack surface. |
