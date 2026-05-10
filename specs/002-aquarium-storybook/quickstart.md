# Quickstart: 水族館動物介紹故事書

## Prerequisites

- .NET SDK 10.x
- A desktop browser: Chrome, Firefox, Safari, or Edge

## Build and Test

```bash
dotnet restore StoryBook2.sln
dotnet build StoryBook2.sln
dotnet test StoryBook2.sln
```

`dotnet test` is mandatory after implementation tasks add or update the aquarium tests. The existing `StoryBook.Tests` project should remain part of `StoryBook2.sln`.

## Run Locally

```bash
dotnet run --project StoryBook/StoryBook.csproj
```

Open the HTTPS or HTTP URL printed by `dotnet run`, then verify the routes below:

- `/`
- `/aquarium`
- `/aquarium/clownfish`
- `/aquarium/axolotl`
- `/aquarium/not-a-real-slug`

## Manual Acceptance Checklist

1. 從首頁 3 秒內找到「水族館動物介紹」入口並進入 `/aquarium`。
2. 在 `/aquarium` 確認可看到水族館封面、10-40 字歡迎文字、搜尋入口、語言切換、回首頁與「開始閱讀」動作。
3. 點擊「開始閱讀」後，5 秒內進入 sortOrder = 1 的 `/aquarium/clownfish` 或實作指定的第一筆內容。
4. 在 `/aquarium` 搜尋「海馬」與 `reef`，1 秒內看到符合結果或無結果提示；輸入單一字元、空白或只有符號時，看到過短搜尋提示且不出現誤導性結果。
5. 清空搜尋後，確認 15 筆內容都恢復可見。
6. 點擊任一搜尋結果後進入 `/aquarium/{slug}`，並顯示名稱、生活區域分類、生活環境、食性、發現地點、簡短介紹、小故事與圖片。
7. 在第一筆內容確認「上一頁」不可啟用；在最後一筆內容確認「下一頁」不可啟用。
8. 使用上一頁/下一頁連續瀏覽至少 3 筆，再用瀏覽器上一頁/下一頁確認 history 正確；快速連續點擊上一頁或下一頁時，最終畫面符合最後一次有效操作。
9. 點擊主要圖片開啟大圖，使用 Escape 或關閉按鈕關閉，焦點回到觸發元素。
10. 切換 English 後，導覽、內容、搜尋結果、分類名稱、提示訊息、圖片 alt/caption 與故事文字都更新為英文或中文 fallback。
11. 重新整理頁面或開啟另一筆水族館介紹，確認 `storybook.language` 語言偏好保持。
12. 開啟 `/aquarium/not-a-real-slug`，確認 HTTP 404、友善找不到內容提示，並可返回首頁或 `/aquarium`。
13. 使用鍵盤 Tab/Shift+Tab/Enter/Space 完成入口、搜尋、清除搜尋、語言切換、上一頁/下一頁、開關大圖與回首頁流程。
14. 快速連續輸入多個搜尋關鍵字時，最終結果符合最後一次有效查詢，且結果、過短提示或無結果提示都在輸入變更後 1 秒內更新。
15. 以測試設定或測試 fixture 模擬 `StoryBook/Data/aquarium.json` 無法讀取，確認畫面顯示孩童可理解的資料錯誤訊息，並提供重新嘗試與回首頁動作。
16. 在 1366x768 與 768px 寬度檢查主要內容不需水平捲動，且搜尋、語言切換、上一頁、下一頁與回首頁控制項維持可見且容易操作。

## Content Validation

實作任務需加入自動化內容驗證，至少覆蓋：

- 15 筆 `slug` 唯一且符合固定清單：`clownfish`、`seahorse`、`sea-turtle`、`jellyfish`、`octopus`、`shark`、`stingray`、`penguin`、`seal`、`dolphin`、`starfish`、`crab`、`coral`、`goldfish`、`axolotl`。
- 每筆都有 `zh-TW` 與 `en` 的名稱、分類、生活環境、食性、發現地點、簡介、小故事與圖片替代文字。
- 至少 5 種生活區域分類，且分類代碼與顯示名稱雙語完整。
- 每篇簡介符合每種語言不超過 200 個可閱讀單位。
- 每則小故事符合每種語言 100-150 個可閱讀單位。
- 繁體中文可閱讀單位以可見中文字元計算；英文以單字計算；空白與標點不計入。
- 每張主圖與故事插圖都有雙語 alt text，且 alt text 不得只是檔名。
- 快取後 slug lookup、previous/next 與搜尋查詢 p95 低於 200ms。
