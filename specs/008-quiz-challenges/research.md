# Research: 互動問答挑戰

## Decision 1: 維持 StoryBook2 Razor Pages 技術棧，不沿用 CardPicker2 app/data layout

**決策**: 問答挑戰實作在現有 `StoryBook2.sln` 的單一 ASP.NET Core Razor Pages app 中，使用 `StoryBook/Pages/Quiz/Index.cshtml`、既有恐龍/水族館 catalog services、Bootstrap 5、`System.Text.Json`、原生 JavaScript 漸進增強與 xUnit 測試。使用者提供的 CardPicker2 `data/cards.json`、早餐/午餐/晚餐卡牌、CRUD、搜尋與抽卡流程不納入本 feature。

**理由**: 目前 spec 是站內故事複習問答，不是新的抽卡 CRUD app。專案憲章與 AGENTS 指引要求維持單一 Razor Pages web application，內容資料放在 `StoryBook/Data/`，並優先使用既有 ASP.NET Core 與專案服務模式。

**替代方案**:
- 建立 CardPicker2-style app 或 `CardPicker2/data/cards.json`: 會引入不相干資料模型與目錄結構。
- 建立新 SPA/Blazor/MVC surface: 對單頁問答過度設計，且違反既有 app model 約束。
- 新增 CRUD 後台管理題庫: spec 明確排除後台管理，題目由維護者人工編輯與審核。

## Decision 2: 題庫使用 `StoryBook/Data/quiz-questions.json`

**決策**: 題庫為 repo 內單一本機 JSON 檔 `StoryBook/Data/quiz-questions.json`，透過 `QuizCatalogOptions.ContentPath` 預設解析為 `{ContentRootPath}/Data/quiz-questions.json`。runtime 唯讀，題庫變更透過一般程式碼審查與測試。

**理由**: 既有 dinosaur/aquarium/journeys 都使用 `StoryBook/Data/` 下的 JSON catalog 與 options pattern。問答題庫是人工維護內容，資料量為數十到數百題，本機 JSON 足夠且便於 review、測試與回滾。

**替代方案**:
- 資料庫: MVP 無多人寫入、查詢複雜度或交易需求，會增加部署與備份成本。
- 外部 CMS/百科 API/即時翻譯: spec 明確禁止使用外部題庫與自動生成內容作為使用者可見題目來源。
- `wwwroot` 公開 JSON: 題庫應由伺服器驗證後 projection，不直接公開原始 correctness 資料。

## Decision 3: 不新增 Serilog、Moq 或 jQuery 依賴

**決策**: 問答功能使用 ASP.NET Core 內建 `ILogger<T>` 與現有測試支援。Moq 與 Serilog 不加入 project files；jQuery 仍保留給既有範本/驗證資產，但問答新互動不依賴 jQuery。

**理由**: 目前 repository 沒有 Serilog 或 Moq package，憲章要求新增依賴前先確認內建能力不足。問答只需要記錄題庫載入/驗證摘要，`ILogger<T>` 足夠；整合測試可透過 `WebApplicationFactory<Program>` 替換服務，單元測試可使用真實服務、臨時資料路徑與現有 fake/recording helper。

**替代方案**:
- Serilog console/file sink: 適合全站營運 logging 政策，不是單一問答 feature 的必要條件，且 file sink 會帶來權限、輪替與敏感資料審查。
- Moq: 可簡化部分 mock，但會增加依賴；現有專案測試風格已能覆蓋錯誤與 fallback。
- jQuery: 對焦點、按鈕防重複提交與狀態文字更新不是必要。

## Decision 4: 使用 Razor Pages form handler，不新增 Web API

**決策**: `/quiz` 使用 `OnGet` 顯示 scope 與題目，使用 `OnPostAnswer` 處理答案提交。答案選項使用一般 HTML form、radio input、button 與 antiforgery token；提交後同頁顯示正誤、友善回饋、解釋、下一題 link 與相關故事 link。

**理由**: 問答是頁面導向互動，Razor Pages handler、model binding 與 validation summary 是現有專案最自然的形狀。Server-side evaluation 避免在初始 DOM 中暴露正確答案，也不需要 API、fetch 或 client-only router。

**替代方案**:
- Minimal API 或 controller API: 對單頁 form 過度設計，增加 API contract、ProblemDetails 與 client error handling 面。
- 完全 client-side 判定: 會把 correctness 資料送到瀏覽器，且更難測試 PageModel handler 與 no-selection fallback。
- query string 保存答案: 會讓作答結果進入 URL/history，不符合不保存/不分享答題結果。

## Decision 5: 嚴格題庫驗證並只顯示有效題目

**決策**: `QuizContentValidator` 驗證根物件、唯一 question id、source、difficulty、題幹、2-4 個選項、唯一 correct option、雙語回饋、雙語解釋、related story references 與 kebab-case slug。`QuizCatalogService` 再確認 related stories 存在於既有 dinosaur/aquarium catalog，無效題目不提供給 UI。

