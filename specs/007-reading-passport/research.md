# Research: 小小探險護照

## Decision 1: 維持 StoryBook2 Razor Pages 技術棧，不沿用 CardPicker2 app/data layout

**決策**: 護照功能實作在現有 `StoryBook2.sln` 的單一 ASP.NET Core Razor Pages app 中，使用 `StoryBook/Pages/Passport/Index.cshtml`、既有 dinosaur/aquarium catalog services、Bootstrap 5、原生 JavaScript 與 xUnit 測試。先前提到的 `CardPicker2/data/cards.json` 與早餐/午餐/晚餐卡牌資料模型不適用於此功能。

**理由**: 目前 feature spec 是跨既有恐龍與水族館故事的閱讀護照，不是新的抽卡 CRUD app。專案憲章也要求維持單一 Razor Pages web application，內容資料放在 `StoryBook/Data/`，使用者狀態不得離開同一瀏覽器。

**替代方案**:
- 建立 CardPicker2-style JSON 寫入資料檔：會引入伺服器端使用者狀態與不相干資料模型，違反護照本機 storage 邊界。
- 建立新 SPA 或 Web API：對本機閱讀狀態過度設計，增加 CSP、測試與錯誤處理面。

## Decision 2: 護照狀態只使用 `localStorage` key `storybook.passport`

**決策**: 使用瀏覽器 `localStorage` 保存 `storybook.passport`，內容是版本化 JSON `{ version, completedStories: [{ source, slug }] }`。`version` 初始為 `1`；每筆 item 只保存 `source` 與 `slug`，`source` enum 固定為 `dinosaurs`、`aquarium`。

**理由**: Spec clarifications 已固定 storage key、shape、allowed source 與 invalid data 行為。localStorage 能支援同瀏覽器重新整理/重開後保留狀態，且不需要登入、cookie、資料庫或跨裝置同步。

**替代方案**:
- Cookie：會增加 request payload 並把閱讀狀態傳回伺服器，不符合本機狀態最小化。
- Server-side file/database：擴大資料保存範圍，也需要身份或裝置識別來分辨使用者。
- URL/query string：會讓閱讀狀態進入 history、分享連結與伺服器 logs。

## Decision 3: 伺服器產生故事/徽章 metadata，瀏覽器計算個人進度

**決策**: `PassportCatalogService` 在伺服器端組合所有可用故事、來源總數、固定 badge 定義與友善 source status；`passport.js` 讀取 localStorage，依頁面輸出的 story metadata 過濾未知項目、計算已讀數、已讀清單與 badge 狀態。

**理由**: localStorage 只能在瀏覽器端讀取；但可用故事總數、雙語標題、摘要、來源排序與 canonical href 應由既有 catalog 決定。將兩者分工可避免 API，並確保 `/passport` 沒有失效連結或空白文字。

**替代方案**:
- 只用 JavaScript hard-code story list：會和 `dinosaurs.json` / `aquarium.json` 漂移。
- POST localStorage 到 server 計算：會把閱讀狀態送出本機瀏覽器，違反 FR-005/FR-025。

## Decision 4: 使用 `PassportCatalogService` 與 `PassportPreferenceService`

**決策**: 新增 `PassportCatalogService` 組合 dinosaur/aquarium catalog projection，新增 `PassportPreferenceService` 暴露 `StorageKey`、`StateVersion`、allowed source codes 與 badge metadata。兩者註冊為 singleton，PageModel 注入後輸出 page contract。

**理由**: 現有 `LanguagePreferenceService`、`ThemePreferenceService` 與跨來源 `ExplorationCatalogService` 已建立此模式。將 storage key/version 與 catalog projection 放在 service，可讓 PageModel 薄層化並可單元測試。

**替代方案**:
- 在 Razor view 內直接組資料：難以測試 source failure、排序與 fallback。
- 只在 `passport.js` 定義 storage key/version：會讓 integration tests 難以驗證 HTML contract，也不符合公開可重用 metadata 的 XML 文件要求。

## Decision 5: 重用 `ExplorationSourceType` source metadata

**決策**: 護照 item source 使用既有 `ExplorationSourceType` 與 extension methods 取得 `ToCode()`、`GetRoutePrefix()`、`GetSortOrder()`、`GetLabel()`；localStorage 仍保存規格要求的小寫 code。

**理由**: `/explore`、`/compare`、`/journeys` 已使用同一來源 metadata。護照也需要相同 source code、route prefix、source order 與雙語 label，重用可避免未來漂移。

