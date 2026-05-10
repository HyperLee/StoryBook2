# Tasks: 水族館動物介紹故事書

**Input**: Design documents from `/specs/002-aquarium-storybook/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: 本功能規格與憲章明確要求測試優先。每個含測試的階段必須先新增或更新失敗測試，再實作並讓測試通過。

**Organization**: 任務依使用者故事分組，確保每個故事可獨立實作、獨立驗證，並維持單一 ASP.NET Core Razor Pages 應用架構。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行，因為檔案不同且不依賴尚未完成的任務
- **[Story]**: 使用者故事標籤，僅出現在使用者故事階段
- 所有任務描述都包含明確檔案路徑

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立水族館功能的檔案位置、靜態資產入口與設定骨架。

- [X] T001 Create Aquarium Razor Pages placeholders in `StoryBook/Pages/Aquarium/Index.cshtml`, `StoryBook/Pages/Aquarium/Index.cshtml.cs`, `StoryBook/Pages/Aquarium/Details.cshtml`, and `StoryBook/Pages/Aquarium/Details.cshtml.cs`
- [X] T002 [P] Create feature asset files `StoryBook/wwwroot/css/aquarium.css` and `StoryBook/wwwroot/js/aquarium.js`
- [X] T003 [P] Add `AquariumCatalog` content-path configuration defaults to `StoryBook/appsettings.json` and `StoryBook/appsettings.Development.json`
- [X] T004 [P] Add conditional `UseAquariumAssets` stylesheet loading to `StoryBook/Pages/Shared/_Layout.cshtml`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有故事共用的資料模型、內容驗證、catalog service、DI 與完整內容資料。

**Critical**: 此階段完成前，不應開始任何使用者故事實作。

- [X] T005 [P] Add aquarium integration test fixture in `StoryBook.Tests/Integration/AquariumPageTestFixture.cs`
- [X] T006 [P] Add failing catalog validation tests for 15 fixed animals, bilingual fields, at least 5 habitat categories, readable-unit limits, main/story image alt text, and schema-aligned paths in `StoryBook.Tests/Unit/AquariumContentValidationTests.cs`
- [X] T007 [P] Add failing catalog service tests for JSON loading, load-failure result/logging, caching, slug lookup, logging unknown slugs, normalized search, short-query handling, previous/next navigation, and p95 lookup/search timing in `StoryBook.Tests/Unit/AquariumCatalogServiceTests.cs`
- [X] T008 [P] Create bilingual text and catalog root models in `StoryBook/Models/AquariumText.cs` and `StoryBook/Models/AquariumCatalog.cs`
- [X] T009 [P] Create habitat category and animal profile models in `StoryBook/Models/AquariumHabitatCategory.cs` and `StoryBook/Models/AquariumAnimalProfile.cs`
- [X] T010 [P] Create image and story models in `StoryBook/Models/AquariumImage.cs` and `StoryBook/Models/AquariumStory.cs`
- [X] T011 [P] Create list-page search projection model in `StoryBook/Models/AquariumSearchResult.cs`
- [X] T012 [P] Create validation result and options classes in `StoryBook/Services/AquariumContentValidationResult.cs` and `StoryBook/Services/AquariumCatalogOptions.cs`
- [X] T013 Implement aquarium content validation rules for fixed animals, at least 5 habitat categories, bilingual fields, readable-unit limits, main/story images, alt text, and schema paths in `StoryBook/Services/AquariumContentValidator.cs`
- [X] T014 Implement initial aquarium catalog loading, load-failure handling, caching, slug lookup, validation failure logging, unknown slug logging, and short-query classification in `StoryBook/Services/AquariumCatalogService.cs`
- [X] T015 Register aquarium options, validator, and catalog service in `StoryBook/Program.cs`
- [X] T016 Add complete 15-animal bilingual content catalog for `clownfish`, `seahorse`, `sea-turtle`, `jellyfish`, `octopus`, `shark`, `stingray`, `penguin`, `seal`, `dolphin`, `starfish`, `crab`, `coral`, `goldfish`, and `axolotl` in `StoryBook/Data/aquarium.json`
- [X] T017 Update language XML documentation from dinosaur-specific wording to shared storybook wording in `StoryBook/Models/LanguageCode.cs` and `StoryBook/Services/LanguagePreferenceService.cs`
- [X] T018 Run foundational tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumContentValidationTests` and `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumCatalogServiceTests` against `StoryBook.Tests/StoryBook.Tests.csproj`

