# Tasks: 小小探險護照

**Input**: Design documents from `/specs/007-reading-passport/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: 本功能規格與憲章明確要求測試優先；每個使用者故事都先建立失敗測試，再實作。

**Organization**: 任務依使用者故事分組，讓每個故事可獨立實作與驗證。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行，因為修改不同檔案且不依賴尚未完成的任務
- **[Story]**: 使用者故事標籤，僅出現在使用者故事階段
- 每個任務都包含明確檔案路徑

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 先建立護照功能的測試與驗收入口，不加入 runtime 行為。

- [X] T001 [P] Create Passport integration fixture shell in `StoryBook.Tests/Integration/PassportPageTestFixture.cs`
- [X] T002 [P] Create Passport integration test class shell in `StoryBook.Tests/Integration/PassportPagesTests.cs`
- [X] T003 [P] Create Passport preference unit test class shell in `StoryBook.Tests/Unit/PassportPreferenceServiceTests.cs`
- [X] T004 [P] Create Passport catalog unit test class shell in `StoryBook.Tests/Unit/PassportCatalogServiceTests.cs`
- [X] T005 [P] Create Passport script contract test class shell in `StoryBook.Tests/Unit/PassportScriptContractTests.cs`
- [X] T006 [P] Add reading-passport manual acceptance evidence checklist placeholder in `specs/007-reading-passport/quickstart.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有護照故事共用的 storage metadata、資料模型、asset hook 與 DI 註冊。

**CRITICAL**: 此階段完成前不要開始使用者故事實作。

- [X] T007 [P] Add failing storage metadata tests for key `storybook.passport`, version `1`, allowed sources and five badges in `StoryBook.Tests/Unit/PassportPreferenceServiceTests.cs`
- [X] T008 Implement `PassportPreferenceService` storage key, state version, allowed source codes and fixed badge metadata in `StoryBook/Services/PassportPreferenceService.cs`
- [X] T009 [P] Add `PassportBadgeMilestone` enum values `CompletedCountAtLeast`, `CompletedAllInSource` and `CompletedAllStories` in `StoryBook/Models/PassportBadgeMilestone.cs`
- [X] T010 [P] Add `PassportBadgeDefinition` model with XML documentation and bilingual label/description fields in `StoryBook/Models/PassportBadgeDefinition.cs`
- [X] T011 [P] Add `PassportStoryItem` model with source, route, ordering, bilingual text and image metadata in `StoryBook/Models/PassportStoryItem.cs`
- [X] T012 [P] Add `PassportSourceStatus` model with friendly failure messages and reason code in `StoryBook/Models/PassportSourceStatus.cs`
- [X] T013 [P] Add `PassportCatalogSnapshot` model with story totals and source failure flags in `StoryBook/Models/PassportCatalogSnapshot.cs`
- [X] T014 Register `PassportPreferenceService` in the service container in `StoryBook/Program.cs`
- [X] T015 Add `UsePassportAssets` stylesheet hook for `~/css/passport.css` in `StoryBook/Pages/Shared/_Layout.cshtml`
- [X] T016 Verify foundational tests fail before implementation and then pass after T008-T015 in `StoryBook.Tests/Unit/PassportPreferenceServiceTests.cs`

**Checkpoint**: Storage metadata, shared models, asset loading hook and DI baseline are ready.

---

## Phase 3: User Story 1 - 標記已讀故事朋友 (Priority: P1)

**Goal**: 孩子在恐龍或水族館詳情頁明確按下完成控制後，同一瀏覽器記住該故事已讀且不重複計算。

**Independent Test**: 開啟 `/dinosaurs/triceratops` 或 `/aquarium/sea-turtle`，按下「我讀完了」，重新整理後仍顯示已讀狀態；再次按下不新增重複項目。

### Tests for User Story 1

> 先寫這些測試，確認在實作前失敗。

- [X] T017 [US1] Add failing integration tests for dinosaur and aquarium detail completion regions, valid source/slug attributes, passport link and no completion control on not-found/load-failure pages in `StoryBook.Tests/Integration/PassportPagesTests.cs`
- [X] T018 [P] [US1] Add failing script contract tests for explicit click-only completion, state version `1`, de-duplication and no auto-complete on page load/scroll/modal in `StoryBook.Tests/Unit/PassportScriptContractTests.cs`

