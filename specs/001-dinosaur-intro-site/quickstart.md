# Quickstart: 兒童恐龍介紹網站

## Prerequisites

- .NET SDK 10.x
- A desktop browser: Chrome, Firefox, Safari, or Edge

## Build and Test

```bash
dotnet restore StoryBook2.sln
dotnet build StoryBook2.sln
dotnet test StoryBook2.sln
```

`dotnet test` becomes mandatory after `StoryBook.Tests` is added by implementation tasks. If no tests exist yet, the command should be re-run once the first TDD task creates the test project.

## Run Locally

```bash
dotnet run --project StoryBook/StoryBook.csproj
```

Open the HTTPS or HTTP URL printed by `dotnet run`, then verify the routes below:

- `/`
- `/dinosaurs`
- `/dinosaurs/tyrannosaurus-rex`
- `/dinosaurs/pteranodon`
- `/dinosaurs/not-a-real-slug`

## Manual Acceptance Checklist

1. 從首頁 3 秒內找到「恐龍介紹」入口並進入 `/dinosaurs`。
2. 在 `/dinosaurs` 搜尋「暴龍」，1 秒內只顯示符合結果或明確排序的結果。
3. 清空搜尋後，確認 8 筆內容都可見。
4. 點擊任一搜尋結果後進入 `/dinosaurs/{slug}`，並顯示名稱、圖片、生活時期、食性、發現地點、體型描述、簡短介紹、小故事與故事插圖。
5. 在第一筆內容確認「上一頁」不可啟用；在最後一筆內容確認「下一頁」不可啟用。
6. 使用上一頁/下一頁連續瀏覽至少 3 筆，再用瀏覽器上一頁/下一頁確認歷史紀錄正確。
7. 點擊主圖片開啟大圖，使用 Escape 或關閉按鈕關閉，焦點回到觸發元素。
8. 切換英文後，導覽、內容、搜尋結果、提示訊息、圖片 alt/caption 與故事文字都更新為英文。
9. 重新整理頁面或開啟另一筆恐龍介紹，確認語言偏好保持。
10. 開啟 `/dinosaurs/not-a-real-slug`，確認顯示友善找不到內容提示，並可返回首頁或完整清單。
11. 使用鍵盤 Tab/Shift+Tab/Enter/Space 完成入口、搜尋、語言切換、上一頁/下一頁、開關大圖與回首頁流程。
12. 在 1366x768 與 768px 寬度檢查主要介紹內容不需水平捲動，且上一頁、下一頁、回首頁、搜尋清除與語言切換控制項維持可見且容易操作。

## Content Validation

實作任務需加入自動化內容驗證，至少覆蓋：

- 8 筆 `slug` 唯一且符合 kebab-case。
- 每筆都有 `zh-TW` 與 `en` 內容。
- 每篇簡介符合 200 個可閱讀單位限制。
- 快取後 slug lookup 與搜尋查詢 p95 低於 200ms。
- 每則小故事符合 100-150 個可閱讀單位限制。
- 翼龍 `category` 為 `prehistoric-flying-reptile` 且有非恐龍說明。
- 每張主圖與故事插圖都有雙語 alt text。
