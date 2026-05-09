---
description: "兒童恐龍介紹網站實作任務清單"
---

# Tasks: 兒童恐龍介紹網站

**Input**: Design documents from `/specs/001-dinosaur-intro-site/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`  
**Tests**: 必須包含測試；本專案憲章與 plan 要求測試優先，涉及 service、資料驗證、路由、PageModel handler 與跨頁流程時先寫失敗測試。

**Organization**: 任務依使用者故事分組，確保每個故事可獨立實作與驗收。Setup 與 Foundational 完成後，P1 故事優先交付 MVP，P2/P3 可在不同檔案上平行推進。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行，因為任務主要修改不同檔案，且不依賴未完成任務
- **[Story]**: 使用者故事標籤，僅出現在使用者故事階段
- 每個任務描述都包含明確檔案路徑

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立測試專案與 ASP.NET Core 整合測試所需基礎。

- [X] T001 Create xUnit test project configuration with `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`, and `Microsoft.AspNetCore.Mvc.Testing` in `StoryBook.Tests/StoryBook.Tests.csproj`
- [X] T002 Add `StoryBook.Tests/StoryBook.Tests.csproj` to the solution in `StoryBook2.sln`
- [X] T003 Expose the top-level ASP.NET Core entry point for `WebApplicationFactory<Program>` by adding `public partial class Program` in `StoryBook/Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 所有使用者故事共用的資料模型、內容檔、內容驗證、catalog service、語言 fallback 與 DI 設定。此階段完成前不得開始任一使用者故事。

**CRITICAL**: No user story work can begin until this phase is complete.

### Tests for Foundation

- [ ] T004 [P] Write content validation tests for schema shape, 8 profiles, unique kebab-case slugs, sort order, pteranodon category, readable-unit limits, story length, and bilingual alt text in `StoryBook.Tests/Unit/DinosaurContentValidationTests.cs`
- [ ] T005 [P] Write catalog service tests for JSON loading, ordering, slug lookup, unknown slug handling, field fallback behavior, logger calls, and cached lookup/search p95 under 200ms in `StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs`
- [ ] T006 [P] Write language preference tests for `zh-TW`, `en`, invalid values, and `storybook.language` storage key behavior in `StoryBook.Tests/Unit/LanguagePreferenceServiceTests.cs`
- [ ] T007 [P] Write integration test fixture using `WebApplicationFactory<Program>` and shared HTML helpers in `StoryBook.Tests/Integration/DinosaurPageTestFixture.cs`

### Implementation for Foundation

- [ ] T008 Create supported language enum and parsing helpers in `StoryBook/Models/LanguageCode.cs`
- [ ] T009 Create localized text model with fallback accessors in `StoryBook/Models/DinosaurText.cs`
- [ ] T010 Create illustration model with path, bilingual alt text, caption, and style tag fields in `StoryBook/Models/DinosaurIllustration.cs`
- [ ] T011 Create dinosaur story model with title, body, and illustration fields in `StoryBook/Models/DinosaurStory.cs`
- [ ] T012 Create dinosaur profile model with slug, category, sort order, localized facts, summary, main image, story, and keywords in `StoryBook/Models/DinosaurProfile.cs`
- [ ] T013 Create catalog options for the JSON content path in `StoryBook/Services/DinosaurCatalogOptions.cs`
- [ ] T014 Create content validation service for slug, language, readable-unit, pteranodon, story, image, and keyword rules in `StoryBook/Services/DinosaurContentValidator.cs`
- [ ] T015 Create catalog service that loads `StoryBook/Data/dinosaurs.json`, validates content, caches sorted profiles, resolves slugs, searches fields, and logs validation failures, load failures, unknown slugs, and unexpected exceptions in `StoryBook/Services/DinosaurCatalogService.cs`
- [ ] T016 Create language preference service with default `zh-TW`, supported-code parsing, and storage key metadata in `StoryBook/Services/LanguagePreferenceService.cs`
- [ ] T017 Register catalog options, catalog service, validator, and language preference service in `StoryBook/Program.cs`
- [ ] T018 Create complete bilingual catalog content for 暴龍、三角龍、劍龍、腕龍、迅猛龍、翼龍、甲龍、副櫛龍 in `StoryBook/Data/dinosaurs.json`
- [ ] T019 Add child-friendly main illustration assets referenced by the catalog in `StoryBook/wwwroot/images/dinosaurs/tyrannosaurus-rex-main.png`, `StoryBook/wwwroot/images/dinosaurs/triceratops-main.png`, `StoryBook/wwwroot/images/dinosaurs/stegosaurus-main.png`, `StoryBook/wwwroot/images/dinosaurs/brachiosaurus-main.png`, `StoryBook/wwwroot/images/dinosaurs/velociraptor-main.png`, `StoryBook/wwwroot/images/dinosaurs/pteranodon-main.png`, `StoryBook/wwwroot/images/dinosaurs/ankylosaurus-main.png`, and `StoryBook/wwwroot/images/dinosaurs/parasaurolophus-main.png`
- [ ] T020 Add child-friendly story illustration assets referenced by the catalog in `StoryBook/wwwroot/images/dinosaurs/tyrannosaurus-rex-story.png`, `StoryBook/wwwroot/images/dinosaurs/triceratops-story.png`, `StoryBook/wwwroot/images/dinosaurs/stegosaurus-story.png`, `StoryBook/wwwroot/images/dinosaurs/brachiosaurus-story.png`, `StoryBook/wwwroot/images/dinosaurs/velociraptor-story.png`, `StoryBook/wwwroot/images/dinosaurs/pteranodon-story.png`, `StoryBook/wwwroot/images/dinosaurs/ankylosaurus-story.png`, and `StoryBook/wwwroot/images/dinosaurs/parasaurolophus-story.png`

**Checkpoint**: Foundation ready. `dotnet test StoryBook2.sln` should run and the foundational tests should pass before story work starts.

---

## Phase 3: User Story 1 - 瀏覽恐龍介紹 (Priority: P1) MVP

**Goal**: 使用者可從首頁進入恐龍介紹，看到第一筆恐龍資料與必要欄位，並能直接開啟單一恐龍網址。

**Independent Test**: 從 `/` 點擊或檢查「恐龍介紹」入口連到 `/dinosaurs`，確認 `/dinosaurs` 顯示第一筆資料，且 `/dinosaurs/tyrannosaurus-rex` 顯示圖片、名稱、生活時期、食性、發現地點、體型描述與簡短介紹。

### Tests for User Story 1

- [ ] T021 [P] [US1] Write integration tests for home entry, `/dinosaurs` first-profile content, and `/dinosaurs/tyrannosaurus-rex` required HTML fields in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`
- [ ] T022 [P] [US1] Write unit tests for first-profile selection and summary readable-unit limits in `StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs`