**替代方案**:
- 新增 `PassportSourceType`：命名貼近功能，但會產生同義 enum 與轉換重複。
- 使用自由字串到處判斷：短期少檔案，但 schema、service、JS 與 route mapping 容易不一致。

## Decision 6: 詳情頁完成控制只做顯式使用者動作

**決策**: 恐龍與水族館詳情頁在故事內容附近加入 button，例如「我讀完了」，並以 `data-passport-source`、`data-passport-slug` 標示目前故事。`passport.js` 只在 button click/keyboard activation 時保存完成項目，不因頁面載入、停留、捲動或 modal 開啟自動保存。

**理由**: 規格明確要求完成必須由使用者明確操作觸發。同一 button 能在已讀時顯示友善已完成狀態，再次操作不新增重複 item。

**替代方案**:
- 讀到頁尾自動完成：不符合 FR-004，也會誤計孩子只是瀏覽的故事。
- 使用連結到 `/passport?complete=...`：會把閱讀狀態放進 URL/history，違反資料邊界。

## Decision 7: 固定徽章由目前有效故事集合推導

**決策**: 固定 5 個 badge：first story、3 stories、all dinosaurs、all aquarium、all stories。`PassportCatalogService` 提供每個 badge 的 target type 與 source scope；`passport.js` 根據有效已讀 item count 與 source total 即時計算 locked/unlocked/unavailable。

**理由**: Badge 不應保存為狀態快照，否則 catalog 改變時會過期。由目前有效故事與 localStorage item 推導能避免重複保存，也能在 invalid item 被忽略後保持一致。

**替代方案**:
- 在 localStorage 保存 unlocked badges：違反 FR-025 的徽章快照限制，且會與有效故事總數漂移。
- 隨機或時間式徽章：違反 FR-011。

## Decision 8: Invalid/unavailable storage 採友善降級

**決策**: `passport.js` 使用 `try/catch` 包住 localStorage read/write/remove。讀取失敗時故事頁仍完整可讀，完成控制顯示「護照暫時不能打開」類提示；寫入失敗時顯示「暫時不能保存」；格式錯誤、版本不支援或項目無效時忽略資料並在下次成功保存/清除寫回有效 shape。

**理由**: 瀏覽器可能在隱私模式、企業政策或 storage quota 下拒絕 storage。護照是附加功能，不得破壞既有閱讀流程。

**替代方案**:
- 改存 sessionStorage/cookie：仍可能被封鎖，且 sessionStorage 不符合重新開啟後保留；cookie 會傳回伺服器。
- 讓 button 失敗時拋出錯誤：會破壞兒童友善體驗。

## Decision 9: 雙語、主題與 accessibility 沿用既有合約

**決策**: Razor 預設輸出繁體中文，所有護照文字、aria-label、狀態訊息、badge label、clear confirmation 與 empty/error state 都提供 `data-i18n-zh-tw` / `data-i18n-en` 或等價 data contract；無效語言回退繁體中文。主題使用既有 layout boot script 與 CSS tokens，`/passport` 不提供 theme selector。

**理由**: 既有恐龍、水族館、探索、比較與旅程頁都使用 data attribute language pattern 與 shared theme contract。護照頁跨來源，必須和整站語言/主題一致，並保持 keyboard/focus 可用。

**替代方案**:
- 伺服器端 cookie 語言：偏離既有 `storybook.language` contract。
- 新增護照頁 theme selector：違反 dark mode feature 約束。

## Decision 10: 不新增 Serilog、Moq 或 jQuery 依賴

**決策**: 此功能使用 ASP.NET Core 內建 `ILogger<T>`，單元測試沿用現有 fake environment/recording logger/test fixture pattern，不新增 Serilog 或 Moq。護照互動用原生 JavaScript，不依賴 jQuery。

**理由**: 目前 project files 沒有 Serilog/Moq package，憲章要求新增依賴前先確認內建能力不足。護照 server-side 訊號很小，`ILogger<T>` 足夠；測試可透過真實服務、臨時 catalog path 與既有 support helpers 驗證；jQuery 對 localStorage/button DOM 更新沒有必要。

**替代方案**:
- 新增 Serilog file sink：會增加設定、檔案權限、敏感資料審查與營運面，但本功能沒有 file log 需求。
- 新增 Moq：可 mock catalog service，但現有服務具體類別與 test fixtures 已能覆蓋 source failure。
- 使用 jQuery：增加新互動對 legacy library 的耦合，且現有新功能已偏向原生 JS。
