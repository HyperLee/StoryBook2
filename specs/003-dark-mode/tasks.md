# Tasks: 網站深色模式與主題切換

**Input**: Design documents from `/specs/003-dark-mode/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/theme-ui.md, quickstart.md

**Tests**: Included. The feature spec and project constitution require test-first coverage for theme metadata, HTML/layout contracts, route behavior, and manual browser acceptance.

**Organization**: Tasks are grouped by user story so each story can be implemented and verified as an independent increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no dependency on incomplete tasks.
- **[Story]**: Maps the task to a user story. Setup, foundational, and polish tasks do not use story labels.
- Every task includes an exact repository path or solution file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the existing Razor Pages solution for the dark-mode implementation without adding new packages or changing architecture.

- [X] T001 Verify `StoryBook/StoryBook.csproj` and `StoryBook.Tests/StoryBook.Tests.csproj` still need no new NuGet or JavaScript package references for the planned Razor Pages, Bootstrap 5, native JavaScript, and xUnit implementation
- [X] T002 [P] Create the browser theme asset file `StoryBook/wwwroot/js/theme.js` as the dedicated native JavaScript entry point for theme behavior
- [X] T003 [P] Create the integration test file `StoryBook.Tests/Integration/ThemePagesTests.cs` for theme HTML, route, and layout contract coverage

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared metadata and theme token infrastructure required by all user stories.

**Critical**: No user story work should begin until this phase is complete.

- [X] T004 [P] Add failing unit tests for storage key `storybook.theme`, valid modes `light`/`dark`/`system`, default `system`, effective light fallback, and bilingual mode labels in `StoryBook.Tests/Unit/ThemePreferenceServiceTests.cs`
- [X] T005 Implement `ThemePreferenceService` with XML documentation, allowed mode parsing, default mode, storage key, safe effective-theme fallback constants, and bilingual mode metadata in `StoryBook/Services/ThemePreferenceService.cs`
- [X] T006 Register `ThemePreferenceService` as an injectable singleton in `StoryBook/Program.cs`
- [X] T007 [P] Add shared semantic CSS variable scaffolding for light theme, dark theme, surfaces, text, borders, controls, alerts, and focus rings in `StoryBook/wwwroot/css/site.css`
- [X] T008 Add reusable route HTML helpers and theme contract assertion helpers for upcoming tests in `StoryBook.Tests/Integration/ThemePagesTests.cs`

**Checkpoint**: Shared theme metadata and base styling contracts are ready for user story implementation.

---

## Phase 3: User Story 1 - 在首頁選擇整站主題 (Priority: P1)

**Goal**: Users can choose light, dark, or system mode on the home page, and every directly browsable page receives the same effective theme while non-home pages do not render a selector.

**Independent Test**: Select each theme mode on `/`, then visit `/`, `/Privacy`, `/Error`, `/dinosaurs`, `/dinosaurs/tyrannosaurus-rex`, `/aquarium`, and `/aquarium/clownfish`; all pages use the same effective theme and only `/` shows the selector.

### Tests for User Story 1

> Write these tests first and confirm they fail before implementation.

- [X] T009 [US1] Add failing integration tests that `/` renders exactly one accessible theme selector with `data-theme-storage-key="storybook.theme"` and three `data-theme-option` values in `StoryBook.Tests/Integration/ThemePagesTests.cs`
- [X] T010 [US1] Add failing integration tests that `/Privacy`, `/Error`, `/dinosaurs`, `/dinosaurs/tyrannosaurus-rex`, `/aquarium`, and `/aquarium/clownfish` do not render the theme selector in `StoryBook.Tests/Integration/ThemePagesTests.cs`
- [X] T011 [US1] Add failing integration tests that the shared layout emits an early theme boot script before the main stylesheets and exposes the required `data-bs-theme`, `data-storybook-theme-mode`, and `data-storybook-effective-theme` contract path in `StoryBook.Tests/Integration/ThemePagesTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Render the accessible segmented radio theme selector with three mutually exclusive options and bilingual data attributes in `StoryBook/Pages/Index.cshtml`
- [X] T013 [US1] Expose theme mode metadata from `ThemePreferenceService` to the home Razor view through `StoryBook/Pages/Index.cshtml.cs`
- [X] T014 [US1] Add the minimal synchronous theme boot script in the `<head>` before stylesheet links in `StoryBook/Pages/Shared/_Layout.cshtml`
- [X] T015 [US1] Reference `StoryBook/wwwroot/js/theme.js` from `StoryBook/Pages/Shared/_Layout.cshtml` while preserving existing Bootstrap, `site.js`, and language-switcher script behavior
- [X] T016 [US1] Implement initial mode resolution, selector checked-state updates, `localStorage` writes, and `<html>` data attribute updates in `StoryBook/wwwroot/js/theme.js`
- [X] T017 [US1] Remove or replace light-only Bootstrap classes such as forced white navbar backgrounds and forced dark nav text in `StoryBook/Pages/Shared/_Layout.cshtml`
- [X] T018 [US1] Style the home theme selector, selected state, 44x44 CSS px target size, and visible focus state in `StoryBook/wwwroot/css/site.css`
- [X] T019 [US1] Run `dotnet test StoryBook2.sln --filter ThemePagesTests` against the route and selector expectations in `specs/003-dark-mode/contracts/theme-ui.md`
- [X] T020 [US1] Run manual browser acceptance for homepage selector, selected-state clarity, non-home selector absence, and cross-route theme application using steps 1-3 in `specs/003-dark-mode/quickstart.md`