**Checkpoint**: Foundation ready. Aquarium content can be loaded, validated, searched, and resolved by slug through DI-backed services.

---

## Phase 3: User Story 1 - 進入水族館故事書 (Priority: P1)

**Goal**: 使用者能從首頁進入 `/aquarium`，看到封面、歡迎文字、搜尋入口、語言切換與開始閱讀動作，並進入第一隻動物。

**Independent Test**: 從首頁點擊水族館入口後，確認 `/aquarium` 顯示主頁內容；點擊開始閱讀後進入 sortOrder = 1 的 `/aquarium/clownfish`。

### Tests for User Story 1

- [ ] T019 [P] [US1] Add failing integration tests for homepage entry, `/aquarium` cover, 10-40 字歡迎文字, search entry, language switcher, start-reading link, and Aquarium home data-load failure state with retry/home actions in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 1

- [ ] T020 [US1] Add visible Aquarium entry link to `StoryBook/Pages/Index.cshtml`
- [ ] T021 [US1] Implement Aquarium list PageModel loading profiles, search projections, first profile, and catalog load-failure state in `StoryBook/Pages/Aquarium/Index.cshtml.cs`
- [ ] T022 [US1] Render Aquarium home cover, welcome text, start-reading anchor, language-aware labels, initial 15-item directory shell, and friendly data-load failure state with retry/home actions in `StoryBook/Pages/Aquarium/Index.cshtml`
- [ ] T023 [US1] Add child-friendly Aquarium cover, start action, and directory layout styles in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T024 [US1] Run US1 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests` and verify `/`, `/aquarium`, `/aquarium/clownfish`, and data-load failure expectations in `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: User Story 1 is independently demoable from the homepage through first animal navigation.

---

## Phase 4: User Story 2 - 閱讀單一動物介紹 (Priority: P1)

**Goal**: 直接開啟任一 `/aquarium/{slug}` 時，只顯示該動物的名稱、分類、生活環境、食性、發現地點、簡介、小故事與圖片。

**Independent Test**: 直接開啟 `/aquarium/clownfish`、`/aquarium/axolotl` 與任一中間項目，確認完整內容、雙語 alt text 與兒童閱讀篇幅符合規格。

### Tests for User Story 2

- [ ] T025 [P] [US2] Add failing integration tests for direct slug details, required fields, story section, main image, story illustration, bilingual alt text, detail data-load failure state, and retry/home actions in `StoryBook.Tests/Integration/AquariumPagesTests.cs`
- [ ] T026 [P] [US2] Add failing content validation tests for summary length, story length, required story illustration, and non-filename alt text in `StoryBook.Tests/Unit/AquariumContentValidationTests.cs`

### Implementation for User Story 2