### Implementation for User Story 1

- [X] T019 [US1] Implement detail-page state read, normalize, save, de-duplicate and status update behavior in `StoryBook/wwwroot/js/passport.js`
- [X] T020 [US1] Add valid-story completion region, bilingual button text, status region and passport link to dinosaur details in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [X] T021 [US1] Add valid-story completion region, bilingual button text, status region and passport link to aquarium details in `StoryBook/Pages/Aquarium/Details.cshtml`
- [X] T022 [US1] Set `UsePassportAssets` and include `~/js/passport.js` on dinosaur details without changing existing dinosaur script behavior in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [X] T023 [US1] Set `UsePassportAssets` and include `~/js/passport.js` on aquarium details without changing existing aquarium script behavior in `StoryBook/Pages/Aquarium/Details.cshtml`
- [X] T024 [US1] Add completion control, read status and passport link styling with visible focus and 44px targets in `StoryBook/wwwroot/css/passport.css`
- [X] T025 [US1] Run targeted US1 integration and script contract checks in `StoryBook.Tests/Integration/PassportPagesTests.cs` and `StoryBook.Tests/Unit/PassportScriptContractTests.cs`

**Checkpoint**: US1 可獨立示範；詳情頁完成標記可保存、重新整理後可讀回，且同一故事只計一次。

---

## Phase 4: User Story 2 - 查看我的探險護照 (Priority: P1)

**Goal**: `/passport` 顯示已讀數、總故事數、已讀清單、固定徽章里程碑與連回既有詳情頁的一般連結。

**Independent Test**: 先標記至少兩篇故事，進入 `/passport` 後確認進度、清單、徽章狀態與 `/dinosaurs/{slug}`、`/aquarium/{slug}` 一般連結都正確。

### Tests for User Story 2

> 先寫這些測試，確認在實作前失敗。

- [X] T026 [P] [US2] Add failing catalog projection tests for total count 23, dinosaur-before-aquarium ordering, canonical hrefs, no blank fallback text, five badge definitions and partial/all source failure status in `StoryBook.Tests/Unit/PassportCatalogServiceTests.cs`
- [X] T027 [US2] Add failing `/passport` route, DOM contract, home entry, shared navigation entry, badge shell, story item shell and theme selector absence tests in `StoryBook.Tests/Integration/PassportPagesTests.cs`

### Implementation for User Story 2

- [X] T028 [US2] Implement `PassportCatalogService` composition from dinosaur and aquarium catalogs with non-sensitive logging in `StoryBook/Services/PassportCatalogService.cs`
- [X] T029 [US2] Register `PassportCatalogService` in the service container in `StoryBook/Program.cs`
- [X] T030 [US2] Implement `/passport` PageModel snapshot loading and friendly source failure state in `StoryBook/Pages/Passport/Index.cshtml.cs`
- [X] T031 [US2] Render `/passport` summary region, story metadata nodes, badge shells, empty/error/fallback regions and no personal state in `StoryBook/Pages/Passport/Index.cshtml`
- [X] T032 [US2] Add shared navigation anchor to `/passport` with bilingual accessible text in `StoryBook/Pages/Shared/_Layout.cshtml`
- [X] T033 [US2] Add home page passport action to `/passport` with bilingual accessible text in `StoryBook/Pages/Index.cshtml`
- [X] T034 [US2] Implement passport-page read list, completed count, total count and badge state rendering from normalized localStorage state in `StoryBook/wwwroot/js/passport.js`
- [X] T035 [US2] Add passport summary, badge grid, read list, source status and empty/error state styles in `StoryBook/wwwroot/css/passport.css`
- [X] T036 [US2] Run targeted US2 catalog and integration checks in `StoryBook.Tests/Unit/PassportCatalogServiceTests.cs` and `StoryBook.Tests/Integration/PassportPagesTests.cs`

**Checkpoint**: US2 可獨立示範；`/passport` 可從首頁或共用導覽進入，並依本機護照狀態顯示進度與連結。

---

## Phase 5: User Story 3 - 清除這台瀏覽器的護照 (Priority: P1)

**Goal**: 使用者可在 `/passport` 清除這台瀏覽器的閱讀護照，且不影響語言、主題或其他站內偏好。

**Independent Test**: 完成至少兩篇故事、設定英文與深色模式後，在 `/passport` 清除護照；已讀清單歸零，`storybook.language` 與 `storybook.theme` 保持原值。

