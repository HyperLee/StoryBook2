# Tasks: 主題學習旅程

**Input**: Design documents from `/specs/006-learning-journeys/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/ui-routes.md`, `contracts/learning-journeys.schema.json`, `quickstart.md`

**Tests**: Required. The feature plan and project constitution require test-first work for validation, services, routes, fallback states, bilingual behavior, theme contract, and HTML contracts.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently after the shared foundation is in place.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches a different file and does not depend on incomplete tasks.
- **[Story]**: User story label from `spec.md` (`US1`, `US2`, `US3`, `US4`).
- Every task includes exact file paths.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add the feature's empty assets, fixture surface, and curated data file locations without changing existing behavior.

- [ ] T001 [P] Create the curated learning journey catalog with at least three complete available journey records in `StoryBook/Data/journeys.json`
- [ ] T002 [P] Create the feature stylesheet scaffold in `StoryBook/wwwroot/css/journeys.css`
- [ ] T003 [P] Create the feature script scaffold in `StoryBook/wwwroot/js/journeys.js`
- [ ] T004 [P] Create the journey integration fixture scaffold in `StoryBook.Tests/Integration/JourneyPageTestFixture.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared contracts, models, options, validator/service shells, and DI registration required before any journey page story can be implemented.

**Critical**: No user story work begins until these tasks compile.

- [ ] T005 Add the `LearningJourneyCatalog` configuration section with `ContentPath` in `StoryBook/appsettings.json`
- [ ] T006 [P] Create `LearningJourneyCatalogOptions` with XML docs and default `Data/journeys.json` path in `StoryBook/Services/LearningJourneyCatalogOptions.cs`
- [ ] T007 [P] Create `JourneyText` with `zh-TW` fallback behavior and XML docs in `StoryBook/Models/JourneyText.cs`
- [ ] T008 [P] Create `JourneyStoryReference` with source, slug, sort order fields, and XML docs in `StoryBook/Models/JourneyStoryReference.cs`
- [ ] T009 [P] Create `LearningJourney` with slug, sort order, localized content, reading metadata, story references, and XML docs in `StoryBook/Models/LearningJourney.cs`
- [ ] T010 [P] Create `JourneyCatalog` root model matching `contracts/learning-journeys.schema.json` with XML docs in `StoryBook/Models/JourneyCatalog.cs`
- [ ] T011 [P] Create `JourneyDiagnosticSummary` with non-sensitive reason/source/reference fields and XML docs in `StoryBook/Models/JourneyDiagnosticSummary.cs`
- [ ] T012 [P] Create `JourneyAvailabilityStatus` and availability state enum with XML docs in `StoryBook/Models/JourneyAvailabilityStatus.cs`
- [ ] T013 [P] Create `JourneySourceStatus` for source availability summaries with XML docs in `StoryBook/Models/JourneySourceStatus.cs`
- [ ] T014 [P] Create `JourneyStoryItem` projection with href, localized source/name/summary, image, stable id fields, and XML docs in `StoryBook/Models/JourneyStoryItem.cs`
- [ ] T015 Create `JourneyCatalogSnapshot` with available journeys, unavailable statuses, source statuses, aggregate failure flags, and XML docs in `StoryBook/Models/JourneyCatalogSnapshot.cs`
- [ ] T016 Create `LearningJourneyContentValidationResult` for errors and diagnostic summaries with XML docs in `StoryBook/Services/LearningJourneyContentValidationResult.cs`
- [ ] T017 Create `LearningJourneyContentValidator` validation entry points and XML docs in `StoryBook/Services/LearningJourneyContentValidator.cs`
- [ ] T018 Create `LearningJourneyCatalogService` compile-ready skeleton with snapshot/detail lookup methods and XML docs in `StoryBook/Services/LearningJourneyCatalogService.cs`
- [ ] T019 Register journey options, validator, and catalog service in `StoryBook/Program.cs`

**Checkpoint**: The solution compiles with journey types and service contracts available, but journey behavior is still expected to fail story tests.

---

## Phase 3: User Story 1 - 找到並進入學習旅程 (Priority: P1)

**Goal**: Users can discover a normal anchor entry from `/` and `/explore`, navigate to `/journeys`, and see at least three available learning journeys.

**Independent Test**: From `/` and `/explore`, activate the learning journey entry and verify `/journeys` renders at least three `data-journey-card` cards in Traditional Chinese by default.

### Tests for User Story 1

Write these tests first and confirm they fail before implementation.

- [ ] T020 [P] [US1] Add catalog load/sort/available-count unit tests for at least three complete journeys in `StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs`
- [ ] T021 [P] [US1] Add `/journeys` integration tests for `200 OK`, `data-journeys-list`, at least three `data-journey-card` elements, and theme selector absence in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T022 [P] [US1] Add homepage and `/explore` integration tests for normal `/journeys` anchors without removing existing actions in `StoryBook.Tests/Integration/JourneyPagesTests.cs`

### Implementation for User Story 1

- [ ] T023 [US1] Implement JSON load, cache, sort by `SortOrder` then slug, and available journey list projection in `StoryBook/Services/LearningJourneyCatalogService.cs`
- [ ] T024 [US1] Implement `/journeys` PageModel with snapshot loading and friendly aggregate state properties in `StoryBook/Pages/Journeys/Index.cshtml.cs`
- [ ] T025 [US1] Implement `/journeys` Razor list markup with `data-journeys-list`, `data-journey-card`, reading time, age guidance, story count, and normal detail anchors in `StoryBook/Pages/Journeys/Index.cshtml`
- [ ] T026 [US1] Add the homepage learning journey entry as a normal anchor in `StoryBook/Pages/Index.cshtml`
- [ ] T027 [US1] Add the `/explore` learning journey entry as a normal anchor in `StoryBook/Pages/Explore/Index.cshtml`
- [ ] T028 [US1] Style journey list cards, metadata, and entry actions without horizontal overflow in `StoryBook/wwwroot/css/journeys.css`
- [ ] T029 [US1] Include the journey stylesheet and script sections for the list page in `StoryBook/Pages/Journeys/Index.cshtml`
- [ ] T030 [US1] Run `dotnet test StoryBook2.sln` and address US1 failures in `StoryBook.Tests/Integration/JourneyPagesTests.cs`

**Checkpoint**: User Story 1 is fully functional and independently testable as the MVP entry/list flow.

---

## Phase 4: User Story 2 - 查看旅程詳情並開始閱讀 (Priority: P1)

**Goal**: Users can open `/journeys/{slug}`, understand the journey, see 3-5 ordered story items, and start reading the first valid story.

**Independent Test**: Directly open a known journey detail URL and verify title, summary, learning goals, reading time, age guidance, ordered story items, start-reading link, story links, and back-to-list link.

### Tests for User Story 2

Write these tests first and confirm they fail before implementation.

- [ ] T031 [P] [US2] Add service tests for resolving dinosaur and aquarium references into names, summaries, source labels, detail hrefs, and first-story start href in `StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs`
- [ ] T032 [P] [US2] Add `/journeys/{slug}` integration tests for detail contract, `data-journey-detail`, `data-journey-start-reading`, and `/journeys` back link in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T033 [P] [US2] Add integration tests that journey story item links point only to `/dinosaurs/{slug}` or `/aquarium/{slug}` normal anchors in `StoryBook.Tests/Integration/JourneyPagesTests.cs`

### Implementation for User Story 2

- [ ] T034 [US2] Implement journey detail projection, source lookup, story item ordering, and first valid item start href in `StoryBook/Services/LearningJourneyCatalogService.cs`
- [ ] T035 [US2] Implement `/journeys/{slug}` PageModel for available journey detail loading in `StoryBook/Pages/Journeys/Details.cshtml.cs`
- [ ] T036 [US2] Implement detail Razor markup with title, summary, learning goals, reading metadata, ordered story item list, start-reading anchor, and back link in `StoryBook/Pages/Journeys/Details.cshtml`
- [ ] T037 [US2] Add detail and story item responsive styles in `StoryBook/wwwroot/css/journeys.css`
- [ ] T038 [US2] Include the journey stylesheet and script sections for the detail page in `StoryBook/Pages/Journeys/Details.cshtml`
- [ ] T039 [US2] Run `dotnet test StoryBook2.sln` and address US2 failures in `StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs`

**Checkpoint**: User Story 2 works independently when a valid journey slug is opened directly or from the list.

---

## Phase 5: User Story 3 - 保持旅程資料完整與友善狀態 (Priority: P1)

**Goal**: Journey pages never show blank text, duplicate cards, broken links, sensitive diagnostics, or confusing technical errors when journey data or source catalogs are incomplete.

**Independent Test**: Simulate missing references, duplicate references, not-enough items, too-many items, unknown slug, partial source failure, and all-source failure; verify friendly states and no broken links or sensitive details.

### Tests for User Story 3

Write these tests first and confirm they fail before implementation.

- [ ] T040 [P] [US3] Add validator tests for kebab-case slugs, unique slugs, required localized text fallback, 1-3 goals, positive minutes, allowed sources, and reference sort order in `StoryBook.Tests/Unit/LearningJourneyContentValidationTests.cs`
- [ ] T041 [P] [US3] Add service tests for invalid references, duplicate references, not-enough items, too-many items, source unavailable, all sources failed, and sanitized diagnostics in `StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs`
- [ ] T042 [P] [US3] Add integration tests for unknown slug, unavailable detail state, hidden unavailable journeys, partial failure state, all-unavailable state, and absence of internal diagnostics in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T043 [P] [US3] Add source failure and invalid journey fixture helpers in `StoryBook.Tests/Integration/JourneyPageTestFixture.cs`

### Implementation for User Story 3

- [ ] T044 [US3] Implement validation rules and child-friendly validation result messages in `StoryBook/Services/LearningJourneyContentValidator.cs`
- [ ] T045 [US3] Implement invalid reference filtering, duplicate removal, 3-5 item availability rules, and source failure aggregation in `StoryBook/Services/LearningJourneyCatalogService.cs`
- [ ] T046 [US3] Implement sanitized `ILogger<LearningJourneyCatalogService>` summaries without file paths, exception details, stack traces, secrets, or personal data in `StoryBook/Services/LearningJourneyCatalogService.cs`
- [ ] T047 [US3] Render partial failure and all-unavailable friendly list states with normal home/explore anchors in `StoryBook/Pages/Journeys/Index.cshtml`
- [ ] T048 [US3] Render not-found and unavailable detail states without misleading start-reading links in `StoryBook/Pages/Journeys/Details.cshtml`
- [ ] T049 [US3] Add CSS for journey status, empty, partial failure, and unavailable states in `StoryBook/wwwroot/css/journeys.css`
- [ ] T050 [US3] Run `dotnet test StoryBook2.sln` and address US3 failures in `StoryBook.Tests/Unit/LearningJourneyContentValidationTests.cs`

**Checkpoint**: User Story 3 protects data integrity and friendly failure states while preserving existing story routes.

---

## Phase 6: User Story 4 - 保持雙語、主題與可及性一致 (Priority: P2)

**Goal**: Journey pages follow existing language preference, theme application, keyboard accessibility, focus visibility, and responsive layout contracts without adding new preference storage.

**Independent Test**: Switch to English and dark theme, open `/journeys` and `/journeys/{slug}`, and verify localized text, fallback behavior, no theme selector, visible focus, 44x44 targets, and no journey progress state.

### Tests for User Story 4

Write these tests first and confirm they fail before implementation.

- [ ] T051 [P] [US4] Add unit tests for `JourneyText` fallback and no blank localized display values in `StoryBook.Tests/Unit/LearningJourneyContentValidationTests.cs`
- [ ] T052 [P] [US4] Add integration tests for bilingual `data-i18n-*` attributes, localized accessible names, theme layout contract, and theme selector absence on journey pages in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T053 [P] [US4] Add script contract tests for `storybook.language` usage, no journey storage writes, no history mutation, and no theme-selector behavior in `StoryBook.Tests/Unit/JourneysScriptContractTests.cs`

### Implementation for User Story 4

- [ ] T054 [US4] Add bilingual labels, `data-i18n-zh-tw`, `data-i18n-en`, localized aria labels, and fallback-safe text to the list page in `StoryBook/Pages/Journeys/Index.cshtml`
- [ ] T055 [US4] Add bilingual labels, `data-i18n-zh-tw`, `data-i18n-en`, localized aria labels, and fallback-safe text to the detail page in `StoryBook/Pages/Journeys/Details.cshtml`
- [ ] T056 [US4] Implement `journeys.js` language synchronization using only `storybook.language` and without writing journey state to storage or history in `StoryBook/wwwroot/js/journeys.js`
- [ ] T057 [US4] Complete responsive, focus-visible, contrast-aware, and 44x44 target styling for 375px, 768px, and 1366px widths in `StoryBook/wwwroot/css/journeys.css`
- [ ] T058 [US4] Run `dotnet test StoryBook2.sln` and address US4 failures in `StoryBook.Tests/Unit/JourneysScriptContractTests.cs`

**Checkpoint**: Journey pages match the existing bilingual, theme, keyboard, and responsive site experience.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, documentation alignment, and regression checks across the full feature.

- [ ] T059 [P] Update manual validation notes with actual journey slugs and representative source-failure setup in `specs/006-learning-journeys/quickstart.md`
- [ ] T060 [P] Add final negative-scope assertions for no login, no external APIs, no progress storage, and no journey-specific story routes in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T061 Record complete-catalog performance verification for `/journeys` and one known `/journeys/{slug}` against SC-011 in `specs/006-learning-journeys/quickstart.md`
- [ ] T062 Run `dotnet build StoryBook2.sln` and address build warnings or errors in `StoryBook2.sln`
- [ ] T063 Run `dotnet test StoryBook2.sln` and address failing journey regressions in `StoryBook.Tests/Integration/JourneyPagesTests.cs`
- [ ] T064 Perform quickstart manual acceptance for `/`, `/explore`, `/journeys`, `/journeys/{slug}`, language, theme, keyboard, responsive, and performance flows documented in `specs/006-learning-journeys/quickstart.md`
- [ ] T065 Review changed files and ensure generated `bin/` or `obj/` artifacts are not staged by checking paths listed in `StoryBook2.sln`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; tasks marked `[P]` can run together.
- **Foundational (Phase 2)**: Depends on Phase 1; blocks all user stories.
- **User Stories (Phase 3-6)**: Depend on Phase 2; P1 stories should be delivered before P2 consistency polish if working sequentially.
- **Polish (Phase 7)**: Depends on all targeted user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundation; no dependency on other user stories; recommended MVP scope.
- **US2 (P1)**: Starts after Foundation; can run in parallel with US1 service work but needs shared model/service contracts.
- **US3 (P1)**: Starts after Foundation; should be completed before release because it guards broken links and sensitive diagnostics.
- **US4 (P2)**: Starts after Foundation; can run after or alongside US1/US2 markup work, with final verification after P1 stories render.

### Within Each User Story

- Tests must be written first and must fail before implementation.
- Models/options before validator and catalog service behavior.
- Validator/source resolution before PageModel rendering of unavailable states.
- PageModel before Razor markup that depends on model properties.
- CSS/JS after the HTML contract is stable.
- Story checkpoint validation before moving to the next priority when working sequentially.

---

## Parallel Opportunities

- Phase 1 setup files `StoryBook/Data/journeys.json`, `StoryBook/wwwroot/css/journeys.css`, `StoryBook/wwwroot/js/journeys.js`, and `StoryBook.Tests/Integration/JourneyPageTestFixture.cs` can be created in parallel.
- Phase 2 model files can be created in parallel before wiring `LearningJourneyCatalogService` and `Program.cs`.
- US1 tests T020-T022 can be written in parallel.
- US2 tests T031-T033 can be written in parallel.
- US3 tests T040-T043 can be written in parallel.
- US4 tests T051-T053 can be written in parallel.
- Markup/CSS/JS tasks for US4 can proceed in parallel after the list/detail HTML contracts are stable.

## Parallel Example: User Story 1

```text
Task: "T020 [P] [US1] Add catalog load/sort/available-count unit tests in StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs"
Task: "T021 [P] [US1] Add /journeys integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
Task: "T022 [P] [US1] Add homepage and /explore integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
```

## Parallel Example: User Story 2

```text
Task: "T031 [P] [US2] Add service tests for source resolution in StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs"
Task: "T032 [P] [US2] Add detail contract integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
Task: "T033 [P] [US2] Add story link route integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
```

## Parallel Example: User Story 3

```text
Task: "T040 [P] [US3] Add validation rule tests in StoryBook.Tests/Unit/LearningJourneyContentValidationTests.cs"
Task: "T041 [P] [US3] Add availability and diagnostic service tests in StoryBook.Tests/Unit/LearningJourneyCatalogServiceTests.cs"
Task: "T042 [P] [US3] Add friendly failure state integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
Task: "T043 [P] [US3] Add source failure fixture helpers in StoryBook.Tests/Integration/JourneyPageTestFixture.cs"
```

## Parallel Example: User Story 4

```text
Task: "T051 [P] [US4] Add localized fallback unit tests in StoryBook.Tests/Unit/LearningJourneyContentValidationTests.cs"
Task: "T052 [P] [US4] Add bilingual and theme contract integration tests in StoryBook.Tests/Integration/JourneyPagesTests.cs"
Task: "T053 [P] [US4] Add journeys script contract tests in StoryBook.Tests/Unit/JourneysScriptContractTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup.
2. Complete Phase 2 foundation.
3. Complete Phase 3 User Story 1.
4. Stop and validate `/` -> `/journeys`, `/explore` -> `/journeys`, and at least three available journeys.

### Incremental Delivery

1. Deliver US1 for discovery and journey list MVP.
2. Add US2 for detail pages and start-reading route.
3. Add US3 before release to guarantee no broken links, blank content, or sensitive diagnostics.
4. Add US4 to complete bilingual, theme, keyboard, and responsive consistency.
5. Run Phase 7 validation before handoff.

### Parallel Team Strategy

1. Complete Setup and Foundation together.
2. Assign US1 list/entry, US2 detail projection, and US3 validation/failure tests to separate workers after Foundation.
3. Start US4 markup/CSS/JS once the list and detail HTML contracts are stable.
4. Merge only after `dotnet build StoryBook2.sln`, `dotnet test StoryBook2.sln`, and quickstart checks pass.

## Notes

- Do not add databases, external APIs, new NuGet packages, JavaScript packages, SPA routing, authentication, progress storage, or journey-specific story detail routes.
- Journey story links must remain normal anchors to `/dinosaurs/{slug}` or `/aquarium/{slug}`.
- Journey pages must not render a theme selector and must not write journey progress to `localStorage`, `sessionStorage`, cookies, query string, browser history, or server state.
- Any logging added for this feature must use sanitized journey/source/reason summaries only.