- [ ] T027 [US2] Implement Aquarium details PageModel slug lookup, profile property, catalog load-failure state, not-found flag, and 404 status code in `StoryBook/Pages/Aquarium/Details.cshtml.cs`
- [ ] T028 [US2] Render one-animal detail layout with facts, summary, story, main image, story illustration, friendly missing-image text, and friendly data-load failure state with retry/home actions in `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T029 [P] [US2] Add coral-reef and freshwater main/story image assets at `StoryBook/wwwroot/images/aquarium/clownfish-main.png`, `StoryBook/wwwroot/images/aquarium/clownfish-story.png`, `StoryBook/wwwroot/images/aquarium/coral-main.png`, `StoryBook/wwwroot/images/aquarium/coral-story.png`, `StoryBook/wwwroot/images/aquarium/goldfish-main.png`, `StoryBook/wwwroot/images/aquarium/goldfish-story.png`, `StoryBook/wwwroot/images/aquarium/axolotl-main.png`, and `StoryBook/wwwroot/images/aquarium/axolotl-story.png`
- [ ] T030 [P] [US2] Add saltwater main/story image assets at `StoryBook/wwwroot/images/aquarium/seahorse-main.png`, `StoryBook/wwwroot/images/aquarium/seahorse-story.png`, `StoryBook/wwwroot/images/aquarium/sea-turtle-main.png`, `StoryBook/wwwroot/images/aquarium/sea-turtle-story.png`, `StoryBook/wwwroot/images/aquarium/octopus-main.png`, `StoryBook/wwwroot/images/aquarium/octopus-story.png`, `StoryBook/wwwroot/images/aquarium/shark-main.png`, `StoryBook/wwwroot/images/aquarium/shark-story.png`, `StoryBook/wwwroot/images/aquarium/stingray-main.png`, `StoryBook/wwwroot/images/aquarium/stingray-story.png`, `StoryBook/wwwroot/images/aquarium/dolphin-main.png`, and `StoryBook/wwwroot/images/aquarium/dolphin-story.png`
- [ ] T031 [P] [US2] Add deep-sea, polar, and tide-pool main/story image assets at `StoryBook/wwwroot/images/aquarium/jellyfish-main.png`, `StoryBook/wwwroot/images/aquarium/jellyfish-story.png`, `StoryBook/wwwroot/images/aquarium/penguin-main.png`, `StoryBook/wwwroot/images/aquarium/penguin-story.png`, `StoryBook/wwwroot/images/aquarium/seal-main.png`, `StoryBook/wwwroot/images/aquarium/seal-story.png`, `StoryBook/wwwroot/images/aquarium/starfish-main.png`, `StoryBook/wwwroot/images/aquarium/starfish-story.png`, `StoryBook/wwwroot/images/aquarium/crab-main.png`, and `StoryBook/wwwroot/images/aquarium/crab-story.png`
- [ ] T032 [US2] Add detail-page, story-section, image, and missing-image responsive styles in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T033 [US2] Run US2 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests` and verify `/aquarium/clownfish`, `/aquarium/axolotl`, `/aquarium/not-a-real-slug`, and data-load failure state against `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: User Story 2 is independently demoable by direct animal URL and friendly 404 URL.

---

## Phase 5: User Story 3 - 逐頁瀏覽不同動物 (Priority: P1)

**Goal**: 使用者能用一般 anchor link 在閱讀順序中上一頁與下一頁瀏覽，第一頁與最後一頁不提供誤導性的有效連結。

**Independent Test**: 在第一筆、中間筆與最後一筆動物頁操作上一頁與下一頁，確認目標 URL、disabled 狀態與鍵盤焦點樣式正確。

### Tests for User Story 3

- [ ] T034 [P] [US3] Add failing unit tests for first, middle, last, unknown slug, and rapid repeated previous/next navigation final-state expectations in `StoryBook.Tests/Unit/AquariumCatalogServiceTests.cs`
- [ ] T035 [P] [US3] Add failing integration tests for pager anchor links, disabled first/last boundaries, and rapid navigation acceptance markers in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 3

- [ ] T036 [US3] Implement `GetFirstProfile`, `GetPreviousProfile`, and `GetNextProfile` in `StoryBook/Services/AquariumCatalogService.cs`
- [ ] T037 [US3] Add previous and next profile properties to `StoryBook/Pages/Aquarium/Details.cshtml.cs`
- [ ] T038 [US3] Render previous/next anchor navigation and disabled boundary spans in `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T039 [US3] Add visible focus, disabled pager, and responsive pager styles in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T040 [US3] Run US3 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumCatalogServiceTests` and `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests`, then verify rapid previous/next steps in `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: All P1 stories are independently functional and cover homepage entry, detail reading, and page navigation.

---

## Phase 6: User Story 4 - 搜尋水族館動物 (Priority: P2)

**Goal**: 使用者可在 `/aquarium` 依名稱、生活環境或食性搜尋，結果可直接連到對應動物頁，無結果時顯示友善提示並可清除。

**Independent Test**: 在 `/aquarium` 搜尋「海馬」、`reef`、英文大小寫混合關鍵字與空白輸入，確認結果、無結果與清除行為正確。

### Tests for User Story 4

- [ ] T041 [P] [US4] Add failing unit tests for normalized Traditional Chinese and English search, whitespace trimming, punctuation removal, case-insensitive English matching, empty-query behavior, short-query handling below 2 effective characters, and rapid-query final-state behavior in `StoryBook.Tests/Unit/AquariumCatalogServiceTests.cs`
- [ ] T042 [P] [US4] Add failing integration tests for search input accessible name, clear button, too-short query text, no-result text, result card fields, result anchor links, and markup needed to verify <=1s client-visible updates in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 4