### Implementation for User Story 1

- [ ] T023 [US1] Add clear「恐龍介紹」entry link to `/dinosaurs` on the homepage in `StoryBook/Pages/Index.cshtml`
- [ ] T024 [US1] Create dinosaur list PageModel that loads sorted profiles and exposes the first profile in `StoryBook/Pages/Dinosaurs/Index.cshtml.cs`
- [ ] T025 [US1] Create dinosaur list Razor page showing the first profile and links to all profiles in `StoryBook/Pages/Dinosaurs/Index.cshtml`
- [ ] T026 [US1] Create dinosaur details PageModel that resolves valid slugs and exposes friendly not-found state in `StoryBook/Pages/Dinosaurs/Details.cshtml.cs`
- [ ] T027 [US1] Create dinosaur details Razor page with required profile fields, main image, and child-friendly empty/error messages in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T028 [US1] Add feature stylesheet link plus initial desktop/laptop and 768px-minimum responsive layout styles in `StoryBook/wwwroot/css/dinosaurs.css`
- [ ] T029 [US1] Include `dinosaurs.css` for dinosaur pages without changing unrelated template pages in `StoryBook/Pages/Shared/_Layout.cshtml`

**Checkpoint**: User Story 1 is independently demoable from home to dinosaur content and direct detail URLs.

---

## Phase 4: User Story 2 - 換頁瀏覽多隻恐龍 (Priority: P1)

**Goal**: 使用者可用明顯的上一頁與下一頁 anchor link 依排序切換恐龍，且第一筆/最後一筆不會提供無效導覽。

**Independent Test**: 在第一筆詳情頁確認「上一頁」不可用或不顯示，點擊「下一頁」進入第二筆，再點擊「上一頁」回第一筆；最後一筆確認「下一頁」不可用或不顯示。

### Tests for User Story 2

