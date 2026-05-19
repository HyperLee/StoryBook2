# Tasks: 內容比較器

**Input**: Design documents from `/specs/005-story-friend-compare/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/ui-routes.md`, `quickstart.md`

**Tests**: 本功能規格與專案憲章明確要求測試優先；Phase 1 先定義失敗測試、靜態合約測試與人工驗收紀錄要求，再進入任何 production implementation task。

**Organization**: Phase 1 groups all failing tests and validation-definition work first; later phases group implementation by user story so each story can be independently implemented and verified.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可與同階段其他任務平行處理，因為使用不同檔案且不依賴未完成任務
- **[Story]**: 使用者故事標籤，僅用於使用者故事階段
- 每個任務描述都包含明確檔案路徑

## Phase 1: Test & Validation Definition (Failing First)

**Purpose**: 先建立可執行或可審查的測試、靜態合約測試與人工驗收紀錄要求，確保後續 implementation task 皆由測試或驗收契約驅動。

**CRITICAL**: No production implementation task can begin until this phase is complete and the automated tests have been observed failing or not compiling for the expected missing implementation.

- [X] T001 [P] Create compare integration test fixture scaffold in `StoryBook.Tests/Integration/ComparePageTestFixture.cs`.
- [X] T002 [US1] Add failing integration tests for `GET /compare`, default `zh-TW` text, first/second select controls, clear button, status region, and absence of theme selector in `StoryBook.Tests/Integration/ComparePagesTests.cs`.
- [X] T003 [US1] Add failing integration tests that homepage `/` and `/explore` expose normal anchor links to `/compare` in `StoryBook.Tests/Integration/ComparePagesTests.cs`.
- [X] T004 [US2] Add failing unit tests for full 23-candidate projection, stable id format, source ordering, source `SortOrder`, and detail hrefs in `StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs`.
- [X] T005 [P] [US2] Add failing static client script contract tests for duplicate/clear message hooks, required page-local selection hooks, no history writes, and no storage writes in `StoryBook.Tests/Unit/CompareScriptContractTests.cs`.
- [X] T006 [US2] Add failing integration tests for candidate metadata, comparison table rows, normal detail links, and hidden table initial state in `StoryBook.Tests/Integration/ComparePagesTests.cs`.
- [X] T007 [US3] Add failing integration tests for bilingual page text, bilingual accessible names, theme boot attributes, no compare theme selector, and preserve-state theme contract in `StoryBook.Tests/Integration/ComparePagesTests.cs`.
- [X] T008 [P] [US3] Add failing unit tests for localized comparison field/value fallback and nonblank `zh-TW` fallback values in `StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs`.
- [X] T009 [US4] Extend catalog path override and limited-candidate helper support for compare integration tests in `StoryBook.Tests/Integration/ComparePageTestFixture.cs`.
- [X] T010 [US4] Add failing unit tests for partial source failure, all-source failure, fewer-than-two candidates, friendly statuses, and sanitized logging in `StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs`.
- [X] T011 [US4] Add failing integration tests for partial failure, not-enough-candidates, all-failed, home link, no internal error details, and 1-second friendly failure-state expectation in `StoryBook.Tests/Integration/ComparePagesTests.cs`.
- [X] T012 [P] Define representative usability evidence log format for SC-001 and SC-010, including 20 entry-finding attempts and 10 compare-understanding attempts, in `specs/005-story-friend-compare/quickstart.md`.
- [X] T013 [P] Define manual browser validation checklist for duplicate selection, clear behavior, theme-preserved selected candidates, and 1-second failure-state display in `specs/005-story-friend-compare/quickstart.md`.
- [X] T014 [P] Define negative-scope audit checklist for no authentication requirement, no external website search, no real-time translation, and no new external content source in `specs/005-story-friend-compare/quickstart.md`.

---

## Phase 2: Setup & Foundational (Blocking Prerequisites)

**Purpose**: 建立比較器需要的前端資產檔案骨架、共用 projection DTO、service skeleton 與 DI 註冊。

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T015 [P] Create compare stylesheet scaffold in `StoryBook/wwwroot/css/compare.css`.
- [X] T016 [P] Create compare page script scaffold in `StoryBook/wwwroot/js/compare.js`.
- [X] T017 [P] Create `ComparisonFieldValue` model with XML docs and localized fallback helpers in `StoryBook/Models/ComparisonFieldValue.cs`.
- [X] T018 [P] Create `ComparisonFieldDefinition` model with XML docs and localized label/not-applicable helpers in `StoryBook/Models/ComparisonFieldDefinition.cs`.
- [X] T019 [P] Create `ComparisonCandidate` model with XML docs, stable id, detail href, source labels, bilingual fields, and field values in `StoryBook/Models/ComparisonCandidate.cs`.
- [X] T020 [P] Create `ComparisonSourceStatus` model with XML docs, availability count, and friendly bilingual message helpers in `StoryBook/Models/ComparisonSourceStatus.cs`.
- [X] T021 Create `ComparisonCatalogSnapshot` model with candidates, field definitions, source statuses, and computed failure flags in `StoryBook/Models/ComparisonCatalogSnapshot.cs`.
- [X] T022 [P] Create `ComparisonSelectionState` model documenting page-local statuses in `StoryBook/Models/ComparisonSelectionState.cs`.
- [X] T023 Create `ComparisonCatalogService` skeleton that injects dinosaur/aquarium catalog services and logger in `StoryBook/Services/ComparisonCatalogService.cs`.
- [X] T024 Register `ComparisonCatalogService` with singleton lifetime in `StoryBook/Program.cs`.

