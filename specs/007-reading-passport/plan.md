# Implementation Plan: 小小探險護照

**Branch**: `007-reading-passport` | **Date**: 2026-05-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/007-reading-passport/spec.md`

**Note**: This plan is filled by the `/speckit-plan` workflow and stops after Phase 1 design artifacts.

## Summary

新增 canonical route `/passport`、共用導覽與首頁護照入口，以及恐龍與水族館詳情頁的「我讀完了」完成控制。護照狀態只保存在同一瀏覽器的 `localStorage` key `storybook.passport`，格式固定為 `{ version, completedStories: [{ source, slug }] }`；伺服器只從既有恐龍與水族館 catalog 產生可顯示故事、總數、來源順序與固定徽章定義。技術上維持單一 ASP.NET Core Razor Pages app、Bootstrap 5、原生 JavaScript、既有語言/主題合約與 xUnit 測試；不新增資料庫、CardPicker `cards.json`、Web API、登入、跨裝置同步、Serilog、Moq、前端套件或 jQuery 依賴。

## Technical Context

**Language/Version**: C# / .NET `net10.0`，使用目前 SDK 對 `net10.0` 的預設 C# 語言版本；ASP.NET Core Razor Pages、nullable enabled、implicit usings enabled。瀏覽器端新增互動使用原生 JavaScript 與 CSS。

**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、`System.Text.Json`、既有 `DinosaurCatalogService` / `AquariumCatalogService`、既有 `ExplorationSourceType` source metadata、`LanguagePreferenceService`、`ThemePreferenceService`、xUnit、`Microsoft.AspNetCore.Mvc.Testing`。現有 jQuery 與 jQuery Validation 可繼續留給範本/驗證資產，但護照功能不依賴 jQuery。此功能不新增 Serilog/Moq/NuGet/JavaScript 套件；logging 沿用 ASP.NET Core `ILogger<T>`。

**Storage**: 護照閱讀狀態只使用瀏覽器 `localStorage` key `storybook.passport`，JSON shape 固定為 `{ version, completedStories: [{ source, slug }] }`，`source` 只允許 `dinosaurs` 與 `aquarium`。伺服器端繼續以既有 `StoryBook/Data/dinosaurs.json` 與 `StoryBook/Data/aquarium.json` 作為唯讀故事來源；本功能不新增 `CardPicker2/data/cards.json`、資料庫、cookie endpoint、server-side session、檔案寫入或外部服務。

**Storage Governance**: `storybook.passport` 是非敏感、同瀏覽器、可清除的使用者狀態，不保存姓名、年齡、班級、學校、自由輸入文字、閱讀時間線、標題快照或徽章快照。瀏覽器資料缺漏、格式錯誤、版本不支援、來源不允許、slug 不存在或重複時，`passport.js` 忽略無效資料，頁面顯示友善提示，並在下一次成功保存或清除時覆寫為有效資料。清除動作只移除或重設 `storybook.passport`，不得碰 `storybook.language`、`storybook.theme` 或其他 storage key。

**Testing**: `dotnet test StoryBook2.sln`。新增 xUnit 單元測試覆蓋護照 catalog projection、來源總數、來源排序、badge milestone 定義、source/slug resolution、invalid source 過濾、雙語 fallback 與 source failure friendly status；新增 script contract tests 覆蓋 `storybook.passport`、版本欄位、去重、clear scope、禁止 cookie/session/history/fetch；新增整合測試覆蓋 `/passport` route、首頁/共用導覽入口、恐龍/水族館詳情完成控制、HTML data contract、theme selector absence、friendly empty/error states 與一般 anchor link。瀏覽器 localStorage 成功/失敗、清除確認、鍵盤、語言、主題與 375/768/1366px 版面依 quickstart 手動驗收。

**Target Platform**: 單一 ASP.NET Core web app；支援 Chrome、Firefox、Safari、Edge 的桌面與行動瀏覽器。開發 URL 以 `dotnet run --project StoryBook/StoryBook.csproj` 輸出為準。

**Project Type**: Razor Pages web application + xUnit test project。

**Performance Goals**: `/passport` PageModel 與 catalog projection p95 目標低於 200ms；主要內容 FCP 目標低於 1.5 秒、LCP 目標低於 2.5 秒；完成標記、清除、徽章更新與已讀清單更新互動回應低於 1 秒。資料量以數十到數百筆故事為設計範圍，初始有效總數為 23 筆故事朋友。

**Performance Verification**: Phase tasks 必須在完整 catalog 狀態下，以 quickstart 或同等瀏覽器任務紀錄驗收 `/passport`、`/dinosaurs/triceratops`、`/aquarium/sea-turtle` warm-load 與互動回應；每個 route 至少 3 次，記錄是否在 1 秒內出現主要可操作內容。自動化測試可驗證 route/HTML contract；瀏覽器渲染時間以手動或 browser automation 紀錄補足。

**Constraints**: 維持單一 ASP.NET Core Razor Pages app，不改 SPA、Blazor、MVC 或 Web API。完成閱讀必須由使用者明確按下控制，不得由載入、捲動、停留時間或開啟圖片自動標記。所有護照故事連結使用一般 anchor 指向 `/dinosaurs/{slug}` 或 `/aquarium/{slug}`。`/passport` 套用既有有效主題但不得出現 theme selector。新增 JavaScript 放在 `StoryBook/wwwroot/js/passport.js`，不改寫 history、不使用 fetch/XMLHttpRequest、不使用 cookie/sessionStorage、不依賴 jQuery。新增 CSS 放在 `StoryBook/wwwroot/css/passport.css` 並沿用 site theme tokens；所有操作目標至少 44x44 CSS px、具 accessible name、可鍵盤操作且焦點可見。正式環境維持 HTTPS/HSTS/CSP 方向；本功能不新增 inline script 或外部資源以避免擴大 CSP 例外。

**Scale/Scope**: 首次整合既有恐龍 8 筆與水族館 15 筆故事，共 23 筆可完成項目；固定徽章 5 個：讀完第一篇、讀完 3 篇、讀完全部恐龍、讀完全部水族館、讀完全部故事。範圍包含 `/passport`、共用導覽入口、首頁入口、詳情頁完成控制、已讀數/總數、已讀清單、固定徽章、清除確認、localStorage 降級提示、雙語與主題一致性。不包含登入、跨裝置同步、伺服器端學習紀錄、排行榜、分享、推薦、後台管理、外部內容、即時翻譯或新故事來源。

**Observability/Logging**: 伺服器端只針對 catalog source 載入失敗、projection 異常或 unknown source metadata 以 `ILogger<PassportCatalogService>` 記錄非敏感摘要，例如 source code、reason code 與 count；不得記錄 localStorage payload、使用者閱讀狀態、檔案絕對路徑、exception detail、stack trace、secret、token 或個資。瀏覽器 storage 讀寫失敗只以使用者可見友善文字降級，不上傳或轉存。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. 程式碼品質至上 | PASS | 維持單一 Razor Pages app；新增 `PassportCatalogService`、projection DTO、badge metadata 與 `passport.js` localStorage adapter，PageModel 保持薄層；公開可重用 service/DTO/options 需加 XML 文件註解並包含 storage schema 邊界。 |
| II. 測試優先開發 | PASS | Tasks 必須先建立失敗單元/整合/script contract 測試，再實作 catalog projection、detail controls、`/passport` route、state validation、clear scope、fallback 與 accessibility contracts。 |
| III. Razor Pages 使用者體驗一致性 | PASS | 新頁面位於 Razor Pages，沿用 Tag Helpers、Bootstrap 5、共用 layout、`data-i18n-*` 雙語合約、既有 theme attributes、feature-specific CSS/JS 與一般 anchor 導覽。 |
| IV. 安全、設定與資料保護 | PASS | 護照只保存非敏感 `{source, slug}` 本機狀態；無登入、表單寫入後端、外部 API、secrets、跨裝置同步或 server storage；清除只碰 `storybook.passport`。 |
| V. 可觀察性、效能與營運準備 | PASS | 伺服器只記錄 source/projection 非敏感摘要；資料量小且 catalog projection 可由 singleton service 快取/組合，符合 p95 < 200ms 目標；不新增背景流程或營運依賴。 |

**Post-Design Re-check**: PASS。Phase 1 artifacts 維持既有技術棧、無新增外部依賴、無憲章例外；Complexity Tracking 無項目。

## Project Structure

### Documentation (this feature)

```text
specs/007-reading-passport/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── passport-state.schema.json
│   └── ui-routes.md
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit-tasks 產生；本命令不建立
```

### Source Code (repository root)

```text
StoryBook/
├── Models/
│   ├── PassportBadgeDefinition.cs
│   ├── PassportBadgeMilestone.cs
│   ├── PassportCatalogSnapshot.cs
│   ├── PassportSourceStatus.cs
│   └── PassportStoryItem.cs
├── Pages/
│   ├── Aquarium/
│   │   └── Details.cshtml        # 加入完成閱讀控制與 passport script
│   ├── Dinosaurs/
│   │   └── Details.cshtml        # 加入完成閱讀控制與 passport script
│   ├── Passport/
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   ├── Shared/
│   │   └── _Layout.cshtml        # 加入一般 anchor 護照入口
│   └── Index.cshtml              # 加入首頁護照 action
├── Services/
│   ├── PassportCatalogService.cs
│   └── PassportPreferenceService.cs
└── wwwroot/
    ├── css/
    │   └── passport.css
    └── js/
        └── passport.js

StoryBook.Tests/
├── Integration/
│   ├── PassportPageTestFixture.cs
│   └── PassportPagesTests.cs
└── Unit/
    ├── PassportCatalogServiceTests.cs
    ├── PassportPreferenceServiceTests.cs
    └── PassportScriptContractTests.cs
```

**Structure Decision**: 使用現有單一 ASP.NET Core Razor Pages 應用。`PassportCatalogService` 從既有 dinosaur/aquarium catalog service 組合有效故事清單、來源狀態、總數、來源排序與 badge 定義；`PassportPreferenceService` 只暴露 storage key、state version、allowed source codes 與 friendly metadata，不讀寫瀏覽器狀態。`Pages/Passport/Index` 伺服器端輸出所有可辨識故事的雙語 metadata 與 badge shell，`passport.js` 在瀏覽器讀寫 `storybook.passport`、去重、過濾、清除與更新 DOM。恐龍/水族館詳情頁只加入顯式完成控制與護照入口，不改既有故事內容、搜尋、modal、上一頁/下一頁、語言或主題流程。

## Complexity Tracking

No constitution violations or complexity exceptions.
