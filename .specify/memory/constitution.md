<!--
Sync Impact Report
Version change: template placeholder -> 1.0.0
Modified principles:
- Template principle 1 -> I. 程式碼品質至上 (NON-NEGOTIABLE)
- Template principle 2 -> II. 測試優先開發 (NON-NEGOTIABLE)
- Template principle 3 -> III. Razor Pages 使用者體驗一致性
- Template principle 4 -> IV. 安全、設定與資料保護
- Template principle 5 -> V. 可觀察性、效能與營運準備
Added sections:
- 技術標準
- 開發工作流程
Removed sections:
- None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md
- ✅ .specify/templates/spec-template.md
- ✅ .specify/templates/tasks-template.md
- ✅ .specify/templates/checklist-template.md (reviewed; no constitution-specific changes required)
- ✅ .specify/extensions/git/commands/*.md (reviewed; no agent-specific references requiring updates)
Follow-up TODOs:
- None
-->

# StoryBook2 Constitution

## Core Principles

### I. 程式碼品質至上 (NON-NEGOTIABLE)

所有 C#、Razor、JavaScript、CSS 與設定檔變更都必須保持可維護、可審查、可回溯。
C# 程式碼必須遵守 `.editorconfig`、啟用 nullable 的語意、明確命名、清楚的服務邊界，
並優先使用 ASP.NET Core 與 .NET 內建能力。公開且會被其他功能重用的 API、服務、
DTO、Options、Tag Helper 或整合介面必須具備 XML 文件註解；當行為不直觀時，文件必須
包含範例或邊界條件。複雜設計必須在 feature plan 或相鄰文件記錄取捨理由。

理由：StoryBook2 是 ASP.NET Core 專案，後續功能會累積在同一個 Razor Pages 應用中。
清楚的邊界、可讀的程式碼與一致格式能降低頁面、服務與資料處理之間的耦合。

### II. 測試優先開發 (NON-NEGOTIABLE)

每個功能切片必須先定義可驗證行為，再實作。涉及業務規則、資料轉換、資料存取、
安全流程、PageModel handler 或跨頁使用者流程的變更，必須先新增或更新失敗測試，
再完成實作並使測試通過。單元測試覆蓋純服務與規則；`WebApplicationFactory<Program>`
整合測試覆蓋 DI、路由、Razor Pages handler、表單提交、驗證、授權與錯誤處理；
重要瀏覽器互動必須以手動驗收步驟或瀏覽器自動化驗證。

理由：測試先行把需求轉成可執行契約，讓每個使用者故事可以獨立交付、獨立驗證，
並為後續重構提供安全網。

### III. Razor Pages 使用者體驗一致性

使用者可見功能必須以頁面導向流程設計，優先沿用 Razor Pages、Tag Helpers、model
binding、validation summary、Bootstrap 5 與共用 layout。頁面必須支援繁體中文
使用情境、回應式版面、清楚的表單驗證與可操作錯誤訊息。新 UI 必須避免與現有 layout
衝突，並滿足基本 WCAG 2.1 AA 可及性要求，包括語意化標記、鍵盤可操作性、表單 label
與足夠對比。

理由：一致的頁面行為能降低使用成本，也讓 Razor Pages 的 route、PageModel 與視圖結構
保持可預測。

### IV. 安全、設定與資料保護

所有使用者輸入、路由值、表單欄位與外部資料都必須驗證。寫入型表單必須使用 ASP.NET
Core 內建 antiforgery 保護；需要身分或權限的頁面必須明確宣告授權規則，不得依賴 UI
隱藏作為安全控制。機密、連線字串、API key 與憑證不得提交到 repository；開發環境使用
Secret Manager 或環境變數，正式環境使用受控的安全儲存。正式環境必須維持 HTTPS、
HSTS 與集中式錯誤處理，不得向使用者暴露內部例外細節。

理由：StoryBook2 可能處理使用者內容與故事資料；安全與設定錯誤會直接造成資料外洩、
錯誤授權或不可預期的正式環境行為。

### V. 可觀察性、效能與營運準備

所有關鍵流程必須具備結構化 logging，並使用 `ILogger<T>` 記錄可關聯的事件、錯誤與
決策結果；不得記錄密碼、token、secret 或敏感內容。新增外部依賴、資料儲存或背景流程時，
必須定義健康檢查或可驗證的營運訊號。效能優化必須先量測，再優先使用 ASP.NET Core
內建能力，例如 static assets、output caching、response compression、rate limiting、
request timeout 與分頁或串流。

理由：可觀察性讓問題能被定位與修復；效能規則防止在小型功能中引入難以診斷的阻塞、
大量 payload 或過度自製基礎設施。

## 技術標準

StoryBook2 必須以目前 repository 的單一 ASP.NET Core Razor Pages 應用為基準：
`StoryBook2.sln`、`StoryBook/StoryBook.csproj`、`net10.0`、nullable enabled、
implicit usings enabled。除非 feature plan 記錄替代理由，新的頁面功能必須放在
`StoryBook/Pages/` 或清楚命名的功能資料夾，靜態資產放在 `StoryBook/wwwroot/`，
跨頁邏輯放在 injectable services，設定使用 Options pattern 與 `appsettings.*.json`。

資料儲存可依功能需求選擇 JSON 文件、檔案或資料庫，但 plan 必須描述資料模型、備份/遷移
策略、並發處理與驗證規則。新增 NuGet 套件、JavaScript 套件或外部服務前，必須先確認
ASP.NET Core 或 .NET 內建能力不足，並在 plan 中記錄選擇理由與替代方案。

Spec Kit 文件、使用者故事、驗收情境、研究結論、任務清單與使用者可見 API 文件必須使用
繁體中文（zh-TW）。程式碼識別字維持英文；註解可使用英文或繁體中文，但必須服務於理解
複雜邏輯，不得重述語法。

## 開發工作流程

所有功能必須從可獨立交付的使用者故事開始，並依 P1、P2、P3 排序。每個故事必須定義：
目標使用者、可獨立驗證的行為、成功標準、邊界情況、安全/資料影響、測試策略與回滾方式。
`plan.md` 的 Constitution Check 必須在研究前通過，設計完成後必須重新檢查；違反憲章的
設計只能在 Complexity Tracking 中以具體理由、替代方案與補救措施記錄。

任務清單必須先列測試與驗收任務，再列實作任務。跨故事共享的基礎建設必須集中在
Foundational phase，且不得讓 P1 故事依賴尚未交付的低優先級故事。完成每個故事後必須
執行對應測試、快速入門驗證或手動驗收步驟，確認該故事能獨立示範。

Pull Request 或交付前必須完成以下品質閘門：建置成功、相關測試通過、無新增編譯警告、
無未處理 placeholder、無 secrets、使用者可見文字符合 zh-TW、重要流程具備 logging，
且變更內容已同步更新對應 Spec Kit 文件。

## Governance

本憲章優先於其他開發慣例、模板說明與個人偏好。當憲章與 feature spec、plan、tasks
或 runtime guidance 發生衝突時，必須先修正較低層文件；若憲章本身需要調整，必須透過
獨立憲章修訂完成，不得在功能實作中默默繞過。

修訂憲章必須包含修訂理由、影響範圍、遷移計畫、版本變更理由與模板同步結果。版本規則
採語意化版本：MAJOR 用於移除或重新定義核心原則與不相容治理變更；MINOR 用於新增原則、
新增章節或實質擴充規範；PATCH 用於文字澄清、錯字修正與不改變治理語意的細化。

所有 spec、plan、tasks 與 PR review 必須檢查憲章合規性。`/speckit-analyze` 發現的憲章
違規視為 critical，必須修正 spec、plan 或 tasks；若功能必須例外，必須在 Complexity
Tracking 中記錄具體風險、替代方案、補償控制與審查者。

**Version**: 1.0.0 | **Ratified**: 2026-05-09 | **Last Amended**: 2026-05-09
