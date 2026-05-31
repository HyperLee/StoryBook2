# Candidate Reconciliation And Dedupe Report

## Inputs

- Raw candidates: `artifacts/02_discovery/raw_candidates.jsonl`
- Work ledger: `artifacts/02_discovery/work_ledger.jsonl`
- Candidate directory: `artifacts/05_findings/CAND-SPECIFY-AUTOCOMMIT-001/`

## Result

| Canonical candidate | Merged rows | Decision |
| --- | --- | --- |
| `CAND-SPECIFY-AUTOCOMMIT-001` | Spec Kit auto-execution config, auto-commit config, Bash auto-commit, PowerShell auto-commit, Bash initialize, PowerShell initialize, local skill docs | Keep one final finding with multiple inseparable affected lines |

## Rationale

The candidate is one workflow-level issue: enabled Spec Kit hooks call an auto-commit command that stages the whole worktree before committing. The Bash and PowerShell scripts are platform variants, and the initialize scripts are the same broad-staging pattern during repository initialization. They are therefore deduped into one final finding while preserving each concrete root-control line in validation and final report evidence.

No separate sibling runtime finding was merged into this candidate. All C#/Razor/JS/JSON runtime candidates were closed before final reconciliation.