**Checkpoint**: User Story 1 is fully functional and testable as the MVP.

---

## Phase 4: User Story 2 - 跟隨系統並保留偏好 (Priority: P2)

**Goal**: Users can select system mode, the site follows browser color-scheme changes, and the browser persists only the selected mode for future visits.

**Independent Test**: Set mode to `system` and emulate light/dark system preferences, then select fixed `light` and `dark`, reload the site, and confirm the saved mode remains effective without saving the resolved effective theme.

### Tests for User Story 2

> Write these tests first and confirm they fail before implementation.

- [X] T021 [P] [US2] Add failing unit tests that invalid, missing, or unsupported saved theme values parse to `system` and never persist an effective theme value in `StoryBook.Tests/Unit/ThemePreferenceServiceTests.cs`
- [X] T022 [P] [US2] Add failing integration tests for bilingual selector labels, bilingual descriptions, and nonblank zh-TW fallback when language values are invalid in `StoryBook.Tests/Integration/ThemePagesTests.cs`

### Implementation for User Story 2

- [X] T023 [US2] Implement unavailable `localStorage`, invalid stored value recovery, and selected-mode-only persistence in `StoryBook/wwwroot/js/theme.js`
- [X] T024 [US2] Implement `system` mode resolution with `matchMedia('(prefers-color-scheme: dark)')`, guarded missing/throwing `matchMedia` fallback to the safe light effective theme, and effective theme updates within 2 seconds in `StoryBook/wwwroot/js/theme.js`
- [X] T025 [US2] Synchronize selector labels, descriptions, and selected-state text with `storybook.language`, including zh-TW fallback for invalid or missing language values, in `StoryBook/wwwroot/js/theme.js`
- [X] T026 [US2] Add any missing bilingual label and description data attributes consumed by `theme.js` to `StoryBook/Pages/Index.cshtml`
- [X] T027 [US2] Run `dotnet test StoryBook2.sln --filter "Theme"` using the persistence, system-mode, and language checks from `specs/003-dark-mode/quickstart.md`
- [X] T028 [US2] Run manual browser acceptance for persisted mode reloads, invalid storage recovery, unavailable `localStorage`, missing `matchMedia`, and system-preference changes using steps 4-7 in `specs/003-dark-mode/quickstart.md`

**Checkpoint**: User Stories 1 and 2 both work independently, including reload and system-preference scenarios.

---

## Phase 5: User Story 3 - 保持可讀、可操作與不干擾內容 (Priority: P3)

**Goal**: All themed pages remain readable, keyboard-operable, responsive, and stable; theme updates do not reset story content, language, search, navigation, image modal, route, or scroll position.

**Independent Test**: In light, dark, and system modes at 375px, 768px, and 1366x768, verify home, dinosaur, and aquarium flows remain readable and operable, including search, language switching, previous/next navigation, image viewing, empty states, and focus states.

### Tests for User Story 3

> Write these tests first and confirm they fail before implementation.

- [ ] T029 [US3] Add failing integration tests that themed routes keep canonical anchors, include theme assets, and do not add query-string, hash, `pushState`, or JavaScript-router markers in `StoryBook.Tests/Integration/ThemePagesTests.cs`

### Implementation for User Story 3

- [ ] T030 [P] [US3] Add dark-mode and high-contrast overrides for dinosaur cards, search controls, pager links, empty states, image buttons, modal content, and focus states in `StoryBook/wwwroot/css/dinosaurs.css`
- [ ] T031 [P] [US3] Add dark-mode and high-contrast overrides for aquarium cover, cards, search controls, pager links, empty states, image buttons, modal content, and focus states in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T032 [US3] Add global light/dark rules for body, navbar, footer, links, buttons, forms, cards, alerts, validation messages, modal surfaces, and focus indicators in `StoryBook/wwwroot/css/site.css`
- [ ] T033 [US3] Implement guarded `storage` event cross-tab synchronization that updates only theme attributes when available and preserves route, scroll position, search inputs, language state, and open modal state in `StoryBook/wwwroot/js/theme.js`
- [ ] T034 [US3] Review and adjust light-only classes, hard-coded backgrounds, or forced text colors in `StoryBook/Pages/Index.cshtml`, `StoryBook/Pages/Privacy.cshtml`, and `StoryBook/Pages/Error.cshtml`
- [ ] T035 [US3] Run manual responsive, keyboard, contrast, cross-tab availability, and state-preservation checks from steps 8-13 in `specs/003-dark-mode/quickstart.md`
- [ ] T036 [US3] Run `dotnet test StoryBook2.sln` after the US3 implementation using `StoryBook2.sln`

