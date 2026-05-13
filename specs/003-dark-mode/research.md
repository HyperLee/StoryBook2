# Research: 網站深色模式與主題切換

## Decision: 使用瀏覽器端 `localStorage` 保存主題模式

**Rationale**: 主題偏好範圍限定於同一瀏覽器與裝置，沒有帳號、跨裝置同步或 server-side 個人化需求。保存 `light`、`dark`、`system` 三種「使用者選擇的模式」即可滿足回訪需求，也避免把執行期間推導出的有效亮色/深色誤存成偏好。

**Alternatives considered**:
- Cookie/server-side preference：可讓伺服器輸出已個人化 HTML，但本功能沒有登入與跨裝置需求，會增加 server-side 狀態與測試面。
- Query string 或 route segment：會污染內容 canonical routes，且主題切換不應影響瀏覽器 history。
- 只用 `prefers-color-scheme` 不保存：無法滿足使用者明確選擇亮色或深色並於回訪沿用。

## Decision: 在 layout head 執行早期 theme boot script

**Rationale**: 規格要求頁面載入時避免先顯示相反主題再切換。將極小同步 script 放在 `_Layout.cshtml` 的 stylesheet 前，可在 CSS 套用前先設定 `data-bs-theme` 與專案自訂 data attributes。無效或不可讀取的 localStorage 值回到 `system`，系統偏好不可判斷時使用亮色安全預設。

**Alternatives considered**:
- 只在 `DOMContentLoaded` 後套用：實作簡單，但容易出現可見閃爍。
- 伺服器依 cookie 套用：可避免 flicker，但需要新增 cookie 寫入、同意/隱私考量與 server-side 狀態。
- 純 CSS `prefers-color-scheme`：能處理 system，但無法表達使用者固定亮色/深色覆蓋。

## Decision: 使用 Bootstrap 5 `data-bs-theme` 搭配專案 semantic CSS variables

**Rationale**: Bootstrap 5 已在專案內，且可透過 `data-bs-theme` 切換 body、forms、buttons 等基礎 token。專案現有 CSS 有許多硬編碼亮色值，因此需在 `site.css` 定義 `--storybook-*` semantic variables，並在 dinosaurs/aquarium CSS 針對 feature-specific 區塊補深色 override，避免修改 vendor Bootstrap 檔。

**Alternatives considered**:
- 直接重寫整份 Bootstrap theme：成本高且容易造成 vendor drift。
- 只加 `body.dark` 少量樣式：無法穩定覆蓋表單、navbar、cards、alerts、modal 與 feature CSS。
- 使用 CSS preprocessor 或新 build pipeline：現有專案沒有前端 build 流程，對本功能過度設計。

## Decision: 建立專用 `theme.js`，不依賴 jQuery

**Rationale**: 主題互動是跨站共用行為，包含首頁 selector、localStorage 寫入、system mode 的 `matchMedia` listener、同站跨分頁 `storage` event 與 labels 狀態更新。專用原生 JavaScript 檔可讓 dinosaurs/aquarium feature script 保持只處理各自搜尋、語言與圖片互動，也符合新互動不依賴 jQuery 的專案規則。

**Alternatives considered**:
- 放進 `site.js`：可行，但現有 `site.js` 是模板空檔；專用檔案能讓主題功能邊界更清楚。
- 放進 `dinosaurs.js` / `aquarium.js`：會造成跨站邏輯重複，也違反 feature ownership。
- 使用 frontend framework：不需要為單一 selector 與 storage sync 引入 build pipeline。

## Decision: 首頁使用可及的 segmented radio group 作為主題 selector

**Rationale**: 三種模式互斥，radio group 的語意最直接，也能自然支援鍵盤操作、目前選取狀態與 accessible name。Bootstrap 可協助視覺呈現，但 DOM 合約仍以原生 radio/label 為主，確保輔助科技可理解。

**Alternatives considered**:
- 三個普通 button + `aria-pressed`：可行，但需自行維護互斥語意，較容易出現狀態不同步。
- `<select>`：可及性良好，但目前選取狀態不如 segmented control 直覺，且較不符合首頁主要設定動作。
- Toggle switch：只能表達二元，不適合三種模式。

## Decision: 自動化測試覆蓋 server-rendered contract，瀏覽器環境行為列入 quickstart

**Rationale**: xUnit 與 `WebApplicationFactory<Program>` 能穩定驗證 theme metadata、layout script ordering、首頁 selector、非首頁無 selector、routes theme attributes 與 HTML contract。系統外觀切換、首次可見閃爍、跨分頁同步、焦點回復與實際對比需要真實瀏覽器環境，先列入 quickstart 手動驗收；若後續多次回歸成本升高，再導入 Playwright for .NET。

**Alternatives considered**:
- 只做手動測試：不符合 constitution 的測試優先要求。
- 立即導入 Playwright：可提高覆蓋，但本階段主要是 layout 與 storage 行為；先用整合測試鎖定 HTML contract 較務實。
- 把 theme resolution 完全搬到 C# 單元測試：localStorage、matchMedia、storage event 是瀏覽器 API，C# 無法代表真實行為。

## Decision: 不記錄瀏覽器主題偏好內容到 server logs

**Rationale**: 主題偏好不是 server-side 業務資料，也不需營運追蹤。若 server rendering 或 service metadata 發生例外，可用 `ILogger<T>` 記錄非敏感事件；瀏覽器 localStorage 內容、系統偏好與分頁狀態不送回伺服器。

**Alternatives considered**:
- 新增 telemetry endpoint：超出需求，且會增加隱私、測試與營運成本。
- 加入 Serilog/file sink：目前無集中式日誌需求，內建 logging 足夠。
