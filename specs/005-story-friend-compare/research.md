# Research: 內容比較器

## Decision 1: 維持單一 Razor Pages 頁面與比較專用 projection service

**決策**: `/compare` 使用 `StoryBook/Pages/Compare/Index.cshtml` 與 `Index.cshtml.cs`，由 `ComparisonCatalogService` 分別讀取既有恐龍與水族館 catalog，產生比較候選與比較欄位 projection。

**理由**: 專案憲章與現況要求維持單一 ASP.NET Core Razor Pages app；既有 catalog services 已負責 JSON 載入、驗證與快取。比較需要來源、名稱、食性、生活區域、生活時期、發現地點、摘要與詳情頁連結，比 `/explore` projection 多了來源適用性與不適用文字，因此使用比較專用 service 可保持 PageModel 薄層並讓規則可用 xUnit 測試。

**替代方案**:
- 直接重用 `ExplorationCatalogService`：可取得來源、摘要與部分 facets，但缺少比較欄位的精確值與不適用狀態，會把比較邏輯塞進頁面或 JavaScript。
- 建立 Web API + 前端 fetch：對 23 筆本機資料過度設計，增加 contract、failure handling 與測試面。

## Decision 2: 沿用既有 source metadata，不新增來源基礎設施

**決策**: 比較器沿用既有 `ExplorationSourceType` 的 source code、route prefix、source label 與 source order；新增比較 DTO 時只保留比較語意，不重命名或大幅重構現有探索模型。

**理由**: 來源類型、排序與 route prefix 已在 `/explore` 中穩定使用，且比較器規格同樣要求先恐龍後水族館、詳情 route 指向 `/dinosaurs/{slug}` 與 `/aquarium/{slug}`。直接重用可避免同義 enum 分裂與不必要 refactor。

**替代方案**:
- 新增 `StorySourceType` 並修改探索頁一起使用：命名更通用，但會擴大變更範圍並增加回歸風險。
- 比較器使用字串常數：短期簡單，但 route、label 與排序規則容易與探索頁漂移。

## Decision 3: 伺服器端輸出候選 metadata，瀏覽器端只管理頁面內選擇

**決策**: 初始 GET `/compare` 由 Razor 輸出所有可用候選、雙語欄位值、not-applicable 文案與 detail href。`compare.js` 使用原生 JavaScript 讀取候選 metadata，處理第一位/第二位選擇、重複選擇提示、清除與比較表顯示；不呼叫 API，不寫入 URL、history 或跨頁儲存。

**理由**: 規格要求比較狀態只存在目前頁面生命週期，重新載入回到起始狀態。資料量小，全部 metadata 可一次輸出；頁面內 DOM 更新能在 1 秒內完成，且詳情頁連結可保持一般 anchor 導覽。

**替代方案**:
- 用 GET query 或 form post 表示兩個選擇：會讓比較狀態進入 URL/history，違反 FR-023。
- 使用 localStorage 保存選擇：會跨頁/重載保留比較狀態，違反規格。

## Decision 4: 比較欄位使用明確欄位定義與兒童友善不適用文字

**決策**: 比較表欄位固定為來源、名稱、食性、生活區域、生活時期、發現地點、摘要與詳情連結。每個欄位有雙語 label；來源不適用的欄位以雙語 not-applicable text 顯示，例如恐龍沒有生活區域、水族館朋友沒有生活時期。

**理由**: 固定欄位能避免資訊過載，也符合規格「每列只比較一個欄位」。明確欄位定義比在 Razor 或 JavaScript 中散落條件更容易測試，並能保證不出現空白、`null`、`undefined` 或內部欄位名稱。

**替代方案**:
- 只顯示兩個來源都有的欄位：會漏掉恐龍生活時期與水族館生活區域，降低比較價值。
- 動態產生任意欄位：彈性較高，但對 5-10 歲使用者容易資訊過載，測試 contract 也較不穩定。

## Decision 5: Partial source failure 採可用候選優先

**決策**: `ComparisonCatalogService` 分別嘗試讀取恐龍與水族館來源。單一來源失敗時記錄 warning/error 並回傳可用候選與 partial failure status；全部來源失敗時顯示友善錯誤狀態與回首頁動作；可用候選少於兩筆時顯示比較器暫時不能開始的友善狀態。

**理由**: 規格要求部分來源不可用時仍讓使用者比較可用資料；同時若候選不足兩筆，核心比較流程不可用，必須明確說明而不是呈現空表。

**替代方案**:
- 任一來源失敗就讓 `/compare` 失敗：較容易實作，但違反 partial availability 需求。
- 靜默忽略失敗來源：使用者會誤以為沒有該類故事朋友，營運上也不容易診斷。

## Decision 6: 不新增外部依賴與資料儲存

**決策**: 不新增 NuGet、JavaScript 套件、資料庫、搜尋引擎、外部百科 API、即時翻譯服務或使用者狀態儲存。

**理由**: 比較器只整合既有本機 catalog。ASP.NET Core Razor Pages、Bootstrap、xUnit、`Microsoft.AspNetCore.Mvc.Testing` 與原生 JavaScript 足以完成；避免額外依賴可降低維護、安全與規格漂移風險。

**替代方案**:
- 前端狀態管理或 client framework：對兩個 select 控制與小型比較表過度。
- 資料庫或 server-side session：違反不保存比較狀態與不新增資料儲存的範圍邊界。
