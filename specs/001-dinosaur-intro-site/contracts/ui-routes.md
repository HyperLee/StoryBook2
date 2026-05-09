# UI and Route Contracts: 兒童恐龍介紹網站

## Public Routes

| Route | Page | Contract |
|-------|------|----------|
| `/` | `Pages/Index.cshtml` | 首頁必須提供清楚的「恐龍介紹」入口，連到 `/dinosaurs`。 |
| `/dinosaurs` | `Pages/Dinosaurs/Index.cshtml` | 顯示可搜尋的恐龍內容清單，預設語言為中文並套用使用者語言偏好。 |
| `/dinosaurs/{slug}` | `Pages/Dinosaurs/Details.cshtml` | 顯示單一恐龍/史前生物詳情；`slug` 必須對應 `DinosaurProfile.slug`。 |
| `/dinosaurs/{unknown-slug}` | `Pages/Dinosaurs/Details.cshtml` | 顯示友善找不到內容狀態，並提供返回首頁與完整清單的路徑。 |

Canonical route 使用 lowercase `/dinosaurs` 與 kebab-case slug。一般連結必須可直接開啟，不依賴 JavaScript router。

## Dinosaur List Contract

- 必須有搜尋框，label 或可辨識的 accessible name 不得缺漏。
- 搜尋輸入變更後 1 秒內更新結果。
- 搜尋比對範圍包含名稱、生活時期、食性、發現地點、體型描述、簡介與關鍵字。
- 搜尋無結果時顯示友善提示，並提供清除搜尋控制項。
- 每個搜尋結果必須是可聚焦連結，連到 `/dinosaurs/{slug}`。
- 翼龍結果必須清楚標示「史前飛行爬行類，不是真正恐龍」或對應英文文案。

## Dinosaur Details Contract

- 必須顯示名稱、主圖片、生活時期、食性、發現地點、體型描述、簡短介紹、小故事與故事插圖。
- 主圖片與故事插圖必須有有意義的替代文字。
- 主圖片必須可用滑鼠或鍵盤開啟大圖檢視。
- 大圖檢視必須可用關閉按鈕、背景點擊或 Escape 關閉，關閉後焦點回到觸發元素。
- 第一筆內容不得提供可啟用的「上一頁」；最後一筆內容不得提供可啟用的「下一頁」。
- 上一頁、下一頁、搜尋結果與回首頁都使用 anchor link，以保留瀏覽器 history。

## Language Switch Contract

- 支援 `zh-TW` 與 `en`。
- 預設語言為 `zh-TW`。
- 使用者切換語言後，導覽文字、恐龍資料、搜尋結果、提示訊息、簡介、小故事、圖片 alt/caption 都必須更新。
- 語言偏好以 `localStorage` key `storybook.language` 保存。
- 無效語言值或缺漏內容必須回退 `zh-TW`，不得讓可見文字區塊空白。

## Accessibility Contract

- 所有互動控制項必須可用 Tab 聚焦與 Enter/Space 啟用。
- 焦點狀態必須可見，不得被 CSS 移除。
- 圖片按鈕、語言切換、大圖關閉與搜尋清除控制項必須有 accessible name。
- 找不到內容、搜尋無結果與資料載入失敗訊息必須是孩童可理解的友善文字。
- 頁面主要內容必須有明確 heading hierarchy，避免只靠顏色傳達狀態。
