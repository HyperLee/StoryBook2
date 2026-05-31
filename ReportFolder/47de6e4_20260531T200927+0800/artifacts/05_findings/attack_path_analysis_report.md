# Scan-Level Attack Path Analysis Report

## Surviving Findings

| Candidate | Final decision | Severity | Confidence | Rationale |
| --- | --- | --- | --- | --- |
| `CAND-SPECIFY-AUTOCOMMIT-001` | reportable | low | high | Direct local tooling evidence supports unsafe broad staging. The path is developer-only and not remotely reachable, so severity is limited. |

## Suppressed Runtime Attack Paths

No C#/Razor/JavaScript runtime attack path survived validation. The application has no authentication boundary, no database/query engine, no upload/extraction workflow, no server-side outbound fetch, and no request-controlled file serving helper in the reviewed code.
