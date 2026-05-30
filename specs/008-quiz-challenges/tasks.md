# Tasks: 互動問答挑戰

**Input**: Design documents from `/specs/008-quiz-challenges/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/quiz-questions.schema.json`, `contracts/ui-routes.md`, `quickstart.md`

**Tests**: 本功能規格與專案憲章明確要求測試優先；每個使用者故事先列測試任務，再列實作任務。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no dependency on incomplete tasks in the same phase
- **[Story]**: Maps to the user story from `spec.md`
- Every task includes concrete repository paths

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish quiz feature scaffolding without changing existing storybook behavior.

- [X] T001 Create quiz page and asset scaffolds in `StoryBook/Pages/Quiz/Index.cshtml`, `StoryBook/Pages/Quiz/Index.cshtml.cs`, `StoryBook/wwwroot/css/quiz.css`, and `StoryBook/wwwroot/js/quiz.js`
- [X] T002 [P] Add the initial quiz catalog file with `version` and `questions` root fields in `StoryBook/Data/quiz-questions.json`
- [X] T003 [P] Add quiz test file scaffolds in `StoryBook.Tests/Unit/QuizContentValidationTests.cs`, `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`, `StoryBook.Tests/Unit/QuizScriptContractTests.cs`, and `StoryBook.Tests/Integration/QuizPagesTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared domain types and service contracts required before any quiz user story can be implemented.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T004 [P] Create localized quiz text model with zh-TW fallback behavior contract in `StoryBook/Models/QuizText.cs`
- [X] T005 [P] Create quiz difficulty enum/parser for `easy` and `medium` in `StoryBook/Models/QuizDifficulty.cs`
- [X] T006 [P] Create quiz scope enum/parser for `all`, `dinosaurs`, and `aquarium` in `StoryBook/Models/QuizScope.cs`
- [X] T007 [P] Create answer option model in `StoryBook/Models/QuizAnswerOption.cs`
- [X] T008 [P] Create related story reference model in `StoryBook/Models/QuizStoryReference.cs`
- [X] T009 Create quiz question and root catalog models in `StoryBook/Models/QuizQuestion.cs` and `StoryBook/Models/QuizCatalog.cs`
- [X] T010 [P] Create safe display projection models in `StoryBook/Models/QuizQuestionView.cs`
- [X] T011 [P] Create transient answer result model in `StoryBook/Models/QuizAnswerResult.cs`
- [X] T012 [P] Create quiz catalog snapshot and source status models in `StoryBook/Models/QuizCatalogSnapshot.cs` and `StoryBook/Models/QuizSourceStatus.cs`
- [X] T013 Create quiz catalog options with default `Data/quiz-questions.json` path in `StoryBook/Services/QuizCatalogOptions.cs`
- [X] T014 Create quiz validation result type for invalid-question and source diagnostics in `StoryBook/Services/QuizContentValidationResult.cs`

**Checkpoint**: Quiz domain model is ready for test-first story implementation.

---

## Phase 3: User Story 1 - 進入問答挑戰並選擇範圍 (Priority: P1) MVP

**Goal**: Users can enter `/quiz`, choose all stories, dinosaurs, or aquarium scope, and see one valid question for the selected scope.

**Independent Test**: From `/` and `/explore`, follow normal anchors to `/quiz`, choose `dinosaurs`, and verify a dinosaur question is displayed.

### Tests for User Story 1

- [X] T015 [P] [US1] Add unit tests for scope parsing, stable question ordering, scope filtering, invalid scope fallback, and next-question id selection in `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`
- [X] T016 [P] [US1] Add integration tests for `/quiz`, home entry, explore entry, scope links, invalid scope fallback, and initial question DOM metadata in `StoryBook.Tests/Integration/QuizPagesTests.cs`

### Implementation for User Story 1

- [X] T017 [US1] Implement quiz JSON load/cache, scope filtering, stable sorting, and next-question href projection in `StoryBook/Services/QuizCatalogService.cs`
- [X] T018 [US1] Seed at least 12 complete starter questions with at least 5 dinosaur and 5 aquarium questions in `StoryBook/Data/quiz-questions.json`
- [X] T019 [US1] Register `QuizCatalogOptions`, `QuizContentValidator`, and `QuizCatalogService` in `StoryBook/Program.cs`
- [X] T020 [US1] Implement `OnGet` scope and question selection for `/quiz` in `StoryBook/Pages/Quiz/Index.cshtml.cs`
- [X] T021 [US1] Render quiz title, scope nav, current question, answer options shell, and data attributes in `StoryBook/Pages/Quiz/Index.cshtml`
- [X] T022 [P] [US1] Add a normal anchor quiz entry on the home page in `StoryBook/Pages/Index.cshtml`
- [X] T023 [P] [US1] Add a normal anchor quiz entry on the explore page in `StoryBook/Pages/Explore/Index.cshtml`
- [X] T024 [US1] Add responsive scope and question layout styles in `StoryBook/wwwroot/css/quiz.css`
- [X] T025 [US1] Run US1 route and scope tests, then record the result in `specs/008-quiz-challenges/quickstart.md`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - 回答單題選擇題並得到友善回饋 (Priority: P1)

**Goal**: Users can submit one selected answer and receive immediate friendly correct, incorrect, or no-selection feedback with an explanation.

**Independent Test**: Submit correct, wrong, and no-selection answers on `/quiz`; verify the page shows only current-question feedback and no cumulative score or progress statistics.

### Tests for User Story 2

- [X] T026 [P] [US2] Add unit tests for correct answer, wrong answer, unknown option, no selected option, and transient answer result behavior in `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`
- [X] T027 [P] [US2] Add integration tests for antiforgery answer post, feedback rendering, explanation rendering, no-selection prompt, and absence of score/progress UI in `StoryBook.Tests/Integration/QuizPagesTests.cs`

### Implementation for User Story 2

- [X] T028 [US2] Add answer evaluation behavior that returns `QuizAnswerResult` without storing answer state in `StoryBook/Services/QuizCatalogService.cs`
- [X] T029 [US2] Implement `OnPostAnswer` with antiforgery model binding, no-selection handling, and no URL/session persistence in `StoryBook/Pages/Quiz/Index.cshtml.cs`
- [X] T030 [US2] Render semantic radio fieldset, submit button, live feedback region, explanation text, and next-question link in `StoryBook/Pages/Quiz/Index.cshtml`
- [X] T031 [US2] Add non-color-only feedback states and 44px minimum answer controls in `StoryBook/wwwroot/css/quiz.css`
- [X] T032 [US2] Add progressive submit guard and focus handling without correctness logic or storage usage in `StoryBook/wwwroot/js/quiz.js`
- [X] T033 [US2] Run US2 answer-flow tests, then record the result in `specs/008-quiz-challenges/quickstart.md`

**Checkpoint**: User Story 2 is independently functional and testable.

---

## Phase 5: User Story 3 - 從題目回到相關故事複習 (Priority: P1)

**Goal**: Every valid question links back to one or more existing dinosaur or aquarium story detail pages for review.

**Independent Test**: Open any valid question and activate each related story link; dinosaur links open `/dinosaurs/{slug}` and aquarium links open `/aquarium/{slug}`.

### Tests for User Story 3

- [X] T034 [P] [US3] Add unit tests for related story resolution, duplicate reference rejection, missing story rejection, source labels, and canonical href derivation in `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`
- [X] T035 [P] [US3] Add integration tests for related story anchors, `data-quiz-related-story` metadata, source labels, and no broken anchors in `StoryBook.Tests/Integration/QuizPagesTests.cs`

### Implementation for User Story 3

- [X] T036 [US3] Resolve related story references through `DinosaurCatalogService` and `AquariumCatalogService` in `StoryBook/Services/QuizCatalogService.cs`
- [X] T037 [US3] Verify every starter question has at least one valid related story reference in `StoryBook/Data/quiz-questions.json`
- [X] T038 [US3] Render related story anchors with source, slug, href, and bilingual label metadata in `StoryBook/Pages/Quiz/Index.cshtml`
- [X] T039 [US3] Add related story review link layout styles in `StoryBook/wwwroot/css/quiz.css`
- [X] T040 [US3] Run US3 related-story tests, then record the result in `specs/008-quiz-challenges/quickstart.md`

**Checkpoint**: User Story 3 is independently functional and testable.

---

## Phase 6: User Story 4 - 題庫資料完整並可降級 (Priority: P1)

**Goal**: Invalid questions are filtered out, catalog/source failures degrade to child-friendly states, and internal diagnostics never appear in user-facing HTML.

**Independent Test**: Point tests at malformed quiz catalogs and unavailable story references; verify invalid questions do not render and `/quiz` shows a friendly fallback when no valid question remains.

### Tests for User Story 4

- [X] T041 [P] [US4] Add validation tests for version, required fields, unique kebab-case ids, source values, difficulty values, bilingual text, option count, unique correct option, related story shape, and minimum valid question counts in `StoryBook.Tests/Unit/QuizContentValidationTests.cs`
- [X] T042 [P] [US4] Add catalog tests for partial invalid filtering, source failure status, friendly source messages, and non-sensitive logging in `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`
- [X] T043 [P] [US4] Add integration tests for empty catalog, invalid question file, unavailable story references, unknown question id fallback, and no internal exception/path output in `StoryBook.Tests/Integration/QuizPagesTests.cs`

### Implementation for User Story 4

- [X] T044 [US4] Implement full quiz schema and content validation rules with non-sensitive logging in `StoryBook/Services/QuizContentValidator.cs`
- [X] T045 [US4] Integrate validation results, invalid question filtering, source status summaries, and friendly fallback decisions in `StoryBook/Services/QuizCatalogService.cs`
- [X] T046 [US4] Add quiz integration fixture helpers for replacing content paths and services in `StoryBook.Tests/Integration/QuizPageTestFixture.cs`
- [X] T047 [US4] Render child-friendly empty catalog, no-question, invalid question id, and source-unavailable states in `StoryBook/Pages/Quiz/Index.cshtml`
- [X] T048 [US4] Add empty and error state layout styles in `StoryBook/wwwroot/css/quiz.css`
- [X] T049 [US4] Run US4 validation and fallback tests, then record the result in `specs/008-quiz-challenges/quickstart.md`

**Checkpoint**: User Story 4 is independently functional and testable.

---

## Phase 7: User Story 5 - 保持雙語、主題與可及性一致 (Priority: P2)

**Goal**: Quiz UI follows existing language preference, theme contract, keyboard accessibility, and responsive layout requirements.

**Independent Test**: Switch language and theme, open `/quiz`, answer a question with keyboard, and verify bilingual text, visible focus, no theme selector, and no overflow at 375px, 768px, and 1366px widths.

### Tests for User Story 5

- [ ] T050 [P] [US5] Add unit tests for quiz text fallback, invalid language fallback, no blank projection text, and bilingual source labels in `StoryBook.Tests/Unit/QuizCatalogServiceTests.cs`
- [ ] T051 [P] [US5] Add script contract tests for no `localStorage`, `sessionStorage`, cookie, fetch, History API, jQuery, timer/countdown dependency, drag/drop dependency, precision gesture dependency, or client-side correctness calculation in `StoryBook.Tests/Unit/QuizScriptContractTests.cs`
- [ ] T052 [P] [US5] Add integration tests for English rendering, zh-TW rendering, theme selector absence, layout theme attributes, accessible names, fieldset legend, and live feedback metadata in `StoryBook.Tests/Integration/QuizPagesTests.cs`

### Implementation for User Story 5

- [ ] T053 [US5] Apply existing language preference metadata and zh-TW fallback values in `StoryBook/Pages/Quiz/Index.cshtml.cs`
- [ ] T054 [US5] Add bilingual `data-i18n-*`, aria labels, source labels, prompt text, option text, feedback text, and explanation text in `StoryBook/Pages/Quiz/Index.cshtml`
- [ ] T055 [US5] Use existing site theme tokens, visible focus styles, and responsive constraints in `StoryBook/wwwroot/css/quiz.css`
- [ ] T056 [US5] Finalize quiz progressive enhancement without storage, fetch, History API, jQuery, or correctness logic in `StoryBook/wwwroot/js/quiz.js`
- [ ] T057 [US5] Ensure quiz CSS and script are referenced without inline event handlers in `StoryBook/Pages/Quiz/Index.cshtml`
- [ ] T058 [US5] Run US5 language, theme, keyboard, responsive, no timer/countdown, no drag/drop, and no precision-gesture acceptance checks, then record the result in `specs/008-quiz-challenges/quickstart.md`

**Checkpoint**: User Story 5 is independently functional and testable.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final quality gates that span all quiz stories.

- [ ] T059 [P] Review XML documentation, nullable annotations, and `.editorconfig` consistency in `StoryBook/Models/Quiz*.cs` and `StoryBook/Services/Quiz*.cs`
- [ ] T060 [P] Review child-friendly Traditional Chinese and English copy in `StoryBook/Data/quiz-questions.json` and `StoryBook/Pages/Quiz/Index.cshtml`
- [ ] T061 Review non-sensitive logging and absence of absolute paths, exception details, stack traces, secrets, and answer-result persistence in `StoryBook/Services/QuizCatalogService.cs`, `StoryBook/Services/QuizContentValidator.cs`, and `StoryBook/Pages/Quiz/Index.cshtml.cs`
- [ ] T062 Run `dotnet test StoryBook2.sln` and record final automated verification in `specs/008-quiz-challenges/quickstart.md`
- [ ] T063 Run local warm-load performance smoke checks for `/quiz`, scope navigation, next-question navigation, and answer feedback timing, then record p95/FCP/LCP/1-second response evidence in `specs/008-quiz-challenges/quickstart.md`
- [ ] T064 Run the full manual acceptance checklist and update evidence checkboxes in `specs/008-quiz-challenges/quickstart.md`
- [ ] T065 Record SC-001 and SC-010 evidence in `specs/008-quiz-challenges/quickstart.md`: at least 20 quiz-entry discovery or equivalent browser task records with at least 19 successes within 5 seconds, and at least 10 explanation-learning records with at least 9 successful key learning points

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user stories.
- **User Stories (Phase 3+)**: Depend on Foundational completion.
- **Polish (Phase 8)**: Depends on all selected user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational; no dependency on other stories.
- **US2 (P1)**: Starts after Foundational; depends on the question projection shape from US1 if implemented sequentially.
- **US3 (P1)**: Starts after Foundational; depends on catalog/story services and can be implemented alongside US2 after US1 projection shape is agreed.
- **US4 (P1)**: Starts after Foundational; hardens service and fallback behavior used by US1-US3.
- **US5 (P2)**: Starts after Foundational; can run after markup contracts from US1-US2 are stable.

### Priority Guidance

- **First demonstrable MVP**: Complete Phase 1, Phase 2, and US1.
- **First releasable P1 slice**: Complete Phase 1, Phase 2, US1, US2, US3, and US4.
- **Full feature**: Complete all phases including US5 and Polish.

---

## Parallel Opportunities

- Setup tasks T002 and T003 can run in parallel after T001.
- Foundational model tasks T004-T008 and T010-T012 can run in parallel because they touch separate files.
- Test tasks at the start of each user story can run in parallel.
- Home and explore entry tasks T022 and T023 can run in parallel.
- US2 JavaScript/CSS work can run in parallel with server-side answer evaluation after the DOM contract in T030 is clear.
- US3 related-story tests and catalog JSON review can run in parallel with Razor rendering once the projection contract is defined.
- US5 unit, script, and integration tests can be authored in parallel.

---

## Parallel Example: User Story 1

```bash
# Parallelizable test authoring:
Task: "T015 [US1] Add unit tests in StoryBook.Tests/Unit/QuizCatalogServiceTests.cs"
Task: "T016 [US1] Add integration tests in StoryBook.Tests/Integration/QuizPagesTests.cs"