- [ ] T030 [P] [US2] Write unit tests for previous/next navigation at first, middle, last, and single-profile boundaries in `StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs`
- [ ] T031 [P] [US2] Write integration tests for previous/next anchor hrefs and disabled boundary states in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`

### Implementation for User Story 2

- [ ] T032 [US2] Add previous/next navigation result methods to the catalog service in `StoryBook/Services/DinosaurCatalogService.cs`
- [ ] T033 [US2] Expose previous and next profile links from the details PageModel in `StoryBook/Pages/Dinosaurs/Details.cshtml.cs`
- [ ] T034 [US2] Render child-friendly previous/next anchor controls with disabled boundary states in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T035 [US2] Add navigation control styles with visible focus states and at least 44x44 CSS px interactive targets in `StoryBook/wwwroot/css/dinosaurs.css`

**Checkpoint**: User Story 2 can be validated without search, language switching, or modal behavior.

---

## Phase 5: User Story 3 - 回到首頁 (Priority: P1)

**Goal**: 使用者在任何恐龍介紹頁都能清楚看見並使用回首頁控制項。

**Independent Test**: 在 `/dinosaurs` 與任一 `/dinosaurs/{slug}` 頁面點擊「回首頁」後返回 `/`，且可再次進入恐龍介紹流程。

### Tests for User Story 3

- [ ] T036 [P] [US3] Write integration tests that list and detail pages expose a visible home link targeting `/` in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`

### Implementation for User Story 3

- [ ] T037 [US3] Add visible home anchor to the dinosaur list page in `StoryBook/Pages/Dinosaurs/Index.cshtml`
- [ ] T038 [US3] Add visible home anchor to the dinosaur details and not-found states in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T039 [US3] Style the home navigation control consistently with the dinosaur page actions in `StoryBook/wwwroot/css/dinosaurs.css`

**Checkpoint**: All P1 stories are complete and MVP can be validated end-to-end.

---

## Phase 6: User Story 4 - 點擊圖片查看大圖 (Priority: P2)

**Goal**: 使用者可用滑鼠或鍵盤開啟恐龍主圖的大圖檢視，並用關閉按鈕、背景或 Escape 關閉後回到原本焦點。

**Independent Test**: 在任一詳情頁以 Enter/Space 開啟主圖 modal，確認大圖與 alt/caption 顯示；按 Escape 或關閉按鈕後 modal 關閉且焦點回到觸發控制項。

### Tests for User Story 4

- [ ] T040 [P] [US4] Write integration tests for image button accessible name, Bootstrap modal markup, close control, and large-image alt text in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`

### Implementation for User Story 4

- [ ] T041 [US4] Wrap the main image in a keyboard-focusable button and add Bootstrap modal markup in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T042 [US4] Implement native JavaScript for modal trigger focus tracking, Escape/backdrop handling, and focus restoration in `StoryBook/wwwroot/js/dinosaurs.js`
- [ ] T043 [US4] Load `dinosaurs.js` only for dinosaur pages through the scripts section in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T044 [US4] Add modal, image button, and focus-visible styles in `StoryBook/wwwroot/css/dinosaurs.css`

**Checkpoint**: Image modal works independently of search and language switching.

---

## Phase 7: User Story 5 - 搜尋恐龍 (Priority: P2)

**Goal**: 使用者可在 `/dinosaurs` 即時搜尋名稱、生活時期、食性、發現地點、體型描述、簡介與關鍵字，無結果時可清除搜尋，搜尋結果使用一般連結進入詳情頁。

**Independent Test**: 在搜尋框輸入「暴龍」只顯示符合結果；輸入不存在關鍵字顯示友善提示與清除控制項；點擊結果後進入 `/dinosaurs/{slug}`。

### Tests for User Story 5

- [ ] T045 [P] [US5] Write unit tests for catalog search across localized names, period, diet, locations, size, summary, keywords, whitespace, punctuation, and no-result input in `StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs`
- [ ] T046 [P] [US5] Write integration tests for search input accessible name, result links, no-result message, clear control, and pteranodon category note on `/dinosaurs` in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`

### Implementation for User Story 5

- [ ] T047 [US5] Add search result projection model for list rendering and client filtering data in `StoryBook/Models/DinosaurSearchResult.cs`
- [ ] T048 [US5] Extend catalog search normalization and result mapping in `StoryBook/Services/DinosaurCatalogService.cs`
- [ ] T049 [US5] Expose searchable result data and friendly no-result text from the list PageModel in `StoryBook/Pages/Dinosaurs/Index.cshtml.cs`
- [ ] T050 [US5] Add accessible search input, clear button, result list links, no-result region, and pteranodon category note in `StoryBook/Pages/Dinosaurs/Index.cshtml`
- [ ] T051 [US5] Implement live filtering, clear search, special-character-safe matching, and result visibility updates in `StoryBook/wwwroot/js/dinosaurs.js`
- [ ] T052 [US5] Load `dinosaurs.js` for the dinosaur list page through the scripts section in `StoryBook/Pages/Dinosaurs/Index.cshtml`
- [ ] T053 [US5] Add search layout, result card, hidden/no-result, and clear-button focus styles in `StoryBook/wwwroot/css/dinosaurs.css`

