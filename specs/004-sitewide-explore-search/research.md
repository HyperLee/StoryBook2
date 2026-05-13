# Research: 全站探索與分類搜尋

## Decision 1: 維持單一 Razor Pages 頁面與 injectable services

**決策**: `/explore` 使用 `StoryBook/Pages/Explore/Index.cshtml` 與 `Index.cshtml.cs`，由 `ExplorationCatalogService` 組合既有 catalog services，並由 `ExplorationSearchService` 定義搜尋正規化、分類合併與穩定排序規則。

**理由**: 專案現況與憲章要求單一 ASP.NET Core Razor Pages app；既有恐龍與水族館內容已由 service 載入、驗證與快取。新增探索 service 可保持 PageModel 薄層，也讓搜尋/分類規則可用 xUnit 測試，不需要建立 API、SPA 或資料庫。

**替代方案**:
- 建立 Web API + 前端 fetch：增加 contract、錯誤處理與測試面，對 23 筆本機 catalog 過度設計。
- 直接在 Razor PageModel 內組合所有資料：短期可行，但會把 projection、錯誤處理與搜尋規則塞進頁面層，違反薄 PageModel 準則。

## Decision 2: 伺服器端輸出完整 projection，瀏覽器端只做頁面內篩選

**決策**: 初始 GET `/explore` 由 Razor 輸出所有可用探索結果、雙語文字、search text 與 facet metadata。`explore.js` 使用原生 JavaScript 讀取 data attributes，做正規化搜尋、每群組單選 filter、AND 合併、清除與提示顯示；不呼叫 API，不寫入 URL、history 或跨頁儲存。

**理由**: 規格要求搜尋/篩選變更 1 秒內更新且狀態不需要跨頁保存。初始資料量小，全部輸出可避免 API latency 與 partial hydration 問題；一般 anchor 詳情連結也保留瀏覽器 history。

**替代方案**:
- 每次輸入向 server 查詢：增加 endpoint、debounce、錯誤狀態與網路 failure handling，不符合本機小資料集。
- URL query 保存搜尋狀態：已由 clarification 排除，避免 history churn 與分享狀態需求。

## Decision 3: 搜尋索引同時包含 `zh-TW` 與 `en`

**決策**: 每筆 `ExplorationItem` 產生雙語 `SearchText`，包含名稱、來源類型、分類、食性、生活環境、生活時期、發現地點、摘要與 curated keywords。搜尋比對不受目前 UI 語言限制；結果顯示仍依目前語言，缺漏回退繁體中文。

**理由**: 使用者可能在繁體中文頁面輸入英文名稱，也可能在英文頁面輸入中文名稱。雙語索引可降低找不到內容的機率，且現有 `DinosaurText` / `AquariumText` 已有 fallback 行為。

**替代方案**:
- 只搜尋目前 UI 語言：測試與 UX 容易出現「英文資料存在但中文模式找不到 shark」的落差。
- 只搜尋 language-neutral slug/keywords：會漏掉摘要、地點與分類中的自然語言查詢。

## Decision 4: Facet 使用群組內單選、群組間 AND

**決策**: 篩選條件分為來源、食性、生活區域、生活時期與發現地點等群組；每個群組一次只能選一個有效值，不同群組共同縮小結果集合。

**理由**: 這符合 5-10 歲孩童點選探索的心智模型，也讓結果規則容易說明與測試。群組間 AND 能支援「水族館 + 海水」或「恐龍 + 草食 + 白堊紀」這類精煉探索。

**替代方案**:
- 全域一次只能選一個 filter：實作簡單但不能組合來源與屬性。
- 同群組多選 OR：彈性較高但控制狀態與提示更複雜，對本階段不是必要需求。

## Decision 5: Partial source failure 採可用內容優先

**決策**: `ExplorationCatalogService` 分別嘗試讀取恐龍與水族館來源。單一來源失敗時記錄 warning/error 並回傳可用來源內容與 partial failure status；全部來源失敗時頁面顯示友善錯誤狀態與回首頁動作。

**理由**: 規格要求部分來源不可用時不得空白；既有 aquarium 頁已有 load failure UX 模式，可延伸到全站探索。這也讓一個 catalog 問題不會阻斷另一個故事書。

**替代方案**:
- 任一來源失敗就讓 `/explore` 失敗：較容易實作，但違反 FR-021。
- 靜默忽略失敗來源：使用者會誤以為沒有該類內容，且營運上無法診斷。

## Decision 6: 不新增外部依賴與資料儲存

**決策**: 不新增 NuGet、JavaScript 套件、資料庫、搜尋引擎、外部百科 API 或即時翻譯服務。

**理由**: 需求只整合現有本機 catalog；.NET/ASP.NET Core、Razor、Bootstrap 與原生 JavaScript 足以完成。避免額外依賴可降低維護與安全風險。

**替代方案**:
- Fuse.js 或其他 client search library：對 23 筆內容過度，且需要新增套件理由與供應鏈風險。
- SQLite/full-text search：目前資料量與查詢需求不需要。
