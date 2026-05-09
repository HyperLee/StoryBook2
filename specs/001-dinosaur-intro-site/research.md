# Research: 兒童恐龍介紹網站

## Decision: 維持 ASP.NET Core Razor Pages 作為主要應用模型

**Rationale**: 既有 repository 已是 `net10.0` Razor Pages 專案，需求也以首頁、清單、詳情頁與找不到內容頁為主，路由自然對應頁面。Razor Pages 可用 `@page`、Tag Helpers、PageModel 與 injectable services 維持簡單架構，符合 constitution 對一致 UX 與服務邊界的要求。

**Alternatives considered**:
- Blazor Web App：互動能力較強，但本功能只有少量搜尋、語言切換與 modal，不需要引入 component render mode 與額外狀態模型。
- MVC：可行但對此頁面導向功能增加 controller/view ceremony。
- SPA framework：會新增建構工具與前端狀態複雜度，超出 8 筆靜態內容展示需求。

## Decision: 使用本機 JSON 內容檔，不使用資料庫

**Rationale**: 初始範圍固定為 8 筆史前生物與雙語文字，沒有後台、登入、使用者投稿或高並發寫入需求。JSON 檔容易審查、版本控制與在測試中載入，能用 service 層集中驗證 slug、語言欄位、字數限制與圖片替代文字。

**Alternatives considered**:
- SQL 資料庫：對固定內容展示過度設計，且需要 migration、連線字串、備份與部署設定。
- `wwwroot/data/dinosaurs.json`：可讓前端直接讀取，但會把資料合約綁到公開靜態資產路徑；本階段改放 `StoryBook/Data/dinosaurs.json`，由 server render 與測試共同使用。
- 外部 CMS 或百科 API：會引入網路依賴、內容審核與兒童友善風險。

## Decision: 原生 JavaScript + Bootstrap 5 處理前端互動

**Rationale**: Bootstrap 5 已存在於專案，能支援 modal、按鈕與表單控制；原生 JavaScript 足以處理 8 筆資料的即時搜尋、語言切換與圖片大圖檢視。新互動不依賴 jQuery，避免在新程式碼中增加舊式 API 耦合。

**Alternatives considered**:
- jQuery：已存在但不是必要依賴；保留給模板或 validation assets，不作為新互動的主要 API。
- React/Vue/Svelte：需要額外 build pipeline 與 hydration 策略，對固定內容不划算。
- 純 server-side 無 JavaScript：可完成基本瀏覽，但無法自然支援即時搜尋、localStorage 語言偏好與 modal 體驗。

## Decision: 使用語言中立 URL，語言偏好存在 `localStorage`

**Rationale**: 每筆恐龍內容需要唯一網址，但同一內容的中文與英文不需要分成兩條 URL。使用 `/dinosaurs/{slug}` 作 canonical route，頁面輸出雙語內容或可切換文案，JavaScript 根據 `storybook.language` localStorage 套用 `zh-TW` 或 `en`。預設語言為中文，缺少英文內容時回退中文並避免空白。

**Alternatives considered**:
- `/zh-TW/dinosaurs/{slug}` 與 `/en/dinosaurs/{slug}`：SEO 與分享較明確，但會加倍路由與測試面，超出本階段需求。
- Cookie/server-side preference：可讓初始 HTML 直接使用偏好語言，但本功能沒有登入或 server-side 個人化需求；若後續需要無閃爍語言切換，可再加 cookie。

## Decision: 上一頁、下一頁、搜尋結果全部使用一般 anchor link

**Rationale**: 需求要求直接網址與瀏覽器上一頁/下一頁保留流程。一般連結最符合瀏覽器歷史模型，也能在停用 JavaScript 時維持基本瀏覽。PageModel 依 JSON 順序計算 previous/next slug，第一筆與最後一筆停用或隱藏無效控制項。

**Alternatives considered**:
- `history.pushState` 單頁切換：可減少頁面載入，但會增加路由同步、焦點管理與無障礙處理複雜度。
- Query string index：URL 可讀性較差，也不如 slug 穩定。

## Decision: 測試採 xUnit + WebApplicationFactory，瀏覽器互動先以手動驗收記錄

**Rationale**: constitution 要求測試先行。服務層可用 xUnit 驗證 JSON 載入、搜尋、語言 fallback、slug 正規化與內容限制；整合測試用 `WebApplicationFactory<Program>` 驗證 route、DI、HTML 狀態與 404 友善頁。大圖 modal、焦點狀態、鍵盤操作與視覺一致性先列入 quickstart 手動驗收，若後續反覆回歸成本升高再加入 Playwright for .NET。

**Alternatives considered**:
- 只做手動測試：不符合 constitution 的測試優先要求。
- 立即導入完整 E2E：本功能規模小，先用可執行服務/整合測試覆蓋主要風險較務實。

## Decision: 使用 ASP.NET Core 內建 `ILogger<T>`，暫緩 Serilog

**Rationale**: `markdownFolder/tempPlan.md` 提到 Serilog，但目前 csproj 未安裝 Serilog，且 constitution 優先要求使用 ASP.NET Core 與 .NET 內建能力。此功能沒有集中式日誌、file sink 或跨服務追蹤需求，先用內建 logging 記錄資料載入與路由異常即可。

**Alternatives considered**:
- 立即加入 Serilog Console/File sink：可行但新增套件與設定面；除非後續營運需求明確，否則不值得在靜態內容功能中引入。
- 不記錄日誌：資料檔損壞、缺圖、未知 slug 會較難診斷，不符合可觀察性原則。