# Parallelizable entry work:
Task: "T022 [US1] Add home entry in StoryBook/Pages/Index.cshtml"
Task: "T023 [US1] Add explore entry in StoryBook/Pages/Explore/Index.cshtml"
```

## Parallel Example: User Story 2

```bash
Task: "T026 [US2] Add answer evaluation unit tests in StoryBook.Tests/Unit/QuizCatalogServiceTests.cs"
Task: "T027 [US2] Add form post integration tests in StoryBook.Tests/Integration/QuizPagesTests.cs"
Task: "T031 [US2] Add feedback styles in StoryBook/wwwroot/css/quiz.css"
Task: "T032 [US2] Add progressive submit guard in StoryBook/wwwroot/js/quiz.js"
```

## Parallel Example: User Story 3

```bash
Task: "T034 [US3] Add related story unit tests in StoryBook.Tests/Unit/QuizCatalogServiceTests.cs"
Task: "T035 [US3] Add related story integration tests in StoryBook.Tests/Integration/QuizPagesTests.cs"
Task: "T037 [US3] Verify relatedStories in StoryBook/Data/quiz-questions.json"
Task: "T039 [US3] Add related story styles in StoryBook/wwwroot/css/quiz.css"
```

## Parallel Example: User Story 4

```bash
Task: "T041 [US4] Add content validation tests in StoryBook.Tests/Unit/QuizContentValidationTests.cs"
Task: "T042 [US4] Add source failure tests in StoryBook.Tests/Unit/QuizCatalogServiceTests.cs"
Task: "T043 [US4] Add fallback integration tests in StoryBook.Tests/Integration/QuizPagesTests.cs"
```

## Parallel Example: User Story 5

```bash
Task: "T050 [US5] Add language fallback unit tests in StoryBook.Tests/Unit/QuizCatalogServiceTests.cs"
Task: "T051 [US5] Add script contract tests in StoryBook.Tests/Unit/QuizScriptContractTests.cs"
Task: "T052 [US5] Add language/theme/accessibility integration tests in StoryBook.Tests/Integration/QuizPagesTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: US1.
4. Stop and validate `/quiz` entry, scope selection, and one visible valid question.

### Incremental Delivery

1. Add US1 for route, entries, scope selection, and initial question display.
2. Add US2 for answer submission and friendly feedback.
3. Add US3 for related story review links.
4. Add US4 for strict catalog validation and graceful fallback.
5. Add US5 for bilingual, theme, keyboard, and responsive polish.
6. Complete Phase 8 quality gates before delivery.

### TDD Order Within Each Story

1. Write the story tests and confirm they fail for the intended missing behavior.
2. Implement the smallest service/PageModel/view changes needed to pass.
3. Run the story-specific tests.
4. Update quickstart evidence for that story.
5. Move to the next story only after the current story can be demonstrated independently.