### Tests for User Story 3

> 先寫這些測試，確認在實作前失敗。

- [X] T037 [US3] Add failing integration tests for visible clear control, explicit confirmation UI and child-friendly clear text in `StoryBook.Tests/Integration/PassportPagesTests.cs`
- [X] T038 [P] [US3] Add failing script contract tests for clear confirmation, `storybook.passport` scoped reset to `{ version: 1, completedStories: [] }`, no `localStorage.clear()`, and no language/theme key mutation in `StoryBook.Tests/Unit/PassportScriptContractTests.cs`

### Implementation for User Story 3

- [X] T039 [US3] Render clear passport button, confirmation region, confirm/cancel controls and bilingual text in `StoryBook/Pages/Passport/Index.cshtml`
- [X] T040 [US3] Implement clear confirmation, scoped valid empty-state reset and UI refresh behavior in `StoryBook/wwwroot/js/passport.js`
- [X] T041 [US3] Add clear confirmation layout, button states and focus styling in `StoryBook/wwwroot/css/passport.css`
- [X] T042 [US3] Run targeted US3 clear-flow checks in `StoryBook.Tests/Unit/PassportScriptContractTests.cs` and `StoryBook.Tests/Integration/PassportPagesTests.cs`

**Checkpoint**: US3 可獨立驗證；清除動作只把護照資料重設為有效空狀態，不碰其他 storage key。

---

## Phase 6: User Story 4 - 保存受限時仍可閱讀 (Priority: P2)

**Goal**: localStorage 無法讀取、寫入或既有資料無效時，故事頁仍可閱讀，護照功能以友善文字降級。

**Independent Test**: 模擬 storage read/write blocked 與 invalid JSON/version/source/slug 後，詳情頁與 `/passport` 不崩潰、不改存其他位置，並顯示兒童友善提示。

### Tests for User Story 4

> 先寫這些測試，確認在實作前失敗。

- [X] T043 [P] [US4] Add failing script contract tests for `try/catch` localStorage read/write/remove, invalid shape normalization, ignored item counting and no cookie/sessionStorage/fetch/history fallback in `StoryBook.Tests/Unit/PassportScriptContractTests.cs`
- [X] T044 [P] [US4] Add failing integration tests for storage warning DOM, invalid-data friendly text and readable detail/passport fallback regions in `StoryBook.Tests/Integration/PassportPagesTests.cs`

### Implementation for User Story 4

- [X] T045 [US4] Implement storage status state machine for `available`, `read-blocked`, `write-blocked` and `invalid-data` in `StoryBook/wwwroot/js/passport.js`
- [X] T046 [US4] Implement strict state normalization for version, allowed sources, kebab-case slug, known catalog items, duplicate removal and extra property dropping in `StoryBook/wwwroot/js/passport.js`
- [X] T047 [US4] Add passport page storage warning, invalid-data fallback and all-sources-failed friendly regions in `StoryBook/Pages/Passport/Index.cshtml`
- [X] T048 [US4] Add detail-page storage warning/status text attributes for dinosaur completion controls in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [X] T049 [US4] Add detail-page storage warning/status text attributes for aquarium completion controls in `StoryBook/Pages/Aquarium/Details.cshtml`
- [X] T050 [US4] Add degraded storage warning and invalid-data styles in `StoryBook/wwwroot/css/passport.css`
- [X] T051 [US4] Run targeted US4 fallback checks in `StoryBook.Tests/Unit/PassportScriptContractTests.cs` and `StoryBook.Tests/Integration/PassportPagesTests.cs`

**Checkpoint**: US4 可獨立驗證；保存受限時不破壞既有閱讀流程，也不把護照資料移到網址、cookie、session 或外部服務。

---

## Phase 7: User Story 5 - 保持雙語、主題與可及性一致 (Priority: P2)

**Goal**: 護照頁與完成控制沿用既有繁中/英文語言偏好、整站主題與可及性規則。

**Independent Test**: 切換語言與主題後開啟詳情頁與 `/passport`，確認文字、aria-label、焦點、44px 操作目標、無 theme selector、無水平溢出與無空白 fallback。

### Tests for User Story 5

> 先寫這些測試，確認在實作前失敗。

