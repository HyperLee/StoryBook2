# Tasks: 全站探索與分類搜尋

**Input**: Design documents from `/specs/004-sitewide-explore-search/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/ui-routes.md`, `quickstart.md`
**Tests**: Included because the specification marks user scenarios/tests as mandatory and the implementation plan/constitution require test-first coverage.
**Feature**: `004-sitewide-explore-search`

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel only when tasks touch different files and do not depend on incomplete test contracts.
- **[Story]**: User story label (`US1`, `US2`, `US3`, `US4`) for story-phase tasks only.
- Every task includes exact repository paths to modify or validate.
- Test and acceptance tasks are listed before production implementation tasks to satisfy the constitution test-first gate.

## Phase 1: Test & Acceptance Baseline

**Purpose**: Define failing automated tests and manual acceptance evidence before production code or markup changes.

**CRITICAL**: Complete T001-T010 first and confirm the new tests fail for the expected missing behavior before starting Phase 2 implementation.

- [X] T001 Add Explore integration test fixture with configurable dinosaur and aquarium catalog paths in `StoryBook.Tests/Integration/ExplorePageTestFixture.cs`
- [X] T002 Add failing projection, source-status, partial/all failure, and `ILogger<ExplorationCatalogService>` no-sensitive-content unit tests in `StoryBook.Tests/Unit/ExplorationCatalogServiceTests.cs`
- [X] T003 [US1] Add failing integration tests for homepage `/explore` link, `/explore` 200 OK, default `zh-TW` content, both source labels, and detail anchors in `StoryBook.Tests/Integration/ExplorePagesTests.cs`
- [X] T004 [US1] Add failing integration tests for partial-source and all-source failure friendly states in `StoryBook.Tests/Integration/ExplorePagesTests.cs`
- [X] T005 [US2] Add failing normalization, bilingual matching, too-short, punctuation-only, no-results, and stable-order unit tests in `StoryBook.Tests/Unit/ExplorationSearchServiceTests.cs`
- [X] T006 [US2] Add failing rendered search contract integration tests for search input, clear button, result status, too-short state, no-results state, bilingual search text, and no `pushState` or `replaceState` usage in `StoryBook.Tests/Integration/ExplorePagesTests.cs`
- [X] T007 [US3] Extend failing unit tests for single-selection facet state, AND filter matching, search/filter intersection, clear-filter behavior, and stable ordering in `StoryBook.Tests/Unit/ExplorationSearchServiceTests.cs`
- [X] T008 [US3] Add failing rendered filter contract integration tests for `data-explore-filter-group`, `data-explore-filter-value`, source/diet/living-area/period/discovery facets, and `data-explore-facets` result metadata in `StoryBook.Tests/Integration/ExplorePagesTests.cs`
- [X] T009 [US4] Add failing integration tests for bilingual `data-i18n`, `data-aria-label`, `data-placeholder`, effective theme layout attributes, absence of `[data-theme-selector]`, and theme-change preservation of search/filter state on `/explore` in `StoryBook.Tests/Integration/ExplorePagesTests.cs`
- [X] T010 [US4] Add failing unit tests that every `ExplorationItem`, `ExplorationFacetGroup`, `ExplorationFacetValue`, and `ExplorationSourceStatus` has nonblank `zh-TW` fallback labels, summaries, source labels, and alt text in `StoryBook.Tests/Unit/ExplorationCatalogServiceTests.cs`

**Checkpoint**: Automated test contracts exist, fail for missing behavior, and cover the user stories before production changes.

---

## Phase 2: Setup & Foundational Implementation

**Purpose**: Add the production surface, shared projection contracts, source status handling, and service registration after the test baseline exists.

