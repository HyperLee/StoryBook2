# Quickstart: 網站深色模式與主題切換

## Prerequisites

- .NET SDK 10.x
- A desktop browser: Chrome, Firefox, Safari, or Edge
- Browser DevTools access for localStorage and emulated color-scheme checks

## Build and Test

```bash
dotnet restore StoryBook2.sln
dotnet build StoryBook2.sln
dotnet test StoryBook2.sln
```

`dotnet test` 必須覆蓋 theme metadata、layout contract、首頁 selector、非首頁無 selector 與主要 routes 的 theme attributes。瀏覽器環境行為依下方手動驗收補足。

## Run Locally

```bash
dotnet run --project StoryBook/StoryBook.csproj
```

Open the HTTPS or HTTP URL printed by `dotnet run`, then verify the routes below:

- `/`
- `/Privacy`
- `/Error`
- `/dinosaurs`
- `/dinosaurs/tyrannosaurus-rex`
- `/aquarium`
- `/aquarium/clownfish`

## Manual Acceptance Checklist

1. 清除 `localStorage` 的 `storybook.theme`，重新開啟首頁，確認預設模式為「跟隨系統」且畫面沒有先閃相反主題。
2. 在首頁依序選擇「深色模式」、「亮色模式」、「跟隨系統」，每次切換後 1 秒內畫面與 selector 目前選取狀態一致。
3. 選擇「深色模式」後前往 `/Privacy`、`/dinosaurs`、`/dinosaurs/tyrannosaurus-rex`、`/aquarium`、`/aquarium/clownfish`，確認所有頁面維持深色，且首頁以外沒有主題 selector。
4. 選擇「亮色模式」後重新整理與關閉重開瀏覽器，確認 `storybook.theme` 保存為 `light` 並自動套用亮色。
5. 將 `storybook.theme` 手動改成無效值，重新整理，確認系統回到「跟隨系統」行為且首頁 selector 不出現空白或未選取狀態。
6. 在「跟隨系統」模式下，用 DevTools Rendering 或系統設定切換 prefers-color-scheme，確認頁面 2 秒內跟著變更。
7. 在「亮色模式」或「深色模式」下切換系統外觀偏好，確認網站仍維持使用者明確選擇。
   - 補充檢查：使用 DevTools snippet、測試 profile 或瀏覽器隱私設定模擬 `localStorage` 不可用、`matchMedia` 缺漏或丟出例外時，頁面仍需可操作、selector 不得空白，且有效主題需回到安全預設或可判斷的系統模式。
8. 開兩個同站分頁，在第一個分頁首頁切換主題，確認第二個分頁 2 秒內同步有效主題且不改變所在 route。
9. 在恐龍或水族館頁輸入搜尋、切換語言、開啟圖片大圖後改變主題，確認搜尋輸入、語言、圖片狀態、scroll position 與故事內容不被重置。
10. 使用鍵盤 Tab/Shift+Tab/Arrow/Space/Enter 操作首頁主題 selector、語言切換、故事入口、搜尋、上一頁/下一頁與圖片檢視，確認焦點清楚可見。
11. 切換 `storybook.language` 為英文，確認首頁主題 selector 的 label/description 變英文；設定無效語言值後確認回退繁體中文。
12. 在 375px、768px、1366x768 寬度檢查首頁、恐龍頁與水族館頁，確認文字、按鈕、卡牌、圖片區塊、表單與主題 selector 不重疊且沒有不合理水平捲動。
13. 在亮色、深色與跟隨系統三種模式下檢查 body、navbar、footer、links、buttons、forms、cards、alerts、modal、搜尋結果與錯誤/空狀態的對比與焦點指示。

## HTML Contract Checks

實作任務需加入整合測試，至少覆蓋：

- `_Layout.cshtml` 輸出早期 theme boot script，且位於主要 stylesheet 前或足夠早的位置。
- 主要頁面 HTML 包含 `data-bs-theme` 初始化路徑或可被 boot script 設定的 contract。
- 首頁包含 `data-theme-storage-key="storybook.theme"` 與三個 `data-theme-option` 值。
- `/Privacy`、`/Error`、`/dinosaurs`、`/dinosaurs/{slug}`、`/aquarium`、`/aquarium/{slug}` 不包含主題 selector。
- 主題 selector label 同時包含 `data-i18n-zh-tw` 與 `data-i18n-en` 或等效 bilingual contract。
- 主題 assets 不依賴 jQuery，且不新增 JavaScript-only router。
- `theme.js` 對 `localStorage`、`matchMedia` 與 `storage` event listener 失敗情境有防護，不得讓頁面變成不可操作狀態。
