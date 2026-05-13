# Implementation Plan: 全站探索與分類搜尋

**Branch**: `004-sitewide-explore-search` | **Date**: 2026-05-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-sitewide-explore-search/spec.md`

**Note**: This plan is filled by the `/speckit-plan` workflow and stops after Phase 1 design artifacts.

## Summary

新增 canonical route `/explore` 的 Razor Page，從既有恐龍與水族館 JSON catalog 透過 injectable services 組成全站探索 projection。頁面以伺服器端 Razor 輸出完整可探索集合、雙語文字、穩定排序與分類 facets；原生 JavaScript 只負責目前頁面生命週期內的搜尋、單一群組篩選、清除與狀態提示，不寫入 URL、history 或跨頁儲存。結果連結維持一般 anchor，導向既有 `/dinosaurs/{slug}` 與 `/aquarium/{slug}` 詳情頁。

## Technical Context

**Language/Version**: C# / .NET `net10.0`, ASP.NET Core Razor Pages, nullable enabled, implicit usings enabled; browser互動使用原生 JavaScript 與 CSS。  
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、既有 `DinosaurCatalogService` / `AquariumCatalogService`、xUnit、`Microsoft.AspNetCore.Mvc.Testing`；不新增 NuGet、JavaScript 套件或外部服務。  
**Storage**: 既有本機 JSON catalog：`StoryBook/Data/dinosaurs.json`、`StoryBook/Data/aquarium.json`；不新增資料庫、cookie preference endpoint 或外部內容來源。  
**Testing**: `dotnet test StoryBook2.sln`；新增/更新 xUnit 單元測試覆蓋 projection、搜尋正規化、分類 AND 規則、穩定排序與部分來源失敗；整合測試覆蓋 `/explore` route、首頁入口、HTML contract、theme selector absence、非 JavaScript-only 導覽。  
**Target Platform**: 單一 ASP.NET Core web application，開發 URL 以 `dotnet run --project StoryBook/StoryBook.csproj` 輸出為準。  
**Project Type**: Razor Pages web app + xUnit test project。  
**Performance Goals**: `/explore` 初始載入與搜尋/篩選狀態更新須符合規格 1 秒內顯示結果或提示；目前初始資料量為 23 筆內容，使用本機記憶體 projection 與 DOM show/hide 足以達成。  
**Constraints**: `/explore` 為唯一全站探索 canonical route；搜尋與分類狀態只存在於目前頁面生命週期，不寫入 query string、history、localStorage 或 session；搜尋需同時比對 `zh-TW` 與 `en` 索引文字；每個分類群組一次只能選一個條件，不同群組以 AND 合併；結果依來源分組並保留各 catalog `SortOrder`；探索頁不得提供新的主題 selector。  
**Scale/Scope**: 初始整合 2 個來源故事書、23 筆內容、來源/食性/生活區域/生活時期/發現地點等 facets；未來第三來源可透過新的 source adapter 擴充，但本 feature 不實作外部來源或管理介面。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. 程式碼品質至上 | PASS | 保持單一 Razor Pages 應用；新增 projection/search service 與 DTO 使用清楚邊界；公開可重用 service/DTO 需加 XML 註解；不新增不必要 abstraction 或外部套件。 |
| II. 測試優先開發 | PASS | Tasks 必須先建立失敗單元/整合測試，再實作 projection、搜尋、filter、route 與 HTML contract。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 新頁面位於 Razor Pages，沿用 Tag Helpers、Bootstrap、共用 layout、語言/主題 data attribute 模式與一般 anchor 導覽。 |
| IV. 安全、設定與資料保護 | PASS | 無登入、寫入型表單、外部 API 或 secrets；所有搜尋與 filter 輸入只作為本機比對條件並做正規化；錯誤訊息不暴露內部例外。 |
| V. 可觀察性、效能與營運準備 | PASS | 來源 catalog 載入失敗與 partial availability 由既有/新增 `ILogger<T>` 記錄；不記錄使用者搜尋字串以避免不必要資料保留；使用本機 cached catalog 與 DOM 篩選達成 1 秒目標。 |

**Post-Design Re-check**: PASS。Phase 1 artifacts 維持同一技術棧、無新增外部依賴、無憲章例外；Complexity Tracking 無項目。

## Project Structure

### Documentation (this feature)

```text
specs/004-sitewide-explore-search/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-routes.md
└── tasks.md              # /speckit-tasks 產生；本命令不建立
```

### Source Code (repository root)

```text
StoryBook/
├── Models/
│   ├── ExplorationFacetGroup.cs
│   ├── ExplorationFacetValue.cs
│   ├── ExplorationItem.cs
│   ├── ExplorationSearchState.cs
│   ├── ExplorationSourceType.cs
│   └── ExplorationSourceStatus.cs
├── Pages/
│   ├── Explore/
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   └── Index.cshtml          # 增加「探索全部故事」入口
├── Services/
│   ├── ExplorationCatalogService.cs
│   └── ExplorationSearchService.cs
└── wwwroot/
    ├── css/
    │   └── explore.css
    └── js/
        └── explore.js

StoryBook.Tests/
├── Integration/
│   └── ExplorePagesTests.cs
└── Unit/
    ├── ExplorationCatalogServiceTests.cs
    └── ExplorationSearchServiceTests.cs
```

**Structure Decision**: 使用現有單一 ASP.NET Core Razor Pages 應用。跨故事書 projection、搜尋與分類規則放在 injectable services；`PageModel` 只負責載入 view model、設定 feature asset flags 與呈現錯誤狀態。Feature-specific CSS/JS 放在 `wwwroot/css/explore.css` 與 `wwwroot/js/explore.js`，主題邏輯仍由既有 layout/theme service 與 `theme.js` 管理。來源 catalog 載入失敗、partial availability 與 all-failed 狀態必須由 `ExplorationCatalogService` 透過 `ILogger<ExplorationCatalogService>` 記錄可關聯事件，但不得記錄使用者搜尋字串、語言偏好之外的個人化資料或任何 secret。

## Complexity Tracking

No constitution violations or complexity exceptions.
