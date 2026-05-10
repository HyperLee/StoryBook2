# Research: 水族館動物介紹故事書

## Decision: 維持 ASP.NET Core Razor Pages 作為主要應用模型

**Rationale**: 既有 repository 已是 `net10.0` Razor Pages 專案，而且水族館需求自然對應首頁、主頁、詳情頁與找不到內容頁。使用 Razor Pages 可沿用 `@page`、Tag Helpers、PageModel 與 injectable services，讓 route、HTML contract、資料載入與測試方式都和恐龍故事書一致。

**Alternatives considered**:
- Blazor Web App：互動能力較強，但本功能只有搜尋、語言切換與圖片 modal，不需要 component render mode 或額外狀態模型。
- MVC：可行，但對頁面導向內容展示增加 controller/view ceremony。
- SPA framework：會引入前端 build pipeline、client router 與 hydration 複雜度，超出 15 筆固定內容需求。

## Decision: 使用本機 JSON 內容檔，並放在 `StoryBook/Data/aquarium.json`

**Rationale**: 初始內容固定為 15 種水族館生物與雙語文字，不需要後台寫入、資料庫 migration 或外部資料同步。放在 `StoryBook/Data/` 可讓 server-side service 統一載入、驗證、快取與測試，且避免把未審核資料合約公開成 web root 靜態資產。

**Alternatives considered**:
- `wwwroot/data/aquarium.json`：大綱原本採此方式，但公開資料路徑會把 JSON 變成 public contract，也和既有恐龍資料放置方式不一致。
- SQL 資料庫：對固定內容展示過度設計，會增加連線字串、migration、備份與部署設定。
- 外部 CMS 或百科 API：會引入網路依賴、內容審核與兒童友善風險，不符合本階段範圍。

## Decision: 新增水族館專用 catalog model，沿用共享語言偏好服務

**Rationale**: 水族館內容欄位與恐龍類似，但具有 `HabitatCategory`、生活環境與不同驗證規則。新增 `Aquarium*` models/services 可降低跨 feature 耦合；`LanguageCode` 與 `LanguagePreferenceService` 已經支援 `zh-TW`、`en` 與 `storybook.language`，可作為故事書共用能力。

**Alternatives considered**:
- 直接重用 `DinosaurProfile`：會讓水族館欄位被恐龍語意污染，也難以加入生活區域分類。
- 先抽象成通用 `StorybookCatalog<T>`：目前只有兩個小型 catalog，抽象成本高於收益；後續第三個類似 feature 再評估。

## Decision: 原生 JavaScript + Bootstrap 5 處理前端互動

**Rationale**: Bootstrap 5 已存在並可支援 layout、form control 與 modal。原生 JavaScript 足以處理 15 筆資料的即時搜尋、語言切換、localStorage 與圖片放大，不需要 jQuery 或新的前端套件。

**Alternatives considered**:
- jQuery：專案已包含，但新互動不需要其 API；保留既有 validation/template assets 即可。
- React/Vue/Svelte：會新增 build pipeline 與狀態同步成本。
- 純 server-side 無 JavaScript：可完成基本瀏覽，但無法自然支援即時搜尋、localStorage 語言偏好與 modal 焦點回復。

## Decision: 使用語言中立 URL 與共享 `storybook.language`

**Rationale**: Canonical routes 固定為 `/aquarium` 與 `/aquarium/{slug}`。同一動物的中文與英文不需要兩條 URL；JavaScript 根據 `localStorage` key `storybook.language` 套用 `zh-TW` 或 `en`。此 key 與恐龍故事書共享，讓使用者在故事書功能間保留語言偏好。

**Alternatives considered**:
- `/zh-TW/aquarium/{slug}` 與 `/en/aquarium/{slug}`：語言 URL 清楚，但會增加 route、測試與 link generation 面積。
- Cookie/server-side preference：可讓初始 HTML 直接使用偏好語言，但本功能沒有登入或 server-side 個人化需求。

## Decision: 上一頁、下一頁與搜尋結果使用一般 anchor link

**Rationale**: 需求要求直接開啟 `/aquarium/{slug}`，並保留瀏覽器 history。一般 anchor link 最符合此行為，也能在停用 JavaScript 時維持基本閱讀流程。PageModel 依 `sortOrder` 計算 previous/next，第一筆與最後一筆不提供有效的不存在連結。

**Alternatives considered**:
- JavaScript-only router：會增加 route 同步、焦點管理與無障礙複雜度。
- Query string index：URL 可讀性較差，且不如 slug 穩定。

## Decision: 測試採 xUnit + WebApplicationFactory，瀏覽器互動以 quickstart 手動驗收

**Rationale**: constitution 要求測試先行。服務層可用 xUnit 驗證 JSON 載入、內容驗證、搜尋、語言 fallback、slug lookup 與導覽規則；整合測試用 `WebApplicationFactory<Program>` 驗證 DI、routes、Razor Pages pipeline、404 與 HTML contract。圖片 modal、焦點回復與鍵盤操作先列入 quickstart 手動驗收，若後續回歸成本升高再加入 Playwright for .NET。

**Alternatives considered**:
- 只做手動測試：不符合測試優先原則，也無法保護 catalog 規則。
- 立即導入完整 E2E：本功能規模小，先用單元/整合測試覆蓋主要風險較務實。

## Decision: 使用 ASP.NET Core 內建 `ILogger<T>`，暫緩 Serilog

**Rationale**: 大綱提到 Serilog，但目前專案沒有安裝 Serilog，且 constitution 要求優先使用 ASP.NET Core 與 .NET 內建能力。水族館功能沒有集中式日誌、file sink 或跨服務追蹤需求，先以 `ILogger<T>` 記錄資料載入、驗證與未知 slug 即可。

**Alternatives considered**:
- 立即加入 Serilog Console/File sink：會新增 NuGet、設定與維運面；目前沒有足夠營運需求。
- 不記錄日誌：資料檔損壞、缺圖或未知 slug 會較難診斷，不符合可觀察性原則。