**Checkpoint**: 共用模型與 service skeleton 可編譯，使用者故事可開始以測試優先方式實作。

---

## Phase 3: User Story 1 - 進入內容比較器 (Priority: P1) MVP

**Goal**: 使用者可從首頁與 `/explore` 進入 `/compare`，並看到兩位故事朋友的選擇控制項。

**Independent Test**: 從 `/` 與 `/explore` 的「比較故事朋友」連結進入 `/compare`，確認頁面回傳 200、預設繁體中文、顯示第一位/第二位選擇控制與清除按鈕，且沒有 theme selector。

### Tests for User Story 1

> Covered by Phase 1 tasks T002 and T003. Confirm they fail before starting the implementation tasks below.

### Implementation for User Story 1

- [X] T025 [US1] Implement thin Compare PageModel that sets `UseCompareAssets`, loads `ComparisonCatalogSnapshot`, and exposes page state in `StoryBook/Pages/Compare/Index.cshtml.cs`.
- [X] T026 [US1] Implement canonical `/compare` Razor page with heading, home link, two select controls, clear button, polite status region, and empty initial state in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T027 [US1] Add a discoverable "比較故事朋友" / "Compare story friends" homepage action anchor to `/compare` in `StoryBook/Pages/Index.cshtml`.
- [X] T028 [US1] Add a discoverable "比較故事朋友" / "Compare story friends" exploration page action anchor to `/compare` in `StoryBook/Pages/Explore/Index.cshtml`.
- [X] T029 [US1] Add conditional compare stylesheet loading based on `ViewData["UseCompareAssets"]` in `StoryBook/Pages/Shared/_Layout.cshtml`.
- [X] T030 [US1] Style the initial compare page controls, status region, and entry layout without horizontal overflow in `StoryBook/wwwroot/css/compare.css`.
- [X] T031 [US1] Run `dotnet test StoryBook2.sln --filter ComparePagesTests` and fix failures related to User Story 1 in `StoryBook2.sln`.

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - 選擇兩位故事朋友並查看比較表 (Priority: P1)

**Goal**: 使用者可選擇兩位不同故事朋友，看到來源、名稱、食性、生活區域、生活時期、發現地點、摘要與詳情頁連結的比較表。

**Independent Test**: 在 `/compare` 選擇「暴龍」與「鯊魚」或「三角龍」與「海龜」，確認比較表顯示完整欄位、不適用文字、穩定 id、排序與一般詳情頁 anchor。

### Tests for User Story 2

> Covered by Phase 1 tasks T004, T005, and T006. Confirm they fail before starting the implementation tasks below.

### Implementation for User Story 2

- [X] T032 [US2] Implement dinosaur comparison candidate projection from `DinosaurCatalogService` in `StoryBook/Services/ComparisonCatalogService.cs`.
- [X] T033 [US2] Implement aquarium comparison candidate projection and source/group ordering from `AquariumCatalogService` in `StoryBook/Services/ComparisonCatalogService.cs`.
- [X] T034 [US2] Implement fixed comparison field definitions and child-friendly not-applicable values in `StoryBook/Services/ComparisonCatalogService.cs`.
- [X] T035 [US2] Render candidate options, bilingual metadata, hidden comparison rows, and normal detail anchors in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T036 [US2] Implement page-local first/second selection, one-selected prompt, duplicate prompt, ready state, and clear selection behavior in `StoryBook/wwwroot/js/compare.js`.
- [X] T037 [US2] Add responsive comparison table/card layout for mobile, tablet, and desktop in `StoryBook/wwwroot/css/compare.css`.
- [X] T038 [US2] Run `dotnet test StoryBook2.sln --filter "ComparisonCatalogServiceTests|ComparePagesTests|CompareScriptContractTests"` and fix User Story 2 failures in `StoryBook2.sln`.

**Checkpoint**: User Stories 1 and 2 provide the core P1 comparison workflow.

---

## Phase 5: User Story 3 - 保持雙語、主題與可及性一致 (Priority: P2)