- [ ] T043 [US4] Implement normalized `Search`, short-query classification, `GetSearchResults`, and client search text projection in `StoryBook/Services/AquariumCatalogService.cs` and `StoryBook/Models/AquariumSearchResult.cs`
- [ ] T044 [US4] Populate search result projections on the Aquarium home PageModel in `StoryBook/Pages/Aquarium/Index.cshtml.cs`
- [ ] T045 [US4] Render search input, clear button, too-short query status, no-result status, and linked result cards with `data-aquarium-search-*` attributes in `StoryBook/Pages/Aquarium/Index.cshtml`
- [ ] T046 [US4] Implement native JavaScript search filtering, query normalization, too-short query handling, no-result toggling, <=1s visible update timing, rapid-query final-state behavior, and clear-search focus behavior in `StoryBook/wwwroot/js/aquarium.js`
- [ ] T047 [US4] Add search controls, result grid, no-result status, and keyboard focus styles in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T048 [US4] Run US4 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumCatalogServiceTests` and verify quickstart search steps, short-query behavior, rapid-query final state, and <=1s visible update timing in `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: User Story 4 can be demonstrated from `/aquarium` without changing route behavior or browser history.

---

## Phase 7: User Story 5 - 切換中文與英文 (Priority: P2)

**Goal**: 使用者可在水族館主頁、搜尋結果與動物頁切換 `zh-TW` 與 `en`，偏好儲存在 `localStorage` key `storybook.language`，缺漏內容回退繁體中文。

**Independent Test**: 在 `/aquarium`、搜尋結果與任一 `/aquarium/{slug}` 切換語言並重新整理，確認主要文字、alt/caption、提示訊息與控制項維持所選語言或繁體中文 fallback。

### Tests for User Story 5

- [ ] T049 [P] [US5] Add or extend language fallback and `storybook.language` storage key tests in `StoryBook.Tests/Unit/LanguagePreferenceServiceTests.cs`
- [ ] T050 [P] [US5] Add failing integration tests for bilingual `data-i18n-*`, `data-alt-*`, `data-aria-label-*`, `data-placeholder-*`, and shared language switch controls in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 5

- [ ] T051 [US5] Add bilingual text, alt, aria-label, and placeholder data attributes across `StoryBook/Pages/Aquarium/Index.cshtml` and `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T052 [US5] Implement Aquarium language application, storage fallback, attribute updates, and `document.documentElement.lang` updates in `StoryBook/wwwroot/js/aquarium.js`
- [ ] T053 [US5] Adjust language-sensitive spacing and responsive text wrapping in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T054 [US5] Run US5 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter LanguagePreferenceServiceTests` and `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests` against `StoryBook.Tests/StoryBook.Tests.csproj`

**Checkpoint**: User Story 5 preserves shared storybook language preference without blank user-visible content.

---

## Phase 8: User Story 6 - 放大查看動物圖片 (Priority: P2)

**Goal**: 使用者可用滑鼠或鍵盤開啟主要圖片大圖，透過明確關閉控制、背景或 Escape 關閉，並回到原觸發焦點。

**Independent Test**: 在任一動物頁用鍵盤開啟大圖、關閉大圖並確認焦點回到圖片控制；圖片失敗時顯示友善替代訊息。

### Tests for User Story 6

- [ ] T055 [P] [US6] Add failing integration tests for modal open button accessible name, modal markup, close control, bilingual modal labels, and image fallback contract in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 6