**理由**: FR-018、FR-019 與錯誤學習風險要求「寧可不顯示無效題目，也不要顯示錯誤題目」。將 schema/content/story-reference 驗證集中在 service 層，PageModel 可保持薄層且容易單元測試。

**替代方案**:
- View 層即時判斷缺漏: 會分散規則並容易造成空白題目/失效連結。
- 有錯仍顯示並標記 warning: 對 5-10 歲使用者不友善，且可能造成錯誤學習。

## Decision 6: Scope selection 使用 `all` 聚合，題目資料只保存 `dinosaurs` 或 `aquarium`

**決策**: 題目 JSON 的 `source` 僅允許 `dinosaurs` 或 `aquarium`。UI scope 使用 `all`、`dinosaurs`、`aquarium`；`all` 只聚合所有有效題目，不作為題目資料值。題目排序依 source sort order、question `sortOrder`、question id 穩定排序。

**理由**: Clarifications 已固定來源資料只能是兩個故事書，`全部故事` 是 UI 聚合篩選。穩定排序讓下一題與循環行為可預測，也避免個人化或追蹤。

**替代方案**:
- 在資料中保存 `all`: 會讓 scope 語意混亂並違反 clarification。
- 隨機抽題: spec 要求題目順序穩定且可預測，不使用外部服務或個人化。

## Decision 7: 下一題以同範圍循環規則計算

**決策**: `QuizCatalogService` 提供目前 scope 中的有效題目序列與 next question id。若目前題目是最後一題，下一題回到同範圍第一題。下一題動作輸出一般 anchor，例如 `/quiz?scope=dinosaurs&questionId=...`。

**理由**: Clarification 指定最後一題後循環回第一題。一般 anchor 保留 browser history/open-new-tab 行為，也符合專案既有導覽規則。

**替代方案**:
- 將題號進度保存到 localStorage/session: MVP 不保存作答或進度。
- JavaScript-only next route: 不符合一般 anchor 導覽約束，且降低 no-JS fallback。

## Decision 8: 雙語、主題與 accessibility 沿用既有合約

**決策**: 題庫與 UI metadata 都提供 `zhTW` 與 `en`。顯示時以 `LanguageCode` 和既有 `storybook.language` 合約選擇語言，缺漏或無效時回退繁體中文。`/quiz` 套用既有 theme boot script 與 CSS tokens，不提供 theme selector。所有答案選項、提交、下一題與故事連結具 accessible name、可鍵盤操作與可見 focus。

**理由**: 問答是跨故事書功能，必須和恐龍、水族館、探索、比較、旅程、護照的語言與主題體驗一致。答案狀態不能只靠顏色，必須搭配文字或狀態提示。

**替代方案**:
- Server-side cookie 語言: 偏離既有 `storybook.language` localStorage contract。
- 問答頁新增主題 selector: 違反 dark mode feature 約束。
- 只用色彩表示正誤: 不符合 FR-030 與 accessibility 需求。

## Decision 9: Friendly fallback 優先顯示可用題目或安全空狀態

**決策**: 題庫部分無效時，顯示剩餘有效題目並記錄非敏感摘要；某 scope 無有效題時，顯示該範圍暫時沒有題目的友善提示；全部題庫不可用或來源 catalog 失敗導致無題可用時，顯示回首頁/故事入口 action，不顯示內部例外、檔案路徑或 stack trace。

**理由**: FR-027 與 edge cases 要求資料不可用時不能空白頁，也不能暴露內部診斷。部分可用時，讓孩子仍能完成可用範圍的問答。

**替代方案**:
- fail-fast 導向 Error page: 對內容部分失效不夠友善。
- 顯示內部 validation messages 給使用者: 會暴露維護者細節且不符合兒童語氣。

## Decision 10: 測試以 service rules、PageModel handler 與 HTML contract 分層

**決策**: 單元測試集中驗證題庫 validation、scope filtering、answer evaluation、language fallback、next cycling 與 source failure。整合測試使用 `WebApplicationFactory<Program>` 驗證 `/quiz`、form post、entry links、theme selector absence、related story anchors 與 friendly fallback。`quiz.js` 若新增，使用 script contract test 檢查不使用 `localStorage`、`sessionStorage`、cookie、fetch、History API 或 jQuery 作為核心流程。

**理由**: 憲章要求測試先行，且問答同時涉及資料規則、Razor Pages handler、跨頁連結、主題/語言與可及性。分層測試能快速定位失敗並避免過度依賴瀏覽器端測試。

**替代方案**:
- 只做 manual QA: 不足以覆蓋題庫規則與 handler 回歸。
- 只做 browser E2E: 成本較高且不適合作為 validation rules 的主要回歸網。