**Goal**: 比較器沿用既有語言偏好與有效主題，支援鍵盤、可見焦點、accessible names，且比較頁不提供新的主題 selector。

**Independent Test**: 切換 `storybook.language` 與首頁主題後開啟 `/compare`，確認文字、候選、欄位、提示、焦點與主題 contract 正確，且選擇狀態不因主題變更而被修改。

### Tests for User Story 3

> Covered by Phase 1 tasks T007 and T008. Confirm they fail before starting the implementation tasks below.

### Implementation for User Story 3

- [X] T039 [US3] Add `data-i18n-*`, `data-aria-label-*`, and nonblank fallback attributes for compare controls, messages, field labels, not-applicable text, and detail actions in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T040 [US3] Update compare script to use localized candidate metadata and preserve selected candidates across language/theme DOM updates without writing storage in `StoryBook/wwwroot/js/compare.js`.
- [X] T041 [US3] Add visible focus, 44x44 CSS px targets, and no-overlap responsive rules for 375px, 768px, and 1366px widths in `StoryBook/wwwroot/css/compare.css`.
- [X] T042 [US3] Run `dotnet test StoryBook2.sln --filter "ComparisonCatalogServiceTests|ComparePagesTests|CompareScriptContractTests"` and fix User Story 3 failures in `StoryBook2.sln`.

**Checkpoint**: Comparison experience is consistent with existing language, theme, and accessibility rules.

---

## Phase 6: User Story 4 - 處理資料載入與空狀態 (Priority: P2)

**Goal**: 當一個或全部故事來源暫時不可用時，比較器仍顯示可用內容或兒童友善狀態，且不暴露內部錯誤。

**Independent Test**: 使用測試 fixture 模擬水族館不可用、候選少於兩筆、全部來源不可用，確認 `/compare` 保持 200、友善訊息、回首頁 action、無檔案路徑或 exception details。

### Tests for User Story 4

> Covered by Phase 1 tasks T009, T010, and T011. Confirm they fail before starting the implementation tasks below.

### Implementation for User Story 4

- [X] T043 [US4] Implement source-level try/catch, `ILogger<ComparisonCatalogService>` warning logs, partial failure flags, all-failed flags, and not-enough candidate detection in `StoryBook/Services/ComparisonCatalogService.cs`.
- [X] T044 [US4] Render partial failure, not-enough-candidates, and all-failed states with polite live regions and home action in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T045 [US4] Ensure compare page output never renders exception type, stack trace, catalog path, `null`, `undefined`, or blank source/status labels in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T046 [US4] Run `dotnet test StoryBook2.sln --filter "ComparisonCatalogServiceTests|ComparePagesTests"` and fix User Story 4 failures in `StoryBook2.sln`.

**Checkpoint**: Partial and full data failure states are independently testable and child-friendly.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 驗證完整流程、回應式/可及性、規格約束與交付品質。

- [X] T047 [P] Execute manual quickstart validation for `/`, `/explore`, `/compare`, selection, language, theme, storage, failure scenarios, and 1-second update expectations in `specs/005-story-friend-compare/quickstart.md`.
- [X] T048 [P] Record representative usability evidence for SC-001 and SC-010 in the format defined by `specs/005-story-friend-compare/quickstart.md`.
- [X] T049 [P] Audit compare JavaScript for no `pushState`, `replaceState`, query initialization, `localStorage`, `sessionStorage`, cookie, or server state writes in `StoryBook/wwwroot/js/compare.js`.
- [X] T050 [P] Audit compare page markup for normal anchors, no theme selector, no blank values, no internal error details, no authentication requirement, no external search, no real-time translation, and no new external content source in `StoryBook/Pages/Compare/Index.cshtml`.
- [X] T051 Run `dotnet build StoryBook2.sln` and fix build warnings or errors in `StoryBook2.sln`.
- [X] T052 Run `dotnet test StoryBook2.sln` and fix remaining regressions in `StoryBook2.sln`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Test & Validation Definition (Phase 1)**: No dependencies; blocks all production implementation.
- **Setup & Foundational (Phase 2)**: Depends on Test & Validation Definition and blocks all user-story implementation.
- **User Story 1 (Phase 3)**: Depends on Foundational.
- **User Story 2 (Phase 4)**: Depends on Foundational; rendered integration work depends on the `/compare` shell from User Story 1.
- **User Story 3 (Phase 5)**: Depends on User Stories 1 and 2 because it verifies language, theme, and accessibility on the completed compare UI.
- **User Story 4 (Phase 6)**: Failure-state implementation depends on Foundational; rendered failure-state delivery depends on User Stories 1 and 2.
- **Polish (Phase 7)**: Depends on whichever user stories are selected for delivery, with full polish after all stories.

### User Story Dependencies