- [X] T052 [US5] Add failing integration tests for bilingual text/aria contracts, zh-TW fallback, theme selector absence on `/passport`, accessible names and no blank title/summary/source labels in `StoryBook.Tests/Integration/PassportPagesTests.cs`
- [X] T053 [P] [US5] Add failing static asset contract tests for no jQuery dependency, no inline router/history mutation, no inline script or external resource dependency, focus-visible selectors and responsive class coverage in `StoryBook.Tests/Unit/PassportScriptContractTests.cs`

### Implementation for User Story 5

- [X] T054 [US5] Complete bilingual `data-i18n-*`, `data-aria-label-*`, language storage key and zh-TW fallback metadata in `StoryBook/Pages/Passport/Index.cshtml`
- [X] T055 [US5] Complete bilingual completion control labels and fallback-safe text for dinosaur details in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [X] T056 [US5] Complete bilingual completion control labels and fallback-safe text for aquarium details in `StoryBook/Pages/Aquarium/Details.cshtml`
- [X] T057 [US5] Implement language switching and aria-label refresh for passport summary, badges, read list, status messages and clear confirmation in `StoryBook/wwwroot/js/passport.js`
- [X] T058 [US5] Finalize responsive layout, visible focus, theme token usage, contrast-safe states and 44px controls in `StoryBook/wwwroot/css/passport.css`
- [X] T059 [US5] Run targeted US5 bilingual/theme/accessibility/static asset contract checks in `StoryBook.Tests/Integration/PassportPagesTests.cs` and `StoryBook.Tests/Unit/PassportScriptContractTests.cs`

**Checkpoint**: US5 可獨立驗證；護照功能與既有語言、主題、鍵盤操作和回應式體驗一致。

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 完成交付前的整體品質、回歸與手動驗收。

- [ ] T060 [P] Review `passport.js` for forbidden persistence and navigation APIs against `specs/007-reading-passport/contracts/ui-routes.md`
- [ ] T061 [P] Review generated storage shape against `specs/007-reading-passport/contracts/passport-state.schema.json`
- [ ] T062 Run `dotnet restore StoryBook2.sln` for dependency validation in `StoryBook2.sln`
- [ ] T063 Run `dotnet build StoryBook2.sln` and confirm no new warnings in `StoryBook2.sln`
- [ ] T064 Run `dotnet test StoryBook2.sln` and confirm all unit/integration tests pass in `StoryBook2.sln`
- [ ] T065 Execute and record quickstart manual acceptance evidence for routes, localStorage, clear flow, language, theme, keyboard, 375/768/1366px layout, at least 20 representative 5-second find-and-operate tasks for SC-001/SC-003, and 3 warm-load checks each for `/passport`, `/dinosaurs/triceratops` and `/aquarium/sea-turtle` in `specs/007-reading-passport/quickstart.md`
- [ ] T066 Review final implementation for secrets, personal data fields, external API usage, forbidden storage keys, inline scripts and external resource dependencies in `StoryBook/wwwroot/js/passport.js`, `StoryBook/Pages/Passport/Index.cshtml`, `StoryBook/Pages/Dinosaurs/Details.cshtml` and `StoryBook/Pages/Aquarium/Details.cshtml`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: 無前置依賴，可立即開始。
- **Phase 2 Foundational**: 依賴 Phase 1，並阻擋所有使用者故事。
- **Phase 3 US1**: 依賴 Phase 2，可作為 MVP 第一個切片。
- **Phase 4 US2**: 依賴 Phase 2，可與 US1 平行開發，但完整手動情境會使用 US1 產生的本機狀態。
- **Phase 5 US3**: 依賴 US2 的 `/passport` 頁面與 clear UI 容器。
- **Phase 6 US4**: 依賴 US1 詳情頁控制與 US2 `/passport` DOM contract。
- **Phase 7 US5**: 依賴 US1/US2 的主要 UI，可在 US3/US4 後整體收斂。
- **Phase 8 Polish**: 依賴目標使用者故事完成。

### User Story Dependencies

- **US1 (P1)**: Foundation 後可開始；不依賴其他故事。
- **US2 (P1)**: Foundation 後可開始；不依賴 US1 的程式碼，但使用同一 storage contract。
- **US3 (P1)**: 需要 US2 的 `/passport` 頁面。
- **US4 (P2)**: 需要 US1/US2 的 detail/page UI 才能驗證降級狀態。
- **US5 (P2)**: 需要 US1/US2 主要 UI，並覆蓋 US3/US4 的文字與焦點狀態。

