# Research: 主題學習旅程

## Decision 1: 維持 Razor Pages 與旅程專用 projection service

**決策**: `/journeys` 使用 `StoryBook/Pages/Journeys/Index.cshtml`，`/journeys/{slug}` 使用 `StoryBook/Pages/Journeys/Details.cshtml`。旅程資料由 `LearningJourneyCatalogService` 讀取本機 JSON、驗證、解析既有恐龍/水族館故事項目，並輸出列表與詳情 projection。

**理由**: 專案憲章與現況要求維持單一 ASP.NET Core Razor Pages app。旅程需要跨來源引用、3-5 有效故事規則、直接開啟不可用旅程時的友善狀態，以及從既有故事內容衍生名稱/摘要；這些規則適合集中在 injectable service，讓 PageModel 保持薄層並可用 xUnit 測試。

**替代方案**:
- 建立 Web API + 前端 fetch：對小型本機內容過度設計，增加 failure handling、contract 與測試面。
- 在 Razor Page 直接讀 JSON：短期簡單，但驗證、來源解析、排序與 fallback 規則難以單元測試。

## Decision 2: 新增 `journeys.json`，不新增資料庫或 CMS

**決策**: 學習旅程使用 `StoryBook/Data/journeys.json`，透過 `LearningJourneyCatalogOptions.ContentPath` 設定路徑，並以 `learning-journeys.schema.json` 與 `LearningJourneyContentValidator` 定義資料合約。

**理由**: 旅程是專案維護者人工策展的只讀內容，規模為至少 3 條旅程、每條 3-5 筆既有故事引用。本機 JSON 與現有 catalog pattern 一致，足以支援驗證、版本控制與測試，不需要資料庫、後台管理或外部 CMS。

**替代方案**:
- 資料庫：增加 migration、備份、營運與安全成本，且本階段沒有寫入或查詢複雜度需求。
- 把旅程資料放入 `wwwroot`：會讓內容資料變成公開靜態路徑，違反既有資料目錄準則。

## Decision 3: 旅程故事項目只保存 source、slug、sortOrder

**決策**: `JourneyStoryReference` 只保存來源故事書 code、來源 slug 與旅程內排序；故事名稱、摘要、圖片替代文字與詳情頁 href 都由既有 `DinosaurCatalogService` / `AquariumCatalogService` 解析衍生。

**理由**: 規格明確要求旅程資料不重複保存顯示名稱與摘要。這能避免同一故事在原始 catalog 更新後旅程頁出現舊資料，也能保證詳情連結只指向既有 canonical routes。

**替代方案**:
- 在旅程 JSON 重複保存名稱與摘要：編輯方便，但容易與來源故事內容漂移，違反 FR-007。
- 旅程詳情建立自己的故事摘要頁：會新增重複詳情頁並破壞既有 `/dinosaurs/{slug}`、`/aquarium/{slug}` 導覽合約。

## Decision 4: 沿用既有 source metadata

**決策**: 來源 code、route prefix、label 與排序沿用既有 `ExplorationSourceType` extension：`dinosaurs` 與 `aquarium`。旅程 service 只在需要時新增 parsing/validation helper，不引入新的同義 source enum。

**理由**: `/explore` 與 `/compare` 已穩定使用同一來源 metadata，旅程也需要相同的詳情 route 與來源排序。重用可避免 source code、label 與 route prefix 漂移。

**替代方案**:
- 新增 `JourneySourceType`：命名更貼近旅程，但會造成同義 enum 並增加轉換與測試負擔。
- 使用自由字串：短期少檔案，但 schema、validator 與 route mapping 容易不一致。

## Decision 5: 列表只顯示完整旅程，詳情保留友善不可用狀態

**決策**: `LearningJourneyCatalogService` 將有效故事項目少於 3 筆或超過 5 筆的旅程標示為 unavailable，從 `/journeys` 可選列表排除；直接開啟 `/journeys/{slug}` 時顯示兒童友善不可用提示與返回動作，不產生失效連結。

**理由**: 規格要求列表只能呈現完整可出發旅程，但直接 route 需要可理解狀態。此方式保留 canonical URL 行為，也讓維護者可透過 sanitized diagnostic summary 找出資料問題。

**替代方案**:
- 列表顯示所有旅程並停用卡片：會增加兒童使用者困惑，且不符合 FR-028。
- 直接回傳空白或技術錯誤：違反友善狀態與資料保護要求。

## Decision 6: Partial source failure 採可用內容優先

**決策**: 恐龍或水族館其中一個來源失敗時，旅程 service 仍解析其他可用來源；完整旅程依可用有效項目重新評估。若沒有任何可出發旅程，`/journeys` 顯示友善錯誤狀態與回首頁 action。

**理由**: 規格要求部分來源暫時不可用時仍顯示可用旅程內容，且不得產生失效連結。逐來源解析與 journey availability status 能保留可用內容並清楚呈現限制。

**替代方案**:
- 任一來源失敗就讓旅程頁整體失敗：實作較簡單，但違反 partial availability。
- 靜默忽略來源失敗：使用者會誤以為旅程較少，維護者也缺少診斷訊號。

## Decision 7: 雙語與主題沿用既有頁面合約

**決策**: Razor 預設輸出繁體中文，並在使用者可見文字加上 `data-i18n-zh-tw` / `data-i18n-en`。`journeys.js` 只套用 `storybook.language` 與語言按鈕狀態；主題由既有 layout boot script、`theme.js` 與 CSS tokens 生效，旅程頁不新增 theme selector。

**理由**: 既有恐龍、水族館、探索與比較頁已使用相同語言 data attribute pattern。旅程只需要跟隨共享偏好，不需要新偏好儲存或伺服器端語言狀態。

**替代方案**:
- 使用 query string 或 cookie 表示語言：會偏離既有 `localStorage` contract。
- 新增每頁主題控制項：違反 dark mode 規格與 FR-015。

## Decision 8: 診斷紀錄使用非敏感摘要

**決策**: 旅程資料驗證失敗、來源解析失敗、旅程不可用與未知 slug 只記錄 source code、journey slug、reason code、count 等非敏感摘要；旅程功能新增 log 不傳入 exception object，不包含 resolved file path、stack trace、secret、個資或使用者閱讀狀態。

**理由**: 規格明確要求維護者可診斷，但不得暴露檔案路徑、例外細節、secret 或個資。reason code 與 slug 足以定位內容資料問題，詳細檔案路徑不應出現在 journey-level log。

**替代方案**:
- 記錄完整 exception：容易洩漏 path、stack trace 或內部設定。
- 完全不記錄：維護者無法知道旅程為何被隱藏或不可用。

## Decision 9: 不新增外部依賴與狀態儲存

**決策**: 不新增 NuGet 套件、JavaScript 套件、資料庫、搜尋引擎、外部百科 API、即時翻譯服務、server-side session、cookie 或 localStorage 旅程狀態。

**理由**: 旅程是只讀、站內、策展導覽功能。ASP.NET Core Razor Pages、Bootstrap、`System.Text.Json`、原生 JavaScript、xUnit 與 `Microsoft.AspNetCore.Mvc.Testing` 已足夠；避免額外依賴可降低維護與安全成本。

**替代方案**:
- SPA/client framework：對列表、詳情與 anchor 導覽過度設計。
- 儲存閱讀進度：屬於後續閱讀護照功能，會違反 FR-027 與 SC-009。