- [X] T011 Create the Explore Razor Page shell in `StoryBook/Pages/Explore/Index.cshtml` and `StoryBook/Pages/Explore/Index.cshtml.cs`
- [X] T012 [P] Create feature asset files in `StoryBook/wwwroot/css/explore.css` and `StoryBook/wwwroot/js/explore.js`
- [X] T013 Add conditional Explore stylesheet loading for `ViewData["UseExploreAssets"]` in `StoryBook/Pages/Shared/_Layout.cshtml`
- [X] T014 [P] Add `ExplorationSourceType` with source order and route prefix metadata in `StoryBook/Models/ExplorationSourceType.cs`
- [X] T015 [P] Add `ExplorationFacetValue` model with bilingual labels and sort metadata in `StoryBook/Models/ExplorationFacetValue.cs`
- [X] T016 [P] Add `ExplorationFacetGroup` model with single-selection metadata in `StoryBook/Models/ExplorationFacetGroup.cs`
- [X] T017 [P] Add `ExplorationItem` projection model with stable id, detail href, bilingual text, search text, and facets in `StoryBook/Models/ExplorationItem.cs`
- [X] T018 [P] Add `ExplorationSearchState` model for raw query, normalized query, selected facets, result mode, language, and visible count in `StoryBook/Models/ExplorationSearchState.cs`
- [X] T019 [P] Add `ExplorationSourceStatus` model for available, partial-failure, and all-failed source reporting in `StoryBook/Models/ExplorationSourceStatus.cs`
- [X] T020 Implement `ExplorationCatalogService` to compose dinosaur and aquarium projections, stable ordering, bilingual search text, source/failure statuses, and structured failure logging without recording user search strings or secrets in `StoryBook/Services/ExplorationCatalogService.cs`
- [X] T021 Register `ExplorationCatalogService` as a singleton in `StoryBook/Program.cs`

**Checkpoint**: Projection and source-status foundation is ready; story implementation can begin.

---

## Phase 3: User Story 1 - 從首頁進入全站探索頁 (Priority: P1) MVP

**Goal**: Users can enter `/explore` from the homepage, see dinosaur and aquarium content, and open existing detail pages through normal links.

**Independent Test**: T003-T004 cover the homepage entry, `/explore` rendered content, detail links, and source failure states.

### Implementation for User Story 1

- [X] T022 [US1] Implement `IndexModel` loading of exploration projection, source statuses, and `ViewData["UseExploreAssets"]` in `StoryBook/Pages/Explore/Index.cshtml.cs`
- [X] T023 [US1] Render `/explore` full-collection sections, result cards, source labels, images/alt text, detail anchors, home link, partial failure state, and all-failed state in `StoryBook/Pages/Explore/Index.cshtml`
- [X] T024 [US1] Add a bilingual "探索全部故事" / "Explore all stories" normal anchor to `/explore` in the homepage action area in `StoryBook/Pages/Index.cshtml`
- [X] T025 [US1] Add initial Explore page, source section, result card, image fallback, and friendly failure-state styles in `StoryBook/wwwroot/css/explore.css`

**Checkpoint**: User Story 1 is independently functional and testable as the MVP.

---

## Phase 4: User Story 2 - 跨故事書搜尋內容 (Priority: P1)

**Goal**: Users can search across both storybooks with Chinese or English terms, receive child-friendly status messages, and clear the query back to the full collection.

**Independent Test**: T005-T006 cover search normalization, bilingual matching, invalid/too-short searches, no-results state, stable order, clear behavior, and no URL/history writes.

### Implementation for User Story 2

- [ ] T026 [US2] Implement `ExplorationSearchService` query normalization, bilingual matching, result mode selection, no-query-logging behavior, and stable source/order preservation in `StoryBook/Services/ExplorationSearchService.cs`
- [ ] T027 [US2] Register `ExplorationSearchService` as a singleton in `StoryBook/Program.cs`
- [ ] T028 [US2] Add search input, clear search control, result status live region, too-short message, no-results message, and `data-explore-search-text` attributes in `StoryBook/Pages/Explore/Index.cshtml`
- [ ] T029 [US2] Implement client-side search, clear search, too-short handling, no-results handling, final-input-wins updates, and no URL/history/storage persistence in `StoryBook/wwwroot/js/explore.js`
- [ ] T030 [US2] Add accessible search control, clear button, live status, too-short, and no-results styles in `StoryBook/wwwroot/css/explore.css`

**Checkpoint**: User Story 2 works independently with `/explore` and does not persist search state outside the current page lifecycle.

---

## Phase 5: User Story 3 - 用分類快速篩選故事內容 (Priority: P2)