- **US1 (P1)**: Independent navigable page shell after Foundational.
- **US2 (P1)**: Core comparison workflow; depends on US1 for page shell but service and script tasks can start after Foundational.
- **US3 (P2)**: Builds on US1/US2 UI and data contracts.
- **US4 (P2)**: Builds on shared service/page contracts; failure tests are defined in Phase 1, and implementation can proceed after US2 contracts stabilize.

### Within Each User Story

- Phase 1 tests must be written first and observed failing before implementation.
- Models and service projection precede Razor rendering.
- Razor metadata precedes JavaScript DOM behavior.
- JavaScript behavior precedes responsive/focus polish.
- Story-specific tests run before moving to the next story checkpoint.

---

## Parallel Opportunities

- Phase 1 integration, unit, static contract, and validation-definition tasks T001-T014 can run in parallel where file ownership does not overlap.
- Foundational model tasks T017, T018, T019, T020, and T022 can run in parallel.
- After Foundational, US1 entry link tasks T027 and T028 can run in parallel with page styling T030 once the page shell exists.
- US2 projection implementation T032-T034 can run in parallel with static script contract refinement T005 after the expected failing tests are present.
- US3 language/theme integration implementation T039-T041 can be split by Razor, JavaScript, and CSS ownership.
- US4 service failure implementation T043 can proceed in parallel with rendered failure-state implementation T044 after fixture support T009 exists.
- Polish audits T047, T048, T049, and T050 can run in parallel.

---

## Parallel Example: User Story 1

```text
Task: "Add homepage compare entry in StoryBook/Pages/Index.cshtml"
Task: "Add explore compare entry in StoryBook/Pages/Explore/Index.cshtml"
Task: "Style compare page controls in StoryBook/wwwroot/css/compare.css"
```

---

## Parallel Example: User Story 2

```text
Task: "Add projection unit tests in StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs"
Task: "Add client script contract tests in StoryBook.Tests/Unit/CompareScriptContractTests.cs"
Task: "Implement compare script behavior in StoryBook/wwwroot/js/compare.js after metadata exists"
```

---

## Parallel Example: User Story 3

```text
Task: "Add language/theme integration tests in StoryBook.Tests/Integration/ComparePagesTests.cs"
Task: "Add localized fallback unit tests in StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs"
Task: "Add focus and responsive CSS in StoryBook/wwwroot/css/compare.css"
```

---

## Parallel Example: User Story 4

```text
Task: "Add source failure unit tests in StoryBook.Tests/Unit/ComparisonCatalogServiceTests.cs"
Task: "Add rendered failure-state integration tests in StoryBook.Tests/Integration/ComparePagesTests.cs"
Task: "Implement friendly failure states in StoryBook/Pages/Compare/Index.cshtml"
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and confirm expected failing tests/validation contracts exist.
2. Complete Phase 2.
3. Complete Phase 3 (US1) to make `/compare` discoverable and navigable.
4. Validate US1 independently with `dotnet test StoryBook2.sln --filter ComparePagesTests`.
5. For practical P1 delivery, continue immediately into Phase 4 (US2), because US2 provides the core comparison value.

### Incremental Delivery

1. Test & Validation Definition creates failing tests and acceptance evidence requirements.
2. Setup + Foundational create shared files and compile-ready service boundaries.
3. US1 delivers discoverable navigation and compare page shell.
4. US2 delivers the usable comparison table and page-local interaction.
5. US3 adds language, theme, keyboard, focus, and responsive consistency.
6. US4 adds partial/failure data resilience.
7. Polish runs full build, full tests, quickstart validation, evidence recording, and audits.

### Parallel Team Strategy

1. One developer handles failing integration and unit tests for the core compare contracts.
2. A second developer prepares static JavaScript contract tests and quickstart validation/evidence definitions.
3. After Phase 1 is complete, setup/foundation DTO and service skeleton files can be split by file ownership.
4. After US1 shell exists, service projection, Razor metadata, JavaScript behavior, and CSS can be split by file ownership.

---

## Notes

- `[P]` tasks use different files or do not depend on unfinished tasks.
- T005 is a static source/contract test for JavaScript invariants; runtime DOM behavior is validated by T013 and T047 manual browser checks unless a browser automation runner is added in a future plan.
- `[US1]` maps to entering the compare page.
- `[US2]` maps to selecting two story friends and viewing the comparison table.
- `[US3]` maps to bilingual, theme, and accessibility consistency.
- `[US4]` maps to source failure and empty states.
- Avoid adding NuGet packages, JavaScript packages, databases, external APIs, saved comparison state, query state, or JavaScript-only routing.
- Do not modify `StoryBook/Data/dinosaurs.json` or `StoryBook/Data/aquarium.json` unless a test fixture explicitly requires isolated test data.
