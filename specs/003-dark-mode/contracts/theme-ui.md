# UI Contract: 網站深色模式與主題切換

## Public Route Contract

| Route | Theme Contract |
|-------|----------------|
| `/` | 必須套用有效主題，並顯示唯一的主題模式 selector。 |
| `/Privacy` | 必須套用有效主題，不得顯示主題模式 selector。 |
| `/Error` | 必須套用有效主題，不得顯示主題模式 selector。 |
| `/dinosaurs` | 必須套用有效主題，不得顯示主題模式 selector，且不得改變恐龍搜尋與語言流程。 |
| `/dinosaurs/{slug}` | 必須套用有效主題，不得顯示主題模式 selector，且不得改變上一頁/下一頁、圖片檢視或故事內容。 |
| `/aquarium` | 必須套用有效主題，不得顯示主題模式 selector，且不得改變水族館搜尋與語言流程。 |
| `/aquarium/{slug}` | 必須套用有效主題，不得顯示主題模式 selector，且不得改變上一頁/下一頁、圖片檢視或故事內容。 |

既有 canonical routes 不得因主題切換新增 query string、hash、route segment 或 JavaScript-only navigation。

## Theme Attribute Contract

- `<html>` 必須在首次可見呈現前設定 `data-bs-theme`，值只允許 `light` 或 `dark`。
- `<html>` 必須設定 `data-storybook-theme-mode`，值只允許 `light`、`dark` 或 `system`。
- `<html>` 必須設定 `data-storybook-effective-theme`，值只允許 `light` 或 `dark`。
- 無效、缺漏或不可讀取的 `storybook.theme` 必須視為 `system`。
- `system` 模式下若無法判斷 `prefers-color-scheme`，有效主題必須是 `light`。
- 主題初始化 script 必須位於主要 stylesheet 之前或足夠早的位置，避免可見相反主題閃爍。

## Storage Contract

- 使用者主題模式保存於 `localStorage` key `storybook.theme`。
- 保存值只允許 `light`、`dark`、`system`。
- 不得保存 `data-storybook-effective-theme` 的推導結果。
- localStorage 不可用時，頁面必須仍可操作，並以 `system` 模式解析有效主題。
- 主題偏好不得送回伺服器或寫入 server logs。

## Home Theme Selector Contract

- Selector 只出現在首頁 `/`。
- Selector 必須提供三個互斥選項：亮色模式、深色模式、跟隨系統。
- Selector 必須可用滑鼠、觸控與鍵盤操作。
- Selector 群組必須有 accessible name，且目前選取狀態必須可由輔助科技辨識。
- 每個可操作選項至少 44x44 CSS px。
- Selector 文案必須支援 `zh-TW` 與 `en`，並跟隨既有 `storybook.language`。
- 無效語言或翻譯缺漏時必須回退繁體中文，不得顯示空白 label。
- 切換後 1 秒內，首頁視覺主題、selector 狀態與 `localStorage` 必須一致。

## System Mode Contract

- `system` 模式必須使用 `matchMedia('(prefers-color-scheme: dark)')` 判斷系統偏好。
- 系統偏好由深色改亮色或由亮色改深色時，若目前模式為 `system`，頁面必須在 2 秒內更新有效主題。
- 若目前模式為 `light` 或 `dark`，系統偏好變更不得覆蓋使用者明確選擇。

## Cross-Tab Sync Contract

- 同一網站的其他已開啟分頁必須監聽 `storage` event。
- 任一分頁在首頁變更 `storybook.theme` 後，其他分頁必須在 2 秒內重新解析有效主題。
- 其他分頁若不是首頁，不得因此顯示 selector。
- 同步不得清空搜尋輸入、改變語言偏好、關閉圖片 modal、改變 scroll position 或觸發 route navigation。

## Visual and Accessibility Contract

- 亮色與深色模式都必須讓 body、navbar、footer、links、buttons、forms、cards、alerts、modal、搜尋結果、圖片說明、找不到內容與資料載入失敗狀態可讀。
- 對比與焦點指示需符合 WCAG 2.2 AA。
- 不得只靠顏色傳達目前選取、錯誤、空狀態或 disabled 狀態。
- 768px 以上寬度不得水平捲動；手機與平板寬度需保持主要內容與主題控制項可操作。
- 主題切換不得造成文字、按鈕、卡牌、圖片區塊或表單重疊。

## Non-Goals

- 不新增 `/theme` API、cookie endpoint 或 server-side preference route。
- 不新增自訂色票、排程切換、帳號同步或每頁獨立主題。
- 不修改恐龍與水族館內容 JSON schema。