**Goal**: Users can filter by source and available content attributes, with one selected value per group and AND behavior across groups.

**Independent Test**: T007-T008 cover source, diet, living-area, period, discovery-location filters, same-group replacement, cross-group AND behavior, and clearing filters.

### Implementation for User Story 3

- [ ] T031 [US3] Extend `ExplorationCatalogService` to build source, diet, living-area, period, and discovery-location facet groups and result facet values in `StoryBook/Services/ExplorationCatalogService.cs`
- [ ] T032 [US3] Render grouped filter controls, active-state attributes, clear filters control, intersection status text, and `data-explore-facets` on result cards in `StoryBook/Pages/Explore/Index.cshtml`
- [ ] T033 [US3] Implement single-select facet replacement, group-to-group AND filtering, search/filter intersection, clear filters, and visible count updates in `StoryBook/wwwroot/js/explore.js`
- [ ] T034 [US3] Add filter group, filter button, active value, clear filters, and compact responsive filter layout styles in `StoryBook/wwwroot/css/explore.css`

**Checkpoint**: User Story 3 works independently with search and filtering composed correctly.

---

## Phase 6: User Story 4 - 保持雙語、主題與可及性一致 (Priority: P2)

**Goal**: `/explore` follows existing language and theme rules, avoids a new theme selector, and remains keyboard-accessible and responsive.

**Independent Test**: T009-T010 cover bilingual metadata, invalid-language fallback, theme contract, absence of a theme selector, nonblank fallback content, and theme-change preservation of Explore interaction state.

### Implementation for User Story 4

- [ ] T035 [US4] Add complete bilingual visible text, placeholders, accessible names, child-friendly status messages, and `aria-live="polite"` result status templates in `StoryBook/Pages/Explore/Index.cshtml`
- [ ] T036 [US4] Extend Explore JavaScript to read `storybook.language`, apply bilingual labels/placeholders/aria labels/status text, fallback invalid or missing language to `zh-TW`, and preserve current search/filter state during language and theme-related DOM updates in `StoryBook/wwwroot/js/explore.js`
- [ ] T037 [US4] Ensure Explore JavaScript never initializes from query string and never writes search/filter state to URL, history, `localStorage`, `sessionStorage`, cookies, or server state in `StoryBook/wwwroot/js/explore.js`
- [ ] T038 [US4] Add light/dark theme-compatible tokens, visible focus states, 44x44 CSS px interactive targets, and responsive 375px/768px/1366px layout rules in `StoryBook/wwwroot/css/explore.css`

**Checkpoint**: User Story 4 keeps `/explore` consistent with existing site language, theme, and accessibility behavior.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature against the quickstart, constitution quality gates, and project constraints.

- [ ] T039 [P] Review child-friendly copy and fallback states against `specs/004-sitewide-explore-search/quickstart.md` and update `StoryBook/Pages/Explore/Index.cshtml`
- [ ] T040 [P] Review no-new-dependency constraint in `StoryBook/StoryBook.csproj` and `StoryBook.Tests/StoryBook.Tests.csproj`
- [ ] T041 Run `dotnet test StoryBook2.sln` for `StoryBook2.sln`
- [ ] T042 Run `dotnet build StoryBook2.sln` for `StoryBook2.sln` and verify no new compile warnings
- [ ] T043 Run the manual quickstart route, search, filter, language, theme, keyboard, responsive, and SC-001/SC-008 usability-sampling checks from `specs/004-sitewide-explore-search/quickstart.md`
- [ ] T044 Review `StoryBook/Services/ExplorationCatalogService.cs`, `StoryBook/Services/ExplorationSearchService.cs`, `StoryBook/wwwroot/js/explore.js`, and `StoryBook/Pages/Explore/Index.cshtml` for no unresolved placeholders, no secrets, no user search-string logging, and zh-TW user-visible fallback text

---

## Dependencies & Execution Order

### Phase Dependencies