### Within Each User Story

- 測試任務必須先完成，並確認在對應實作前失敗。
- Models before services；services before PageModel；PageModel before Razor DOM contract；DOM contract before JavaScript enhancement。
- 每個故事完成後先執行該故事的 targeted tests，再進入下一個優先級故事。

---

## Parallel Opportunities

- Phase 1 的 T001-T006 可平行建立測試與驗收入口。
- Phase 2 的 T009-T013 可平行建立模型檔，T007 可與模型檔平行撰寫。
- US1 的 T017 與 T018 可平行撰寫測試；T020 與 T021 可在 T019 contract 明確後平行實作。
- US2 的 T026 與 T027 可平行撰寫測試；T032 與 T033 可平行新增導覽入口。
- US3 的 T037 與 T038 可平行撰寫測試。
- US4 的 T043 與 T044 可平行撰寫測試；T048 與 T049 可平行更新兩個詳情頁。
- US5 的 T052 與 T053 可平行撰寫測試；T055 與 T056 可平行更新兩個詳情頁。
- Phase 8 的 T060 與 T061 可平行做靜態合約檢查。

---

## Parallel Example: User Story 1

```bash
Task: "T017 Add failing integration tests in StoryBook.Tests/Integration/PassportPagesTests.cs"
Task: "T018 Add failing script contract tests in StoryBook.Tests/Unit/PassportScriptContractTests.cs"
```

```bash
Task: "T020 Add dinosaur completion region in StoryBook/Pages/Dinosaurs/Details.cshtml"
Task: "T021 Add aquarium completion region in StoryBook/Pages/Aquarium/Details.cshtml"
```

## Parallel Example: User Story 2

```bash
Task: "T026 Add failing catalog projection tests in StoryBook.Tests/Unit/PassportCatalogServiceTests.cs"
Task: "T027 Add failing passport route and DOM tests in StoryBook.Tests/Integration/PassportPagesTests.cs"
```

```bash
Task: "T032 Add shared navigation passport anchor in StoryBook/Pages/Shared/_Layout.cshtml"
Task: "T033 Add home page passport action in StoryBook/Pages/Index.cshtml"
```

## Parallel Example: User Story 4

```bash
Task: "T043 Add failing storage contract tests in StoryBook.Tests/Unit/PassportScriptContractTests.cs"
Task: "T044 Add failing storage warning integration tests in StoryBook.Tests/Integration/PassportPagesTests.cs"
```

```bash
Task: "T048 Add dinosaur storage warning text in StoryBook/Pages/Dinosaurs/Details.cshtml"
Task: "T049 Add aquarium storage warning text in StoryBook/Pages/Aquarium/Details.cshtml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 US1.
4. Stop and validate: details pages can mark one story complete, refresh, and avoid duplicate state.
5. Demo US1 before adding `/passport` if a very small MVP checkpoint is needed.

### P1 Increment

1. Add US2 to display `/passport` progress and badges.
2. Add US3 to clear local passport data without touching language/theme.
3. Validate P1 end-to-end: mark dinosaur, mark aquarium, view passport, clear passport.

### Incremental Delivery

1. Foundation ready.
2. US1 complete and tested.
3. US2 complete and tested.
4. US3 complete and tested.
5. US4 storage failure behavior complete and tested.
6. US5 bilingual/theme/accessibility behavior complete and tested.
7. Run Phase 8 full validation.

### Parallel Team Strategy

1. Team finishes Setup + Foundational together.
2. Developer A: US1 detail completion.
3. Developer B: US2 catalog projection and `/passport` page.
4. Developer C: US3 clear flow after US2 page contract exists.
5. Developer D: US4/US5 hardening after primary UI contracts are stable.

---

## Notes

- `[P]` tasks touch different files or are independent test-writing tasks.
- `[US1]` through `[US5]` map directly to the five user stories in `spec.md`.
- All story test tasks must fail before the corresponding implementation task starts.
- Do not add database, Web API, login, cookie/session persistence, external services, jQuery dependency, or frontend build pipeline.
- Do not use bulk deletion commands; generated build artifacts under `bin/` and `obj/` must not be manually edited.
