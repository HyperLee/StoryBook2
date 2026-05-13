# Implementation Plan: 網站深色模式與主題切換

**Branch**: `003-dark-mode` | **Date**: 2026-05-13 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/003-dark-mode/spec.md`; user-supplied technical context adapted from the existing dinosaur/storybook architecture to the dark-mode feature scope.

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

在既有 `StoryBook` ASP.NET Core Razor Pages 應用中新增「首頁主題選擇」與「整站有效主題套用」。核心作法是維持單一 Razor Pages 應用與共用 layout，於首頁提供「亮色模式」、「深色模式」、「跟隨系統」三選一控制項，將使用者選擇保存到瀏覽器 `localStorage` 的 `storybook.theme`，並在所有可直接瀏覽的頁面透過早期主題初始化、Bootstrap 5 theme attributes、共用 CSS variables 與原生 JavaScript 套用一致有效主題。此功能不新增資料庫、登入、外部服務、SPA router 或前端 build pipeline，也不改變恐龍與水族館既有內容資料、搜尋、語言偏好或路由。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 / ASP.NET Core 10.0；既有 `StoryBook/StoryBook.csproj` 已設定 `net10.0`、nullable enabled、implicit usings enabled。
**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、原生 JavaScript；既有 jQuery 與 validation assets 保留給模板/驗證用途，本功能新互動邏輯不依賴 jQuery。
**Storage**: 使用瀏覽器 `localStorage` 保存使用者主題模式，key 固定為 `storybook.theme`，值只允許 `light`、`dark`、`system`；不保存執行期間推導出的有效主題。本功能不修改 `StoryBook/Data/dinosaurs.json` 或 `StoryBook/Data/aquarium.json`，也不使用資料庫、登入、外部 CMS 或即時翻譯服務。
**Testing**: 沿用 `StoryBook.Tests` xUnit 專案與 `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory<Program>`；主題 metadata、允許值與 fallback 可用單元測試覆蓋，layout script ordering、首頁 selector、非首頁無 selector、主要 routes 輸出 theme contract 使用整合測試覆蓋；系統外觀變更、跨分頁同步、首次呈現閃爍、鍵盤操作、焦點與對比依 `quickstart.md` 手動驗收，若回歸成本升高再補 Playwright for .NET。
**Target Platform**: 桌面與筆電瀏覽器 Chrome、Firefox、Safari、Edge；依 spec 與 quickstart 需檢查手機、平板與桌面寬度，代表寬度至少包含 375px、768px 與 1366px。所有檢查寬度下，主題控制項與主要頁面文字、按鈕、卡牌、圖片區塊與表單都不得重疊或水平溢出，且主要控制項必須可操作。
**Project Type**: 單一 ASP.NET Core Razor Pages web application。
**Performance Goals**: 頁面首次可見呈現前套用有效主題以避免相反主題可見閃爍；首頁切換主題後 1 秒內更新目前頁面與後續瀏覽頁面；選擇跟隨系統時，系統外觀偏好變更後 2 秒內更新；同站其他已開啟分頁在 2 秒內同步；主題切換不得讓故事搜尋、語言切換或圖片檢視狀態中斷。
**Constraints**: 主題選項固定為亮色、深色、跟隨系統；首頁以外頁面必須套用有效主題但不得提供主題選擇控制項；互動目標至少 44x44 CSS px；所有互動控制項可鍵盤操作並有可見焦點；三種模式都需符合 WCAG 2.2 AA 對比與焦點指示；主題文字需跟隨 `storybook.language`，缺漏或無效語言回退繁體中文；主題切換不得修改故事內容、圖片說明、搜尋輸入、語言偏好、導覽位置或瀏覽器 history。
**Scale/Scope**: 3 種主題模式、2 種有效主題、首頁 selector、共用 layout/theme assets、首頁、Privacy、Error、恐龍首頁、恐龍詳情頁、水族館首頁與水族館詳情頁；不含自訂色票、排程切換、帳號同步、每頁獨立主題、後台設定或外部同步。
**Observability/Logging**: 主題偏好主要在瀏覽器端解析，無 server-side 使用者偏好寫入。若新增 `ThemePreferenceService` 或 layout metadata service，使用 ASP.NET Core 內建 `ILogger<T>` 只記錄非敏感的設定/渲染異常；不記錄 localStorage 內容、瀏覽器環境細節或任何 secrets，暫不新增 Serilog。

### Frontend Strategy

採用共用 layout + 共用 theme assets：

- `_Layout.cshtml` 的 `<head>` 放置極小的同步 theme boot script，在 stylesheet 載入前讀取 `storybook.theme`、判斷 `prefers-color-scheme`，並設定 `document.documentElement` 的 `data-bs-theme`、`data-storybook-theme-mode` 與 `data-storybook-effective-theme`。
- `StoryBook/wwwroot/js/theme.js` 使用原生 JavaScript 處理首頁控制項、`localStorage` 寫入、`matchMedia('(prefers-color-scheme: dark)')` 變更、`storage` 跨分頁同步與 bilingual label 狀態；此檔是跨站共用主題功能，不塞進 dinosaur/aquarium feature script。
- `StoryBook/wwwroot/css/site.css` 定義全站 semantic CSS variables、body/layout/navbar/footer/form/focus 基礎主題樣式；`dinosaurs.css` 與 `aquarium.css` 只補 feature-specific token overrides，避免修改 vendor Bootstrap 檔。
- 首頁 `Pages/Index.cshtml` 顯示唯一主題 selector；Privacy、Error、恐龍與水族館頁只接收已套用的有效主題，不渲染 selector。
- 不建立 JavaScript-only router；主題切換只改變視覺呈現與可及性狀態，不改動 anchor navigation、搜尋資料、語言偏好或 modal 流程。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate Result | Plan Evidence |
|-----------|-------------|---------------|
| I. 程式碼品質至上 | PASS | 維持單一 Razor Pages app；跨站主題邏輯集中於共用 layout、`site.css`、`theme.js` 與小型 theme metadata service；不修改 vendor assets；公開 reusable service/DTO 需 XML 註解。 |
| II. 測試優先開發 | PASS | 實作前先新增單元測試與整合測試，覆蓋允許主題值、fallback、layout contract、首頁 selector、非首頁無 selector 與主要 routes theme attributes；瀏覽器環境行為列入 quickstart 驗收。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 使用 Razor Pages、Tag Helpers、Bootstrap 5、共用 layout 與首頁 Page；主題 selector 只出現在首頁，其他頁沿用一致 layout；控制項需 keyboard/focus/accessibility contract。 |
| IV. 安全、設定與資料保護 | PASS | 不新增登入、寫入型表單、外部服務或 server-side 個人化儲存；localStorage 值白名單驗證，無效值回退 `system`；不提交 secrets 或 connection strings。 |
| V. 可觀察性、效能與營運準備 | PASS | 主題功能不引入背景流程或外部依賴；首次呈現、切換、系統同步與跨分頁同步都有可驗收效能目標；只使用內建 logging 記錄非敏感 server-side 異常。 |

**Post-Design Re-check**: PASS。Phase 0/1 設計仍維持單一 Razor Pages 架構、內建 Bootstrap/theme attributes、原生 JavaScript、測試先行與無外部服務；沒有需要在 Complexity Tracking 記錄的憲章例外。

## Project Structure

### Documentation (this feature)

```text
specs/003-dark-mode/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── theme-ui.md
└── tasks.md             # 後續由 /speckit-tasks 建立，不由本次 /speckit-plan 產生
```

### Source Code (repository root)

```text
StoryBook2.sln
StoryBook/
├── Program.cs
├── StoryBook.csproj
├── Services/
│   ├── LanguagePreferenceService.cs
│   └── ThemePreferenceService.cs        # 新增：主題 key、允許模式與 label metadata
├── Pages/
│   ├── Index.cshtml                     # 新增首頁唯一 theme selector
│   ├── Index.cshtml.cs
│   ├── Privacy.cshtml
│   ├── Error.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml               # 新增 early boot script、theme attributes、theme.js reference
│   ├── Dinosaurs/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   └── Aquarium/
│       ├── Index.cshtml
│       └── Details.cshtml
└── wwwroot/
    ├── css/
    │   ├── site.css                     # 新增全站 theme tokens 與 shared components
    │   ├── dinosaurs.css                # 補深色模式 feature overrides
    │   └── aquarium.css                 # 補深色模式 feature overrides
    └── js/
        ├── site.js
        └── theme.js                     # 新增主題 selector、system mode、cross-tab sync

StoryBook.Tests/
├── StoryBook.Tests.csproj
├── Unit/
│   └── ThemePreferenceServiceTests.cs
└── Integration/
    └── ThemePagesTests.cs
```

**Structure Decision**: 採用既有單一 Razor Pages web app，不新增 SPA、Blazor、MVC、資料庫或外部服務。主題功能屬於跨站 UX 行為，因此放在共用 layout、`site.css` 與專用 `theme.js`，首頁只負責顯示 selector；恐龍與水族館 feature CSS 只補必要的深色 token/selector override，避免把整站主題邏輯分散到 feature scripts。

## Complexity Tracking

No constitution violations require exception tracking.