- **Test & Acceptance Baseline (Phase 1)**: No production implementation starts until these failing tests and acceptance checks exist.
- **Setup & Foundational Implementation (Phase 2)**: Depends on Phase 1; blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers MVP navigation and full collection rendering.
- **User Story 2 (Phase 4)**: Depends on Foundational and can proceed alongside US1 with coordination on `StoryBook/Pages/Explore/Index.cshtml` and `StoryBook/wwwroot/js/explore.js`.
- **User Story 3 (Phase 5)**: Depends on Foundational and the shared search/filter service contract; can proceed after US2 tests define `ExplorationSearchService`.
- **User Story 4 (Phase 6)**: Depends on rendered Explore controls and result cards from US1-US3.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: First recommended increment after foundation; no dependency on other stories.
- **US2 (P1)**: Uses the `/explore` page surface and projection; can be implemented after foundation if it owns the search-specific markup/script changes.
- **US3 (P2)**: Builds on the search/filter service and result metadata, so implement after US2 or coordinate changes to `ExplorationSearchService` and `explore.js`.
- **US4 (P2)**: Cross-cutting language/theme/accessibility pass after the controls and cards exist.

### Within Each User Story

- Confirm the relevant Phase 1 tests fail for the expected missing behavior before implementing that story.
- Models and projection contracts precede services.
- Services precede Razor PageModel and Razor markup.
- Razor markup precedes feature JavaScript behavior.
- CSS polish follows working controls and states.
- Run the story-specific tests after each story checkpoint.

---

## Parallel Opportunities

- T012 can run in parallel with T013 after T011 establishes the page shell.
- T014-T019 can run in parallel because they touch separate model files.
- T039 and T040 can run in parallel during polish because they validate different file sets.

Tasks that edit `StoryBook.Tests/Integration/ExplorePagesTests.cs`, `StoryBook.Tests/Unit/ExplorationCatalogServiceTests.cs`, `StoryBook.Tests/Unit/ExplorationSearchServiceTests.cs`, `StoryBook/Pages/Explore/Index.cshtml`, `StoryBook/wwwroot/js/explore.js`, or `StoryBook/wwwroot/css/explore.css` are intentionally not marked `[P]` when they share the same target file.

---

## Parallel Example: Foundational Models

```bash
Task: "T014 Add ExplorationSourceType with source order and route prefix metadata in StoryBook/Models/ExplorationSourceType.cs"
Task: "T015 Add ExplorationFacetValue model with bilingual labels and sort metadata in StoryBook/Models/ExplorationFacetValue.cs"
Task: "T016 Add ExplorationFacetGroup model with single-selection metadata in StoryBook/Models/ExplorationFacetGroup.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 test and acceptance baseline.
2. Complete Phase 2 foundational projection and status handling.
3. Complete Phase 3 User Story 1.
4. Stop and validate: homepage link, `/explore` render, source cards, detail links, partial/all failure states.

### Incremental Delivery

1. Add US1 for a navigable all-content Explore MVP.
2. Add US2 for cross-storybook search and clear behavior.
3. Add US3 for grouped classification filters and search/filter intersection.
4. Add US4 for language, theme, accessibility, and responsive hardening.
5. Finish with full `dotnet test StoryBook2.sln`, `dotnet build StoryBook2.sln`, quickstart validation, and no-placeholder/no-secret/no-sensitive-logging review.

### Parallel Team Strategy

1. Complete the test and acceptance baseline together.
2. Complete Setup and Foundational phases with model files split by owner.
3. After foundation, split work by story while coordinating shared files:
   - Developer A: US1 route, homepage entry, result cards.
   - Developer B: US2 search service and client behavior.
   - Developer C: US3 facet projection and filter behavior.
   - Developer D: US4 language/theme/accessibility checks after controls exist.
4. Merge by priority order and rerun the relevant unit/integration tests after each story.

---

## Notes

- Do not add NuGet packages, JavaScript packages, external APIs, databases, cookies, login, or server-side preference endpoints for this feature.
- Preserve canonical detail links: `/dinosaurs/{slug}` and `/aquarium/{slug}` only.
- Do not add `/explore/{slug}` or any JavaScript-only router.
- Search and filter state must remain page-local and reset on reload/re-entry.
- Keep theme control only on the homepage; `/explore` must apply the existing effective theme without rendering a selector.
- Use `living-area` as the canonical facet group id for water/aquarium environment categories; visible labels may use localized child-friendly wording.
