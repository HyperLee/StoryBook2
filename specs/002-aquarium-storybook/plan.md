# Implementation Plan: 水族館動物介紹故事書

**Branch**: `002-aquarium-storybook` | **Date**: 2026-05-10 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/002-aquarium-storybook/spec.md`; draft outline from `markdownFolder/tempAquariumPlan.md`

## Summary

在既有 `StoryBook` ASP.NET Core Razor Pages 應用中新增「水族館動物介紹故事書」。核心作法是沿用恐龍故事書已建立的 Razor Pages、薄 PageModel、injectable services、本機 JSON 內容、Bootstrap 5 與原生 JavaScript 模式，新增 `/aquarium` 主頁與 `/aquarium/{slug}` 單一動物頁，固定提供 15 種水族館生物、至少 5 種生活區域分類、雙語內容、搜尋、上一頁/下一頁、主要圖片放大、故事插圖、資料讀取失敗狀態與回首頁導覽。內容資料放在 repository 內的 `StoryBook/Data/aquarium.json`，不放在公開 `wwwroot/data`，也不新增資料庫、外部 API、SPA router 或即時翻譯服務。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 / ASP.NET Core 10.0；既有 `StoryBook/StoryBook.csproj` 已設定 `net10.0`、nullable enabled、implicit usings enabled。
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、原生 JavaScript；保留既有 jQuery 與 validation assets，但水族館新互動不依賴 jQuery。
**Storage**: JSON 內容檔 `StoryBook/Data/aquarium.json` 作為水族館內容來源；圖片放在 `StoryBook/wwwroot/images/aquarium/`。不使用資料庫、登入、外部 CMS、外部百科 API 或即時翻譯服務。
**Testing**: 沿用既有 `StoryBook.Tests` 專案，使用 xUnit、`Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory<Program>`；服務規則使用單元測試，DI、路由、Razor Pages pipeline、404 與 HTML contract 使用整合測試，圖片 modal、鍵盤操作、語言切換與視覺流程依 `quickstart.md` 手動驗收。
**Target Platform**: 本階段正式驗收為桌機瀏覽器 Chrome、Firefox、Safari、Edge；行動裝置深度最佳化不列入本次交付，但基本 Bootstrap 回應式版面不得造成主要內容水平捲動。
**Project Type**: 單一 ASP.NET Core Razor Pages web application。
**Performance Goals**: 首頁到 `/aquarium` 入口 3 秒內可理解如何開始閱讀；從 `/aquarium` 開始閱讀第一隻動物 5 秒內完成；搜尋結果、過短搜尋提示或無結果提示需在輸入變更後 1 秒內更新到畫面；本機 catalog lookup/search p95 低於 200ms；語言切換 2 秒內更新主要可見文字。
**Constraints**: 固定 15 種水族館生物；至少 5 種生活區域分類；每篇簡介每種語言不超過 200 個可閱讀單位；每則故事每種語言 100-150 個可閱讀單位；繁體中文以可見中文字元計算，英文以單字計算，空白與標點不計入；搜尋正規化後少於 2 個有效搜尋字元或英文字母數字時視為過短；所有互動控制項可鍵盤聚焦與啟用；語言偏好使用 `localStorage` key `storybook.language`；無效偏好或缺漏內容回退 `zh-TW`，不得顯示空白內容區塊。
**Scale/Scope**: 15 筆內容、2 種語言、至少 5 種生活區域分類、首頁入口、水族館主頁、動物詳情頁、搜尋狀態、友善 404、圖片放大視圖；不含帳號、收藏、測驗、社群分享、後台管理或外部資料整合。
**Observability/Logging**: 使用 ASP.NET Core 內建 `ILogger<T>` 記錄資料載入失敗、內容驗證錯誤、未知 slug、缺圖或非預期例外；暫不新增 Serilog，除非後續營運需求明確要求 file sink 或集中式結構化日誌並在 plan/tasks 中記錄理由。

### Frontend Strategy

採用 Razor Pages 伺服器端輸出主要內容與一般 anchor link，Bootstrap 5 提供版面、按鈕、表單與 modal 基礎樣式，`StoryBook/wwwroot/js/aquarium.js` 使用原生 JavaScript 處理：

- 讀取與套用 `storybook.language`，切換 `zh-TW` / `en` 可見文字與內容。
- 即時搜尋已渲染或已序列化的 15 筆摘要，支援中英文、大小寫忽略與空白正規化。
- 開啟與關閉圖片放大視圖，支援 Escape、明確關閉控制與焦點回復。
- 不建立 JavaScript-only router；上一頁、下一頁、搜尋結果、回水族館主頁與回首頁全部使用一般連結。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate Result | Plan Evidence |
|-----------|-------------|---------------|
| I. 程式碼品質至上 | PASS | 維持單一 Razor Pages app；新增內容查詢、搜尋、導覽與驗證規則到 injectable services；PageModel 保持薄層；公開 service、DTO、options 需 XML 註解。 |
| II. 測試優先開發 | PASS | 實作前先新增或更新 xUnit 單元測試與 `WebApplicationFactory<Program>` 整合測試，覆蓋 catalog validation、搜尋、語言 fallback、上一頁/下一頁、路由與 404。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 使用 Razor Pages、Tag Helpers、Bootstrap 5 與共用 layout；新增 `Pages/Aquarium/` feature pages；互動控制維持 keyboard/focus/accessibility contract。 |
| IV. 安全、設定與資料保護 | PASS | 無登入與寫入型表單；slug 與搜尋字串只作本機內容查詢並正規化；資料檔不含 secrets；不新增外部服務或公開未審核 JSON endpoint。 |
| V. 可觀察性、效能與營運準備 | PASS | 使用 `ILogger<T>` 記錄資料與路由異常；內容規模小，JSON 可於 service 中快取；效能目標可由單元測試、整合測試與 quickstart 驗證。 |

**Post-Design Re-check**: PASS。Phase 0/1 設計仍維持單一 Razor Pages 架構、本機 JSON、內建 logging、測試先行與無外部服務；沒有需要在 Complexity Tracking 記錄的憲章例外。

## Project Structure

### Documentation (this feature)

```text
specs/002-aquarium-storybook/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── content-catalog.schema.json
│   └── ui-routes.md
└── tasks.md             # 後續由 /speckit-tasks 建立，不由本次 /speckit-plan 產生
```

### Source Code (repository root)

```text
StoryBook2.sln
StoryBook/
├── Program.cs
├── StoryBook.csproj
├── Data/
│   ├── dinosaurs.json
│   └── aquarium.json
├── Models/
│   ├── AquariumAnimalProfile.cs
│   ├── AquariumCatalog.cs
│   ├── AquariumHabitatCategory.cs
│   ├── AquariumImage.cs
│   ├── AquariumSearchResult.cs
│   ├── AquariumStory.cs
│   └── AquariumText.cs
├── Services/
│   ├── AquariumCatalogOptions.cs
│   ├── AquariumCatalogService.cs
│   └── AquariumContentValidator.cs
├── Pages/
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   └── Aquarium/
│       ├── Index.cshtml
│       ├── Index.cshtml.cs
│       ├── Details.cshtml
│       └── Details.cshtml.cs
└── wwwroot/
    ├── css/
    │   └── aquarium.css
    ├── js/
    │   └── aquarium.js
    └── images/
        └── aquarium/

StoryBook.Tests/
├── StoryBook.Tests.csproj
├── Unit/
│   ├── AquariumCatalogServiceTests.cs
│   ├── AquariumContentValidationTests.cs
│   └── LanguagePreferenceServiceTests.cs
└── Integration/
    └── AquariumPagesTests.cs
```

**Structure Decision**: 採用既有單一 Razor Pages web app，不新增 SPA、Blazor、MVC、資料庫或外部服務。水族館功能使用 `Pages/Aquarium/` feature folder，資料與規則放在 `Models/`、`Services/`，內容 JSON 放在非公開的 `StoryBook/Data/`，公開圖片、CSS、JavaScript 放在 `wwwroot` 下的 feature-specific 目錄。`LanguagePreferenceService` 與 `LanguageCode` 可沿用既有共享語言偏好邏輯；若實作時更新其 XML 註解，需讓描述從恐龍專用改為故事書通用。

## Complexity Tracking

No constitution violations require exception tracking.
