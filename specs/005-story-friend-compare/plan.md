# Implementation Plan: 內容比較器

**Branch**: `005-story-friend-compare` | **Date**: 2026-05-19 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-story-friend-compare/spec.md`

**Note**: This plan is filled by the `/speckit-plan` workflow and stops after Phase 1 design artifacts.

## Summary

新增 canonical route `/compare` 的 Razor Page，讓使用者從首頁與 `/explore` 進入內容比較器，選擇兩位不同故事朋友並查看來源、名稱、食性、生活區域、生活時期、發現地點、摘要與既有詳情頁連結。技術上沿用先前 `/explore` 功能的單一 Razor Pages app、既有 Dinosaur/Aquarium catalog services、Bootstrap 5、原生 JavaScript 與 CSS；新增比較專用 projection service 與頁面內互動，不新增套件、資料庫、外部 API、URL/query state 或跨頁儲存。

## Technical Context

**Language/Version**: C# / .NET `net10.0`, ASP.NET Core Razor Pages, nullable enabled, implicit usings enabled; browser互動使用原生 JavaScript 與 CSS。  
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、既有 `DinosaurCatalogService` / `AquariumCatalogService`、既有 `ExplorationSourceType` source metadata、xUnit、`Microsoft.AspNetCore.Mvc.Testing`；不新增 NuGet、JavaScript 套件或外部服務。  
**Storage**: 既有本機 JSON catalog：`StoryBook/Data/dinosaurs.json`、`StoryBook/Data/aquarium.json`；不新增資料庫、cookie preference endpoint、比較歷史、分享 URL 或外部內容來源。  
**Testing**: `dotnet test StoryBook2.sln`；新增/更新 xUnit 單元測試覆蓋比較候選 projection、stable id、來源排序、欄位 fallback、不適用文字、重複選擇規則與部分來源失敗；整合測試覆蓋 `/compare` route、首頁與 `/explore` 入口、HTML contract、theme selector absence、一般 anchor 詳情導覽與錯誤狀態。  
**Target Platform**: 單一 ASP.NET Core web application，開發 URL 以 `dotnet run --project StoryBook/StoryBook.csproj` 輸出為準。  
**Project Type**: Razor Pages web app + xUnit test project。  
**Performance Goals**: `/compare` 初始載入與選擇變更後的比較表或提示更新須符合規格 1 秒內完成；目前候選集合為 23 筆內容，伺服器端 projection 加頁面內 DOM 更新足以達成。  
**Constraints**: `/compare` 為唯一內容比較器 canonical route；比較選擇狀態只存在目前頁面生命週期，不寫入 query string、history、localStorage、sessionStorage、cookie 或 server state；候選識別為 `{sourceType, slug}`；同一候選短暫被選兩次時顯示友善提示且不顯示比較表；排序先恐龍後水族館並保留各 catalog `SortOrder`；比較頁不得提供新的主題 selector。  
**Scale/Scope**: 初始整合 2 個來源故事書、23 筆候選項目、7 個資料比較欄位與詳情連結；未來第三來源可透過新的 source adapter 擴充，但本 feature 不實作外部來源、管理介面、自動比較文字或使用者保存。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. 程式碼品質至上 | PASS | 保持單一 Razor Pages 應用；新增比較 projection service 與 DTO 使用清楚邊界；PageModel 僅載入 snapshot 與呈現狀態；公開可重用 service/DTO 需加 XML 註解。 |
| II. 測試優先開發 | PASS | Tasks 必須先建立失敗單元/整合測試，再實作候選 projection、欄位 fallback、重複選擇、route、入口與 HTML contract。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 新頁面位於 Razor Pages，沿用 Tag Helpers、Bootstrap、共用 layout、語言/主題 data attribute 模式、feature-specific CSS/JS 與一般 anchor 導覽。 |
| IV. 安全、設定與資料保護 | PASS | 無登入、寫入型表單、外部 API 或 secrets；比較選擇只在 DOM 記憶體內運作；錯誤訊息不得暴露檔案路徑、exception type 或 stack trace。 |
| V. 可觀察性、效能與營運準備 | PASS | catalog 載入失敗與 partial availability 由 `ILogger<T>` 記錄；不記錄使用者選擇組合；使用本機 cached catalog 與小型 DOM 更新達成 1 秒目標。 |

**Post-Design Re-check**: PASS。Phase 1 artifacts 維持同一技術棧、無新增外部依賴、無憲章例外；Complexity Tracking 無項目。

## Project Structure

### Documentation (this feature)

```text
specs/005-story-friend-compare/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-routes.md
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit-tasks 產生；本命令不建立
```

### Source Code (repository root)

```text
StoryBook/
├── Models/
│   ├── ComparisonCandidate.cs
│   ├── ComparisonCatalogSnapshot.cs
│   ├── ComparisonFieldDefinition.cs
│   ├── ComparisonFieldValue.cs
│   ├── ComparisonSelectionState.cs
│   └── ComparisonSourceStatus.cs
├── Pages/
│   ├── Compare/
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   ├── Explore/
│   │   └── Index.cshtml      # 增加「比較故事朋友」入口
│   └── Index.cshtml          # 增加「比較故事朋友」入口
├── Services/
│   └── ComparisonCatalogService.cs
└── wwwroot/
    ├── css/
    │   └── compare.css
    └── js/
        └── compare.js

StoryBook.Tests/
├── Integration/
│   └── ComparePagesTests.cs
└── Unit/
    └── ComparisonCatalogServiceTests.cs
```

**Structure Decision**: 使用現有單一 ASP.NET Core Razor Pages 應用。比較候選 projection、欄位 fallback、不適用文字與來源失敗狀態集中在 `ComparisonCatalogService` 與比較 DTO；`PageModel` 保持薄層，只提供 snapshot 與 feature asset flags。`compare.js` 只處理目前頁面內兩個選擇控制、清除、重複選擇提示與比較表顯示；詳情頁導覽維持一般 anchor。Feature-specific CSS/JS 放在 `wwwroot/css/compare.css` 與 `wwwroot/js/compare.js`，主題邏輯仍由既有 layout/theme service 與 `theme.js` 管理。

## Complexity Tracking

No constitution violations or complexity exceptions.