**Checkpoint**: Search can be validated without story display or language persistence.

---

## Phase 8: User Story 6 - 閱讀恐龍小故事 (Priority: P2)

**Goal**: 每個恐龍詳情頁在介紹後顯示溫馨短故事與故事插圖，呈現兒童繪本感且具備雙語 alt text。

**Independent Test**: 任一詳情頁主要介紹下方出現故事標題、100-150 可閱讀單位故事文字與故事插圖；插圖 alt text 有意義且不使用檔名替代。

### Tests for User Story 6

- [ ] T054 [P] [US6] Write integration tests for story title, story body, story illustration, and meaningful alt text on detail pages in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`
- [ ] T055 [P] [US6] Extend content validation tests for story tone markers, story body length, and story illustration style tag in `StoryBook.Tests/Unit/DinosaurContentValidationTests.cs`

### Implementation for User Story 6

- [ ] T056 [US6] Render story title, body, illustration, caption, and accessible alt text in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T057 [US6] Add story section spacing, image sizing, and visual hierarchy styles in `StoryBook/wwwroot/css/dinosaurs.css`
- [ ] T058 [US6] Verify all story image paths in catalog data match files under `StoryBook/wwwroot/images/dinosaurs/` from `StoryBook/Data/dinosaurs.json`

**Checkpoint**: P2 content enhancement is independently visible on every detail route.

---

## Phase 9: User Story 7 - 切換語言 (Priority: P3)

**Goal**: 使用者可在中文與英文之間切換導覽文字、恐龍資料、搜尋結果、提示訊息、圖片 alt/caption 與小故事，偏好保存於 `localStorage` key `storybook.language`。

**Independent Test**: 在任一頁切換英文後，所有可見文字更新為英文；重新整理或開啟另一筆恐龍仍維持英文；切回中文後保存中文；無效 localStorage 值回退 `zh-TW` 且不顯示空白內容。

### Tests for User Story 7

- [ ] T059 [P] [US7] Write integration tests that list and detail pages emit bilingual text attributes, language switch controls, and `storybook.language` metadata in `StoryBook.Tests/Integration/DinosaurPagesTests.cs`
- [ ] T060 [P] [US7] Extend language preference unit tests for fallback display text and missing localized content behavior in `StoryBook.Tests/Unit/LanguagePreferenceServiceTests.cs`

### Implementation for User Story 7

- [ ] T061 [US7] Add language switch controls and localized navigation text containers to the shared layout in `StoryBook/Pages/Shared/_Layout.cshtml`
- [ ] T062 [US7] Add bilingual data attributes for list labels, search messages, result text, and category notes in `StoryBook/Pages/Dinosaurs/Index.cshtml`
- [ ] T063 [US7] Add bilingual data attributes for detail facts, navigation labels, modal labels, story text, image alt/caption, and not-found messages in `StoryBook/Pages/Dinosaurs/Details.cshtml`
- [ ] T064 [US7] Implement localStorage language initialization, invalid-value fallback, text swapping, alt/caption swapping, and cross-page persistence in `StoryBook/wwwroot/js/dinosaurs.js`
- [ ] T065 [US7] Add language switcher layout and visible focus states in `StoryBook/wwwroot/css/dinosaurs.css`

**Checkpoint**: P3 language switching is independently testable across list, detail, modal, and not-found states.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: 完成跨故事品質、驗收與交付檢查。

- [ ] T066 [P] Add integration tests for `/dinosaurs/not-a-real-slug` friendly not-found status, home link, and list link in `StoryBook.Tests/Integration/RoutingAndFallbackTests.cs`
- [ ] T067 [P] Add integration tests for canonical `/dinosaurs` and `/dinosaurs/{slug}` routes without JavaScript-only routing in `StoryBook.Tests/Integration/RoutingAndFallbackTests.cs`
- [ ] T068 Review public XML documentation for reusable models, services, options, and helpers in `StoryBook/Models/DinosaurProfile.cs` and `StoryBook/Services/DinosaurCatalogService.cs`
- [ ] T069 Run `dotnet restore StoryBook2.sln`, `dotnet build StoryBook2.sln`, and `dotnet test StoryBook2.sln` from the repository root described in `specs/001-dinosaur-intro-site/quickstart.md`
- [ ] T070 Execute the manual keyboard, modal, search, browser history, language persistence, responsive layout, and not-found checklist in `specs/001-dinosaur-intro-site/quickstart.md`
- [ ] T071 Check generated catalog and configuration for secrets, external data URLs, and unapproved dependencies in `StoryBook/Data/dinosaurs.json`
- [ ] T072 Confirm no feature JavaScript depends on jQuery and no data file was placed under public `wwwroot` data paths in `StoryBook/wwwroot/js/dinosaurs.js`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks every user story.
- **User Stories (Phase 3+)**: Depend on Foundational completion.
- **Polish (Final Phase)**: Depends on whichever stories are in scope for delivery; full release depends on all stories.

### User Story Dependencies

- **US1 瀏覽恐龍介紹 (P1)**: Starts after Foundation; MVP scope.
- **US2 換頁瀏覽多隻恐龍 (P1)**: Starts after Foundation and benefits from US1 detail page.
- **US3 回到首頁 (P1)**: Starts after US1 list/detail pages exist.
- **US4 點擊圖片查看大圖 (P2)**: Starts after US1 detail page and main image markup exist.
- **US5 搜尋恐龍 (P2)**: Starts after Foundation and US1 list page exists.
- **US6 閱讀恐龍小故事 (P2)**: Starts after Foundation and US1 detail page exists.
- **US7 切換語言 (P3)**: Starts after list/detail markup exists; can be layered after P1/P2 content is visible.

### Within Each User Story

- Tests must be written first and observed failing before implementation.
- Models and shared services before PageModels.
- PageModels before Razor page rendering.
- Core HTML contracts before JavaScript enhancement.
- Story checkpoint validation before moving to the next priority group.

---

## Parallel Opportunities

- **Foundation**: T004-T007 can be written in parallel; T008-T012 are sequential model dependencies; T013-T016 can proceed once model shapes are known; T019 and T020 can proceed independently from service code if asset filenames match `dinosaurs.json`.
- **US1**: T021 and T022 can be written in parallel; after tests, T023-T029 should be sequenced around list/detail page creation to avoid same-file conflicts.
- **US2**: T030 and T031 can be written in parallel; T032-T035 are sequential because service output drives PageModel and view markup.
- **US3**: T037 and T038 can be done in parallel after T036 because they touch different Razor pages; T039 follows styling review.
- **US4**: T040 can be written while T041 markup is planned; T042 and T044 touch separate files and can run in parallel after modal IDs/classes are agreed.
- **US5**: T045 and T046 can be written in parallel; T047 and T048 can proceed before T049-T053 integrate PageModel, Razor, JavaScript, and CSS.
- **US6**: T054 and T055 can be written in parallel; T056 and T057 touch separate view/style files and can proceed together after test expectations are clear.
- **US7**: T059 and T060 can be written in parallel; T062 and T063 can be done in parallel after T061 defines the shared language controls.

---

## Parallel Example: User Story 1

```bash
Task: "T021 [US1] Write integration tests for home entry, /dinosaurs first-profile content, and /dinosaurs/tyrannosaurus-rex required HTML fields in StoryBook.Tests/Integration/DinosaurPagesTests.cs"
Task: "T022 [US1] Write unit tests for first-profile selection and summary readable-unit limits in StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs"
```

## Parallel Example: User Story 5

```bash
Task: "T045 [US5] Write unit tests for catalog search across localized fields in StoryBook.Tests/Unit/DinosaurCatalogServiceTests.cs"
Task: "T046 [US5] Write integration tests for search UI contract in StoryBook.Tests/Integration/DinosaurPagesTests.cs"
```

## Parallel Example: User Story 7

```bash
Task: "T059 [US7] Write integration tests for bilingual text attributes and language switch controls in StoryBook.Tests/Integration/DinosaurPagesTests.cs"
Task: "T060 [US7] Extend language preference unit tests for fallback behavior in StoryBook.Tests/Unit/LanguagePreferenceServiceTests.cs"
```

---

## Implementation Strategy

### MVP First (P1 Core)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundation.
3. Complete US1, then validate home to first dinosaur and direct detail route.
4. Complete US2, then validate previous/next history-preserving anchor navigation.
5. Complete US3, then validate return-home flow.
6. Stop and demo the P1 MVP before adding P2/P3 enhancements.

### Incremental Delivery

1. Foundation creates a stable content and service contract.
2. P1 stories deliver the usable dinosaur browsing loop.
3. P2 stories add modal inspection, search, and story enrichment without changing canonical routes.
4. P3 adds bilingual switching and persistence.
5. Final polish runs build/test plus quickstart manual acceptance.

### Validation Gates

1. Every test task is expected to fail before its matching implementation task.
2. Each story checkpoint requires `dotnet test StoryBook2.sln` or the relevant subset before continuing.
3. Final delivery requires build success, all tests passing, quickstart manual checklist executed, no secrets, no external content dependencies, and no JavaScript-only router.
