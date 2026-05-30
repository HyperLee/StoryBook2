# Implementation Plan: 互動問答挑戰

**Branch**: `010-quiz-challenges` | **Date**: 2026-05-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/008-quiz-challenges/spec.md`

**Note**: This plan is filled by the `/speckit-plan` workflow and stops after Phase 1 design artifacts.

## Summary

新增唯一 canonical route `/quiz`，讓使用者從首頁或全站探索入口進入問答挑戰，選擇全部故事、恐龍或水族館範圍，作答單題選擇題，得到友善正誤回饋、解釋與相關故事複習連結。技術上維持目前 `StoryBook2.sln` 的單一 ASP.NET Core Razor Pages app、`net10.0`、Bootstrap 5、`System.Text.Json`、既有 catalog services、既有語言/主題合約與 xUnit 測試；題庫使用 repo 內人工維護 JSON `StoryBook/Data/quiz-questions.json`，不沿用 CardPicker2 `cards.json`、不新增資料庫、CRUD 後台、外部題庫、即時翻譯、Web API、Serilog、Moq 或 jQuery 依賴。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0，target framework `net10.0`，ASP.NET Core Razor Pages、nullable enabled、implicit usings enabled。瀏覽器端若需要小型漸進增強，使用原生 JavaScript；問答核心可在 Razor Pages handler 中完成。

**Primary Dependencies**: ASP.NET Core Razor Pages、Tag Helpers、Bootstrap 5、`System.Text.Json`、Options pattern、既有 `DinosaurCatalogService` / `AquariumCatalogService`、既有 `ExplorationSourceType` source metadata、`LanguagePreferenceService`、`ThemePreferenceService`、xUnit、`Microsoft.AspNetCore.Mvc.Testing`。現有 jQuery 與 jQuery Validation 可保留給範本/既有驗證資產，但問答功能不依賴 jQuery。此功能不新增 Serilog/Moq/NuGet/JavaScript 套件；logging 沿用 ASP.NET Core `ILogger<T>`，測試優先使用現有 fixture、fake environment、recording logger 與可替換服務。

**Storage**: 題庫是單一本機 JSON 文字檔，repo 位置為 `StoryBook/Data/quiz-questions.json`，執行期預設路徑為 `{ContentRootPath}/Data/quiz-questions.json`，透過 `QuizCatalogOptions.ContentPath` 設定。檔案只由專案維護者在 repo 中人工編輯與審核；runtime 不提供 CRUD UI、不寫入 JSON、不使用 `CardPicker2/data/cards.json`、不使用資料庫、cookie、session、localStorage 作為答題結果保存，也不呼叫外部 API。

**Testing**: `dotnet test StoryBook2.sln`。新增/更新 xUnit 單元測試覆蓋 `QuizContentValidator` schema/content rules、`QuizCatalogService` JSON 載入/cache/filter/sort/source failure、story reference resolution、language fallback、question scope selection、answer evaluation 與 next-question cycling。新增整合測試覆蓋 `/quiz` route、DI、Razor Pages handler、antiforgery form submit、no-selection friendly validation、scope links、related story anchors、theme selector absence、friendly empty/error states 與首頁/探索入口。若需要替換服務驗證錯誤情境，使用 `WebApplicationFactory<Program>` / TestServer service replacement；Moq 不列為預設相依性。

**Target Platform**: 單一 ASP.NET Core web app；支援 Chrome、Firefox、Safari、Edge 的桌面與行動瀏覽器。開發 URL 以 `dotnet run --project StoryBook/StoryBook.csproj` 輸出為準。

**Project Type**: ASP.NET Core Razor Pages web application + xUnit test project。

**Performance Goals**: `/quiz` Page handler 與題庫 projection p95 低於 200ms；FCP 低於 1.5 秒；LCP 低於 2.5 秒；作答提交後正誤回饋與解釋在 1 秒內顯示；scope 切換與下一題導覽在 1 秒內回應。題庫以數十到數百題為設計範圍，MVP 完整資料至少 12 題有效題目，恐龍與水族館各至少 5 題。

**Constraints**: 維持單一 Razor Pages app，不改 SPA、Blazor、MVC 或獨立 Web API。唯一 canonical route 是 `/quiz`；題目 `source` 只允許 `dinosaurs` 或 `aquarium`，`all` 只作為 UI 聚合篩選。`difficulty` 只允許 `easy`、`medium`。MVP 不顯示累計分數、答對/答錯統計或題數進度，不保存作答狀態，不引入登入、排行榜、限時競賽、後台管理或使用者上傳內容。所有使用者可見文字使用繁體中文為預設並提供英文，缺漏或無效語言回退繁體中文。正式環境維持既有 HTTPS/HSTS 方向，問答新增資產必須相容嚴格 CSP：不新增外部 script/style、inline event handler 或外部資源。

**Scale/Scope**: 初始整合既有恐龍 8 筆與水族館 15 筆故事內容作為複習連結來源。功能範圍包含首頁或探索入口、`/quiz` 頁面、scope 選擇、單題顯示、2-4 個單選選項、提交、正誤/未選提示、解釋、下一題循環、相關故事連結、題庫完整性檢查、友善空/錯誤狀態、雙語、主題套用與可及性。不包含 CardPicker 的早餐/午餐/晚餐卡牌、抽卡、CRUD、搜尋卡牌或資料寫入流程。

**Observability/Logging**: 使用 `ILogger<QuizCatalogService>` / `ILogger<QuizContentValidator>` 記錄非敏感摘要，例如 reason code、question id、source code、valid/invalid counts。不得在使用者頁面或 logs 中暴露檔案絕對路徑、exception detail、stack trace、secret、token、connection string 或作答結果。此功能不需要 Serilog file sink；若未來全站營運要求 file logging，需另立 plan 記錄理由、資料保護與輪替策略。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. 程式碼品質至上 | PASS | 維持單一 Razor Pages app；新增 quiz model/service/validator/page/css/js 都按既有 feature folder pattern；PageModel 保持薄層，題庫載入、驗證、篩選、答題判定與下一題規則放在 injectable services；公開可重用 service/DTO/options 補 XML 文件註解與邊界範例。 |
| II. 測試優先開發 | PASS | Tasks 必須先建立失敗單元/整合/script contract 測試，再實作題庫 schema、content validation、scope 篩選、answer evaluation、next cycling、route/form/fallback/accessibility contracts。 |
| III. Razor Pages 使用者體驗一致性 | PASS | `/quiz` 使用 Razor Pages、Tag Helpers、model binding、antiforgery form、Bootstrap 5、共用 layout、既有語言/主題 data contract、一般 anchor 導覽與 feature-specific CSS/JS。 |
| IV. 安全、設定與資料保護 | PASS | 題庫唯讀且人工維護；作答結果只存在目前 response/view state，不進 URL、localStorage、cookie、session、server file 或外部服務；錯誤訊息友善且不暴露內部細節。 |
| V. 可觀察性、效能與營運準備 | PASS | 使用 `ILogger<T>` 記錄非敏感題庫載入與驗證摘要；資料量小且可 lazy cache；不新增外部依賴、背景工作、資料庫或 file logging，符合 p95 < 200ms 目標。 |

**Post-Design Re-check**: PASS。Phase 1 artifacts 維持既有技術棧、無新增外部依賴、無憲章例外；Complexity Tracking 無項目。

## Project Structure

### Documentation (this feature)

```text
specs/008-quiz-challenges/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── quiz-questions.schema.json
│   └── ui-routes.md
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit-tasks 產生；本命令不建立
```

### Source Code (repository root)

```text
StoryBook/
├── Data/
│   └── quiz-questions.json
├── Models/
│   ├── QuizAnswerOption.cs
│   ├── QuizAnswerResult.cs
│   ├── QuizCatalog.cs
│   ├── QuizCatalogSnapshot.cs
│   ├── QuizDifficulty.cs
│   ├── QuizQuestion.cs
│   ├── QuizQuestionView.cs
│   ├── QuizScope.cs
│   ├── QuizSourceStatus.cs
│   ├── QuizStoryReference.cs
│   └── QuizText.cs
├── Pages/
│   ├── Explore/
│   │   └── Index.cshtml          # 加入一般 anchor 問答入口
│   ├── Quiz/
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   ├── Shared/
│   │   └── _Layout.cshtml        # 如需全站入口，使用一般 anchor
│   └── Index.cshtml              # 加入首頁問答 action
├── Services/
│   ├── QuizCatalogOptions.cs
│   ├── QuizCatalogService.cs
│   ├── QuizContentValidationResult.cs
│   └── QuizContentValidator.cs
└── wwwroot/
    ├── css/
    │   └── quiz.css
    └── js/
        └── quiz.js               # 只做漸進增強，不保存作答狀態

StoryBook.Tests/
├── Integration/
│   ├── QuizPageTestFixture.cs
│   └── QuizPagesTests.cs
└── Unit/
    ├── QuizCatalogServiceTests.cs
    ├── QuizContentValidationTests.cs
    └── QuizScriptContractTests.cs
```

**Structure Decision**: 使用現有單一 ASP.NET Core Razor Pages 應用。`QuizCatalogService` 從 `StoryBook/Data/quiz-questions.json` 載入題庫，委派 `QuizContentValidator` 驗證題目欄位、選項、正確答案、difficulty、source 與 related story references，再透過既有 dinosaur/aquarium catalog services 解析複習連結。`Pages/Quiz/Index` 透過 `OnGet` 顯示 scope 與目前題目，透過 `OnPostAnswer` 驗證所選答案並回傳同頁 feedback；下一題使用一般 anchor 指向 `/quiz?scope={scope}&questionId={nextQuestionId}`。`quiz.js` 不承擔 correctness 判定，也不保存結果，只允許做焦點、快速重複提交防護或狀態文字漸進增強。

## Complexity Tracking

No constitution violations or complexity exceptions.
