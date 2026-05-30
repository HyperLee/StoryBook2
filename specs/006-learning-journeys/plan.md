# Implementation Plan: 主題學習旅程

**Branch**: `006-learning-journeys` | **Date**: 2026-05-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-learning-journeys/spec.md`

**Note**: This plan is filled by the `/speckit-plan` workflow and stops after Phase 1 design artifacts.

## Summary

新增 canonical routes `/journeys` 與 `/journeys/{slug}`，讓使用者從首頁與 `/explore` 進入主題學習旅程列表，查看策展路線、學習目標、建議閱讀時間、建議年齡與 3-5 筆既有故事項目。技術上維持單一 ASP.NET Core Razor Pages app、既有 Dinosaur/Aquarium catalog services、Bootstrap 5、原生 JavaScript 與 xUnit 測試；新增旅程 JSON catalog、旅程驗證/投影服務、Razor Pages、feature-specific CSS/JS 與 UI/data contracts，不新增資料庫、外部 API、SPA、即時翻譯或閱讀進度儲存。

## Technical Context

**Language/Version**: C# / .NET `net10.0`, ASP.NET Core Razor Pages, nullable enabled, implicit usings enabled; browser 端只使用原生 JavaScript 與 CSS。
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、`System.Text.Json`、既有 `DinosaurCatalogService` / `AquariumCatalogService`、既有 `ExplorationSourceType` source metadata、`LanguagePreferenceService`、`ThemePreferenceService`、xUnit、`Microsoft.AspNetCore.Mvc.Testing`；不新增 NuGet、JavaScript 套件、jQuery dependency 或外部服務。
**Storage**: 新增本機 JSON catalog `StoryBook/Data/journeys.json` 作為學習旅程資料來源；旅程故事項目只保存來源故事書、slug 與排序。既有 `StoryBook/Data/dinosaurs.json`、`StoryBook/Data/aquarium.json` 繼續作為顯示名稱、摘要與詳情連結來源。不新增資料庫、cookie endpoint、server-side user preference、閱讀進度儲存或外部 CMS。
**Storage Governance**: `journeys.json` 是 repository 版本控管的唯讀策展內容；備份與回復依 Git history、code review 與單一路徑 revert 流程處理，不在 runtime 寫入或遷移。資料模型以 `data-model.md` 與 `contracts/learning-journeys.schema.json` 為準；schema/data 變更必須同步更新 validator、測試與 quickstart。並發處理以 immutable cached catalog/projection 為界線，使用者請求只讀取已驗證 snapshot，避免多 request 寫入競爭。
**Testing**: `dotnet test StoryBook2.sln`；新增/更新 xUnit 單元測試覆蓋旅程 JSON 載入、content validation、slug uniqueness、3-5 有效故事規則、來源/slug resolution、重複引用、排序、雙語 fallback、不可用狀態與非敏感診斷摘要；整合測試覆蓋 `/journeys`、`/journeys/{slug}`、首頁與 `/explore` 入口、HTML contract、theme selector absence、friendly not-found/unavailable/all-failed states 與一般 anchor 導覽。瀏覽器語言切換、主題套用、鍵盤操作與 375/768/1366px 視覺流程以 quickstart 手動驗收，除非後續 plan 明確新增 browser automation runner。
**Target Platform**: 單一 ASP.NET Core web application，開發 URL 以 `dotnet run --project StoryBook/StoryBook.csproj` 輸出為準；支援 Chrome、Firefox、Safari、Edge，並在 375px、768px、1366px 代表寬度維持無水平溢出、文字不重疊與可鍵盤操作。
**Project Type**: Razor Pages web app + xUnit test project。
**Performance Goals**: 完整資料情境下 `/journeys` 與 `/journeys/{slug}` 主要內容在一般本機開發環境 1 秒內可用；本機 JSON 載入與旅程 projection 使用 singleton cached catalog，查詢 p95 目標低於 200ms；語言切換與主題有效外觀同步沿用既有 2 秒內規則。
**Performance Verification**: Phase 7 必須依 quickstart 在完整 catalog 狀態下，對 `/journeys` 與一個已知 `/journeys/{slug}` 各執行 5 次 warm-load 驗收並記錄結果；每次主要內容需在 1 秒內可用。若採用測試 fixture 或瀏覽器任務紀錄替代人工量測，紀錄需包含 route、樣本次數、是否達標與非敏感環境說明。
**Constraints**: `/journeys` 只顯示完整可出發旅程；有效故事項目少於 3 筆或超過 5 筆的旅程從列表隱藏，直接開啟詳情頁顯示友善不可用提示；旅程項目導覽使用一般 anchor 指向 `/dinosaurs/{slug}` 或 `/aquarium/{slug}`；不得建立重複故事詳情頁、JavaScript-only router、登入、收藏、評論、個人化推薦、閱讀進度、外部百科搜尋、即時翻譯或新故事書來源；旅程頁不得提供新的主題 selector；所有控制項至少 44x44 CSS px 且具 accessible name 與可見焦點。
**Scale/Scope**: 初始整合 2 個既有來源故事書、23 筆既有故事朋友、至少 3 條完整旅程、每條 3-5 筆有效故事項目、2 種語言、列表頁、詳情頁、開始閱讀動作、來源部分失敗/全部失敗/旅程不可用/找不到旅程狀態。
**Observability/Logging**: 使用 `ILogger<LearningJourneyCatalogService>` 記錄旅程資料驗證失敗、旅程不可用原因、來源故事書解析失敗與未知旅程 slug 的非敏感摘要；旅程功能新增 log 不記錄檔案路徑、exception detail、stack trace、secret、個資或使用者閱讀狀態。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. 程式碼品質至上 | PASS | 保持單一 Razor Pages 應用；新增旅程 catalog、validator、projection DTO 與 service 使用清楚邊界；PageModel 僅載入 snapshot 與呈現狀態；公開可重用 service/DTO/options 需加 XML 註解。 |
| II. 測試優先開發 | PASS | Tasks 必須先建立失敗單元/整合測試，再實作旅程 validation、source resolution、route、入口、HTML contract、fallback 與 failure states。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 新頁面位於 Razor Pages，沿用 Tag Helpers、Bootstrap、共用 layout、語言 `data-i18n` 合約、既有主題 attributes、feature-specific CSS/JS 與一般 anchor 導覽。 |
| IV. 安全、設定與資料保護 | PASS | 無登入、寫入型表單、外部 API、secrets 或個人資料；旅程只讀取本機 JSON 與既有 catalog；使用者可見錯誤與新增 journey logs 不暴露檔案路徑、exception details、secret 或內部設定。 |
| V. 可觀察性、效能與營運準備 | PASS | 旅程資料與來源解析失敗由 sanitized `ILogger<T>` summary 記錄；資料量小且以 singleton cached catalog/projection 達成 1 秒主要內容目標；不新增背景流程或營運依賴。 |

**Post-Design Re-check**: PASS。Phase 1 artifacts 維持同一技術棧、無新增外部依賴、無憲章例外；Complexity Tracking 無項目。

## Project Structure

### Documentation (this feature)

```text
specs/006-learning-journeys/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── learning-journeys.schema.json
│   └── ui-routes.md
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit-tasks 產生；本命令不建立
```

### Source Code (repository root)

```text
StoryBook/
├── Data/
│   └── journeys.json
├── Models/
│   ├── JourneyAvailabilityStatus.cs
│   ├── JourneyCatalog.cs
│   ├── JourneyCatalogSnapshot.cs
│   ├── JourneyDiagnosticSummary.cs
│   ├── JourneyStoryItem.cs
│   ├── JourneyStoryReference.cs
│   ├── LearningJourney.cs
│   └── JourneyText.cs
├── Pages/
│   ├── Journeys/
│   │   ├── Details.cshtml
│   │   ├── Details.cshtml.cs
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   ├── Explore/
│   │   └── Index.cshtml      # 增加「學習旅程」入口
│   └── Index.cshtml          # 增加「學習旅程」入口
├── Services/
│   ├── LearningJourneyCatalogOptions.cs
│   ├── LearningJourneyCatalogService.cs
│   ├── LearningJourneyContentValidationResult.cs
│   └── LearningJourneyContentValidator.cs
└── wwwroot/
    ├── css/
    │   └── journeys.css
    └── js/
        └── journeys.js

StoryBook.Tests/
├── Integration/
│   ├── JourneyPageTestFixture.cs
│   └── JourneyPagesTests.cs
└── Unit/
    ├── LearningJourneyCatalogServiceTests.cs
    └── LearningJourneyContentValidationTests.cs
```

**Structure Decision**: 使用現有單一 ASP.NET Core Razor Pages 應用。旅程 JSON 載入、驗證、來源故事解析、有效/不可用狀態與診斷摘要集中在 `LearningJourneyCatalogService` 與 `LearningJourneyContentValidator`；`PageModel` 保持薄層，只提供列表 snapshot、詳情 projection、feature asset flags 與友善狀態。`journeys.js` 只承接既有 `storybook.language` / `data-i18n-*` 合約與必要的頁面內語言文字更新，不保存旅程狀態、不改 URL/history、不處理主題切換。Feature-specific CSS/JS 放在 `wwwroot/css/journeys.css` 與 `wwwroot/js/journeys.js`，主題邏輯仍由既有 layout/theme service 與 `theme.js` 管理。

## Complexity Tracking

No constitution violations or complexity exceptions.
