# Implementation Plan: 兒童恐龍介紹網站

**Branch**: `001-dinosaur-intro-site` | **Date**: 2026-05-09 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/001-dinosaur-intro-site/spec.md`; technical stack draft from `markdownFolder/tempPlan.md`

## Summary

在既有 `StoryBook` ASP.NET Core Razor Pages 應用中新增兒童友善的恐龍介紹體驗。核心作法是維持單一 Razor Pages 專案，使用伺服器端頁面與薄 PageModel 呈現首頁入口、恐龍清單、可直接連結的恐龍詳情頁，上一頁/下一頁與搜尋結果使用一般 anchor link 保留瀏覽器歷史紀錄，並以少量原生 JavaScript 支援即時搜尋、語言切換與 Bootstrap 大圖 modal。恐龍內容使用 repository 內的 JSON 內容檔，不引入資料庫或外部服務。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 / ASP.NET Core 10.0；既有專案 `StoryBook/StoryBook.csproj` 已設定 `net10.0`、nullable enabled、implicit usings enabled。  
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、原生 JavaScript；既有 jQuery 與 validation assets 保留給模板/驗證用途，本功能新互動邏輯不依賴 jQuery。  
**Storage**: JSON 內容檔 `StoryBook/Data/dinosaurs.json` 作為恐龍資料來源；圖片與插圖放在 `StoryBook/wwwroot/images/dinosaurs/`。不使用資料庫、登入、外部 CMS 或即時翻譯服務。  
**Testing**: 新增 `StoryBook.Tests` 測試專案，使用 xUnit、`Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory<Program>`；純資料搜尋、語言選擇與導覽計算使用單元測試，路由與 Razor Pages pipeline 使用整合測試，鍵盤操作、大圖與視覺流程以 quickstart 手動驗收，必要時再補 Playwright for .NET。  
**Target Platform**: 桌面與筆電瀏覽器：Chrome、Firefox、Safari、Edge；手機與平板深度最佳化不屬於本階段必要範圍，但 768px 以上寬度需維持基本回應式版面、不得水平捲動，且互動控制項仍可鍵盤操作。
**Project Type**: 單一 ASP.NET Core Razor Pages web application。  
**Performance Goals**: 首頁到恐龍介紹入口 3 秒內可見；恐龍詳情頁本機內容載入 3 秒內完成；搜尋輸入更新 1 秒內反映結果；語言切換 2 秒內更新所有可見文字；本機 JSON 內容查詢 p95 低於 200ms。  
**Constraints**: 內容必須適合 5-10 歲孩童；每篇簡介每種語言不超過 200 個可閱讀單位；小故事每種語言 100-150 個可閱讀單位；上一頁、下一頁、回首頁、搜尋清除與語言切換等互動目標至少 44x44 CSS px；所有互動控制項可鍵盤操作並有焦點狀態；圖片必須有有意義替代文字；每筆內容必須有唯一可直接開啟網址並保留瀏覽器歷史流程。
**Scale/Scope**: 8 筆史前生物、2 種語言、首頁、恐龍清單/搜尋頁、恐龍詳情頁、找不到內容狀態、大圖 modal；不含帳號、收藏、評論、後台內容管理或外部百科搜尋。  
**Observability/Logging**: 使用 ASP.NET Core 內建 `ILogger<T>` 記錄資料載入失敗、找不到 slug、內容驗證錯誤與非預期例外；暫不新增 Serilog，除非後續營運需求需要 file sink 或集中式結構化日誌並在任務中記錄理由。

### Frontend Strategy

採用 Razor Pages 伺服器端輸出為主，Bootstrap 5 提供 layout、按鈕、form control 與 modal 基礎樣式，原生 JavaScript 負責：

- 即時搜尋已渲染或已序列化的 8 筆內容摘要。
- 使用 `localStorage` 保存語言偏好，並在頁面載入時套用中文/英文文案。
- 開啟與關閉 Bootstrap 大圖 modal，支援 Escape 與焦點回復。
- 在不引入 SPA router 的前提下，由 Razor Pages 輸出上一頁、下一頁與搜尋結果的一般連結，保留瀏覽器上一頁/下一頁行為。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate Result | Plan Evidence |
|-----------|-------------|---------------|
| I. 程式碼品質至上 | PASS | 維持單一 Razor Pages app；PageModel 保持薄層；資料查詢、語言與導覽規則移至 injectable services；公開 service、DTO、options 需 XML 註解。 |
| II. 測試優先開發 | PASS | 任務必須先建立 xUnit 單元測試與 `WebApplicationFactory<Program>` 整合測試，再實作內容載入、搜尋、路由與錯誤狀態。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 使用 Razor Pages、Tag Helpers、Bootstrap 5 與既有 layout；頁面文案以繁體中文為預設，支援基本 WCAG 2.1 AA，並在 768px 以上寬度維持基本回應式版面。 |
| IV. 安全、設定與資料保護 | PASS | 無登入與寫入型表單；搜尋字串與 slug 只作為本機內容查詢並進行正規化；不提交 secrets；不依賴 UI 隱藏作安全控制。 |
| V. 可觀察性、效能與營運準備 | PASS | 使用 `ILogger<T>` 記錄資料與路由異常；內容規模小且本機 JSON 可快取；效能目標可用 quickstart 與測試驗證。 |

**Post-Design Re-check**: PASS。Phase 0/1 設計仍維持單一 Razor Pages 架構、本機 JSON 內容、測試先行與無外部服務；沒有需要在 Complexity Tracking 記錄的憲章例外。

## Project Structure

### Documentation (this feature)

```text
specs/001-dinosaur-intro-site/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── content-catalog.schema.json
│   └── ui-routes.md
└── tasks.md
```

### Source Code (repository root)

```text
StoryBook2.sln
StoryBook/
├── Program.cs
├── StoryBook.csproj
├── Data/
│   └── dinosaurs.json
├── Models/
│   ├── DinosaurIllustration.cs
│   ├── DinosaurProfile.cs
│   ├── DinosaurSearchResult.cs
│   ├── DinosaurStory.cs
│   ├── DinosaurText.cs
│   └── LanguageCode.cs
├── Services/
│   ├── DinosaurCatalogOptions.cs
│   ├── DinosaurCatalogService.cs
│   ├── DinosaurContentValidator.cs
│   └── LanguagePreferenceService.cs
├── Pages/
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   └── Dinosaurs/
│       ├── Index.cshtml
│       ├── Index.cshtml.cs
│       ├── Details.cshtml
│       └── Details.cshtml.cs
└── wwwroot/
    ├── css/
    │   └── dinosaurs.css
    ├── js/
    │   └── dinosaurs.js
    └── images/
        └── dinosaurs/

StoryBook.Tests/
├── StoryBook.Tests.csproj
├── Unit/
│   ├── DinosaurCatalogServiceTests.cs
│   ├── DinosaurContentValidationTests.cs
│   └── LanguagePreferenceServiceTests.cs
└── Integration/
    ├── DinosaurPagesTests.cs
    └── RoutingAndFallbackTests.cs
```

**Structure Decision**: 採用既有單一 Razor Pages web app，新增 feature folder `Pages/Dinosaurs/` 與集中式 `Services/`、`Models/`。內容資料放在 `StoryBook/Data/`，避免把資料讀取邏輯綁死在公開 web root；公開圖片資產仍放在 `wwwroot/images/dinosaurs/`。測試以獨立 `StoryBook.Tests` 專案加入 solution，讓 TDD 與整合測試不污染 production app。

## Complexity Tracking

No constitution violations require exception tracking.
