# UI and Route Contracts: 水族館動物介紹故事書

## Public Routes

| Route | Page | Contract |
|-------|------|----------|
| `/` | `Pages/Index.cshtml` | 首頁必須提供清楚的「水族館動物介紹」入口，連到 `/aquarium`。 |
| `/aquarium` | `Pages/Aquarium/Index.cshtml` | 顯示水族館故事書主頁、封面、10-40 字歡迎文字、搜尋入口、語言切換、開始閱讀動作與完整可探索集合。 |
| `/aquarium/{slug}` | `Pages/Aquarium/Details.cshtml` | 顯示單一水族館生物詳情；`slug` 必須對應 `AquariumAnimalProfile.slug`。 |
| `/aquarium/{unknown-slug}` | `Pages/Aquarium/Details.cshtml` | 回傳 HTTP 404，顯示友善找不到內容狀態，並提供返回首頁與 `/aquarium` 的連結。 |

Canonical routes 使用 lowercase `/aquarium` 與 kebab-case slug。一般連結必須可直接開啟，不依賴 JavaScript router。

## Aquarium Home Contract

- 必須顯示水族館主題封面與 10-40 字歡迎文字。
- 必須有「開始閱讀」動作，連到 sortOrder = 1 的 `/aquarium/{slug}`。
- 必須顯示可探索的 15 筆內容，至少包含名稱、生活區域分類、短摘要與開啟介紹頁動作。
- 必須有搜尋框，label 或 accessible name 不得缺漏。
- 必須有語言切換控制，支援 `zh-TW` 與 `en`。
- 必須有回首頁導覽動作。

## Search Contract

- 搜尋比對範圍包含雙語動物名稱、生活區域分類、生活環境、食性與 search keywords。
- 搜尋前必須 trim 前後空白並合併多餘空白。
- 英文搜尋必須忽略大小寫差異。
- 搜尋輸入變更後 1 秒內更新結果或無結果提示。
- 搜尋結果每筆必須是可鍵盤聚焦的 anchor link，連到 `/aquarium/{slug}`。
- 搜尋無結果時必須顯示 5-10 歲孩童能理解的友善提示，並提供清除搜尋或回到完整集合的方式。
- 清除搜尋後必須恢復顯示完整可探索集合。

## Aquarium Details Contract

- 每頁一次只聚焦一隻水族館生物。
- 必須顯示名稱、生活區域分類、生活環境、食性、發現地點、簡短介紹、小故事與主要圖片。
- 主要圖片與故事插圖必須有有意義的雙語替代文字。
- 主要圖片必須可用滑鼠或鍵盤開啟大圖檢視。
- 大圖檢視必須可用明確關閉控制與 Escape 關閉；關閉後焦點回到觸發元素。
- 圖片無法顯示時，必須保留版面並顯示友善替代訊息。
- 第一筆內容不得提供可啟用的「上一頁」；最後一筆內容不得提供可啟用的「下一頁」。
- 上一頁、下一頁、回水族館主頁與回首頁都使用 anchor link，以保留瀏覽器 history。

## Language Switch Contract

- 支援 `zh-TW` 與 `en`。
- 預設語言為 `zh-TW`。
- 使用者切換語言後，導覽文字、水族館內容、搜尋結果、分類名稱、提示訊息、簡介、小故事、圖片 alt/caption 都必須更新或回退中文。
- 語言偏好以 `localStorage` key `storybook.language` 保存，並與恐龍故事書共享。
- 無效語言值或缺漏內容必須回退 `zh-TW`，不得讓可見文字區塊空白。

## Accessibility Contract

- 所有互動控制項必須可用 Tab 聚焦與 Enter/Space 啟用。
- 焦點狀態必須可見，不得被 CSS 移除。
- 搜尋框、搜尋清除、圖片開啟、大圖關閉、語言切換、上一頁與下一頁控制項都必須有 accessible name。
- 找不到內容、搜尋無結果、資料載入失敗與圖片無法顯示訊息必須是孩童可理解的友善文字。
- 頁面主要內容必須有明確 heading hierarchy，避免只靠顏色傳達狀態。