- [ ] T056 [US6] Add main-image button, modal dialog markup, close control, caption, and friendly image fallback text to `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T057 [US6] Implement Aquarium modal open, close, Escape handling, backdrop close, and focus return logic in `StoryBook/wwwroot/js/aquarium.js`
- [ ] T058 [US6] Add modal, image button, close control, and missing-image styles in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T059 [US6] Run US6 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests` and verify quickstart modal steps in `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: User Story 6 image inspection works without trapping keyboard users.

---

## Phase 9: User Story 7 - 離開水族館故事書並返回首頁 (Priority: P3)

**Goal**: 使用者可從水族館主頁、搜尋狀態、任一動物頁與找不到內容狀態回到網站首頁。

**Independent Test**: 從 `/aquarium`、搜尋結果狀態、`/aquarium/clownfish` 與 `/aquarium/not-a-real-slug` 點擊回首頁，確認都返回 `/`。

### Tests for User Story 7

- [ ] T060 [P] [US7] Add failing integration tests for home links on Aquarium home, details, search-state markup, and not-found state in `StoryBook.Tests/Integration/AquariumPagesTests.cs`

### Implementation for User Story 7

- [ ] T061 [US7] Add clear return-home anchors to Aquarium home and detail pages in `StoryBook/Pages/Aquarium/Index.cshtml` and `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T062 [US7] Ensure Aquarium not-found state returns HTTP 404 and includes links to `/` and `/aquarium` in `StoryBook/Pages/Aquarium/Details.cshtml.cs` and `StoryBook/Pages/Aquarium/Details.cshtml`
- [ ] T063 [US7] Style return-home and Aquarium-home navigation actions in `StoryBook/wwwroot/css/aquarium.css`
- [ ] T064 [US7] Run US7 tests with `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter AquariumPagesTests` and verify return-home quickstart steps in `specs/002-aquarium-storybook/quickstart.md`

**Checkpoint**: User Story 7 provides clear escape routes from every Aquarium state.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: 完成交付品質、手動驗收、文件同步與全 solution 驗證。