**Checkpoint**: All user stories are independently functional and the full themed experience is ready for final validation.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final consistency checks across specs, implementation, tests, and generated artifacts.

- [ ] T037 [P] Review `specs/003-dark-mode/spec.md` for implementation drift found during delivery and update only if behavior changed
- [ ] T038 [P] Review `specs/003-dark-mode/plan.md` for implementation drift, dependency changes, or architecture changes and update only if behavior changed
- [ ] T039 Run `dotnet build StoryBook2.sln` and `dotnet test StoryBook2.sln`, confirm no new compiler warnings, and confirm no generated `StoryBook/obj` or `StoryBook.Tests/obj` artifacts are staged while validating `StoryBook2.sln`
- [ ] T040 Run the full browser acceptance checklist in `specs/003-dark-mode/quickstart.md` and record any remaining follow-up directly in `specs/003-dark-mode/tasks.md`
- [ ] T041 Review final constitution delivery gates for unresolved template markers, secrets/connection strings/API keys, user-visible zh-TW text, and required non-sensitive `ILogger<T>` coverage across `StoryBook/`, `StoryBook.Tests/`, and `specs/003-dark-mode/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1 Setup has no dependencies and can start immediately.
- Phase 2 Foundational depends on Phase 1 and blocks all user stories.
- Phase 3 US1 depends on Phase 2 and is the MVP.
- Phase 4 US2 depends on Phase 2 and may reuse US1 selector/layout work, but its persistence/system behavior can be tested independently once the selector exists.
- Phase 5 US3 depends on Phase 2 and benefits from US1/US2 behavior, but its CSS and noninterference checks are scoped to existing routes and assets.
- Phase 6 Polish depends on whichever user stories are selected for delivery.

### User Story Dependencies

- US1 (P1): Starts after Foundational. No dependency on US2 or US3.
- US2 (P2): Starts after Foundational. Uses the same selector contract as US1 for manual browser verification.
- US3 (P3): Starts after Foundational. CSS overrides can be prepared independently, but final validation should run after selected theme behavior exists.

### Within Each User Story

- Tests must be written first and should fail before implementation.
- Shared service metadata comes before Razor rendering.
- Layout boot script comes before route-wide visual styling.
- Browser behavior in `theme.js` comes before manual quickstart checks.
- Manual or browser acceptance for the story must run before marking that story checkpoint complete.
- Story validation should pass before moving to the next priority story.

### Parallel Opportunities

- T002 and T003 can run in parallel during setup.
- T004 and T007 can run in parallel during foundational work.
- T021 and T022 can run in parallel at the start of US2.
- T030 and T031 can run in parallel because dinosaur and aquarium CSS are separate files.
- T037 and T038 can run in parallel during polish.

---

## Parallel Example: User Story 2

```bash
# Run these two task assignments in parallel after Phase 2:
Task: "Add failing unit tests that invalid, missing, or unsupported saved theme values parse to system and never persist an effective theme value in StoryBook.Tests/Unit/ThemePreferenceServiceTests.cs"
Task: "Add failing integration tests for bilingual selector labels, bilingual descriptions, and nonblank zh-TW fallback when language values are invalid in StoryBook.Tests/Integration/ThemePagesTests.cs"
```

---

## Parallel Example: User Story 3

```bash
# Run these two task assignments in parallel after US3 tests are written:
Task: "Add dark-mode and high-contrast overrides for dinosaur cards, search controls, pager links, empty states, image buttons, modal content, and focus states in StoryBook/wwwroot/css/dinosaurs.css"
Task: "Add dark-mode and high-contrast overrides for aquarium cover, cards, search controls, pager links, empty states, image buttons, modal content, and focus states in StoryBook/wwwroot/css/aquarium.css"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 US1.
4. Stop and validate `/`, `/Privacy`, `/Error`, dinosaur routes, and aquarium routes against `specs/003-dark-mode/contracts/theme-ui.md`.
5. Demo the homepage selector and cross-page theme application.

### Incremental Delivery

1. Setup + Foundational: Add shared metadata, base CSS tokens, and test scaffolding.
2. US1: Deliver the homepage selector and route-wide effective theme application as MVP.
3. US2: Add persistence, system preference following, invalid storage fallback, and bilingual selector behavior.
4. US3: Add final visual, accessibility, responsive, cross-tab sync, and state-preservation hardening.
5. Polish: Re-run build, tests, and manual quickstart acceptance before delivery.

### Parallel Team Strategy

1. One developer completes service metadata and DI after the failing unit tests are in place.
2. One developer writes route/layout integration tests and homepage Razor selector work.
3. After Phase 2, CSS work for dinosaur and aquarium routes can split by feature stylesheet.
4. Final browser acceptance should be run once the selected implementation scope is integrated.

## Notes

- Keep the application as a single ASP.NET Core Razor Pages app.
- Do not add a SPA router, database, external CMS, theme API, cookie preference endpoint, or frontend build pipeline.
- Do not modify Bootstrap vendor files under `StoryBook/wwwroot/lib/`.
- Do not save derived effective theme values to storage; only save `light`, `dark`, or `system`.
- Keep `storybook.language` behavior separate from `storybook.theme`.