- [ ] T065 [P] Audit public XML documentation on aquarium models, options, validators, services, and shared language types in `StoryBook/Models/AquariumText.cs`, `StoryBook/Models/AquariumCatalog.cs`, `StoryBook/Models/AquariumAnimalProfile.cs`, `StoryBook/Models/AquariumHabitatCategory.cs`, `StoryBook/Models/AquariumImage.cs`, `StoryBook/Models/AquariumStory.cs`, `StoryBook/Models/AquariumSearchResult.cs`, `StoryBook/Services/AquariumCatalogOptions.cs`, `StoryBook/Services/AquariumContentValidator.cs`, and `StoryBook/Services/AquariumCatalogService.cs`
- [ ] T066 [P] Review child-friendly Traditional Chinese and English user-visible copy in `StoryBook/Pages/Aquarium/Index.cshtml`, `StoryBook/Pages/Aquarium/Details.cshtml`, and `StoryBook/Data/aquarium.json`
- [ ] T067 [P] Review Aquarium content, at least 5 habitat categories, and asset references against `specs/002-aquarium-storybook/contracts/content-catalog.schema.json` and `StoryBook/Data/aquarium.json`
- [ ] T068 [P] Inspect Aquarium files for placeholders or secrets in `StoryBook/Data/aquarium.json`, `StoryBook/wwwroot/js/aquarium.js`, `StoryBook/wwwroot/css/aquarium.css`, and `StoryBook/wwwroot/images/aquarium/`
- [ ] T069 Run restore with `dotnet restore StoryBook2.sln` for `StoryBook2.sln`
- [ ] T070 Run build with `dotnet build StoryBook2.sln` for `StoryBook2.sln`
- [ ] T071 Run full tests with `dotnet test StoryBook2.sln` for `StoryBook2.sln`
- [ ] T072 Complete the manual acceptance checklist, including data-load failure, short-query, rapid-operation, and <=1s search-update checks, and record any blocker or deviation in `specs/002-aquarium-storybook/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies.
- **Phase 2 Foundational**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 US1**: Depends on Phase 2.
- **Phase 4 US2**: Depends on Phase 2; can proceed in parallel with US1 after foundation, but the P1 release should validate US1 and US2 together.
- **Phase 5 US3**: Depends on Phase 2 and touches the same detail/service files as US2, so coordinate file ownership if parallelized.
- **Phase 6 US4**: Depends on Phase 2 and can proceed after the Aquarium home shell exists.
- **Phase 7 US5**: Depends on Aquarium markup from US1/US2/US4.
- **Phase 8 US6**: Depends on details markup from US2.
- **Phase 9 US7**: Depends on Aquarium home and details pages from US1/US2.
- **Phase 10 Polish**: Depends on all selected user stories.

### User Story Dependencies

- **US1 (P1)**: Independent after Foundation. Provides entry and home page.
- **US2 (P1)**: Independent after Foundation. Provides direct animal reading and 404 behavior.
- **US3 (P1)**: Depends on detail route and service data from Foundation; practically integrates with US2 page files.
- **US4 (P2)**: Depends on Aquarium home markup and catalog search projections.
- **US5 (P2)**: Depends on rendered Aquarium markup from US1, US2, and US4.
- **US6 (P2)**: Depends on main image markup from US2.
- **US7 (P3)**: Depends on Aquarium home/details/not-found states.

### Within Each User Story

- Write or update tests first and confirm they fail for the missing behavior.
- Implement models or projections before services.
- Implement services before PageModels.
- Implement PageModels before Razor markup.
- Implement markup before JavaScript and CSS polish.
- Run story-specific tests before moving to the next story checkpoint.

---

## Parallel Opportunities

- T002, T003, and T004 can run in parallel after T001.
- T005, T006, and T007 can run in parallel because they create separate test files.
- T008 through T012 can run in parallel because they create separate model/service support files.
- T019, T025, T034, T041, T049, T055, and T060 can be authored independently as failing tests once Foundation exists.
- T029, T030, and T031 can run in parallel because each owns disjoint Aquarium image asset files.
- US4 service/search work and US6 modal work can proceed in parallel after US2 if CSS and JS edit ownership is coordinated.

---

## Parallel Examples

### User Story 1

```text
Task: T019 Add Aquarium entry and home-page integration tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
Task: T023 Add cover and directory styling in StoryBook/wwwroot/css/aquarium.css
```

### User Story 2

```text
Task: T025 Add detail-page integration tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
Task: T026 Add detail content validation tests in StoryBook.Tests/Unit/AquariumContentValidationTests.cs
Task: T029 Add coral-reef and freshwater image assets in StoryBook/wwwroot/images/aquarium/
Task: T030 Add saltwater image assets in StoryBook/wwwroot/images/aquarium/
Task: T031 Add deep-sea, polar, and tide-pool image assets in StoryBook/wwwroot/images/aquarium/
```

### User Story 3

```text
Task: T034 Add navigation unit tests in StoryBook.Tests/Unit/AquariumCatalogServiceTests.cs
Task: T035 Add pager integration tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
```

### User Story 4

```text
Task: T041 Add search unit tests in StoryBook.Tests/Unit/AquariumCatalogServiceTests.cs
Task: T042 Add search HTML contract tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
```

### User Story 5

```text
Task: T049 Extend language preference tests in StoryBook.Tests/Unit/LanguagePreferenceServiceTests.cs
Task: T050 Add bilingual attribute contract tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
```

### User Story 6

```text
Task: T055 Add modal contract tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
Task: T058 Add modal styles in StoryBook/wwwroot/css/aquarium.css
```

### User Story 7

```text
Task: T060 Add return-home integration tests in StoryBook.Tests/Integration/AquariumPagesTests.cs
Task: T063 Add return navigation styles in StoryBook/wwwroot/css/aquarium.css
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 User Story 1.
4. Stop and validate homepage to `/aquarium` to first-animal flow independently.

### P1 Core Release

1. Complete MVP First.
2. Complete Phase 4 User Story 2.
3. Complete Phase 5 User Story 3.
4. Validate direct animal reading, friendly 404, and previous/next reading flow.

### Incremental Delivery

1. Add US4 Search and validate `/aquarium` search independently.
2. Add US5 Language switching and validate shared `storybook.language` behavior.
3. Add US6 Image modal and validate keyboard and focus behavior.
4. Add US7 Return-home navigation.
5. Complete Phase 10 Polish and run full build/test/manual acceptance.

### Parallel Team Strategy

1. One developer owns shared catalog models/services/tests in Phase 2.
2. One developer owns Aquarium Razor Pages markup in `StoryBook/Pages/Aquarium/`.
3. One developer owns assets and styles in `StoryBook/wwwroot/images/aquarium/` and `StoryBook/wwwroot/css/aquarium.css`.
4. One developer owns browser behavior in `StoryBook/wwwroot/js/aquarium.js`.
5. Coordinate changes to `StoryBook.Tests/Integration/AquariumPagesTests.cs`, because many stories append assertions to that shared file.
