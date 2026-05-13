# Quickstart: 全站探索與分類搜尋

## 前置條件

- .NET SDK compatible with `net10.0`
- Repository root: `/Users/qiuzili/StoryBook2`

## 建置與測試

```bash
dotnet restore StoryBook2.sln
dotnet build StoryBook2.sln
dotnet test StoryBook2.sln
```

## 本機執行

```bash
dotnet run --project StoryBook/StoryBook.csproj
```

使用 `dotnet run` 輸出的 URL；目前 launch settings 包含:

- `http://localhost:5059`
- `https://localhost:7111`

## 手動驗收流程

1. Open `/`.
2. Confirm the homepage includes a clear "探索全部故事" entry.
3. Activate the entry and confirm the browser navigates to `/explore`.
4. Confirm the page shows both dinosaur and aquarium content when both catalogs load.
5. Activate a dinosaur result and confirm it opens `/dinosaurs/{slug}`.
6. Return to `/explore`, activate an aquarium result, and confirm it opens `/aquarium/{slug}`.

## 搜尋驗證

1. On `/explore`, search `暴龍`.
2. Confirm the Tyrannosaurus result remains visible and the result status explains the current search state.
3. Search `shark` while the UI language is `zh-TW`.
4. Confirm the shark result can still be found, but visible text follows the current language with `zh-TW` fallback if needed.
5. Search with punctuation-only text or a single valid character.
6. Confirm the page shows a child-friendly too-short prompt and keeps the available collection visible.
7. Clear search and confirm the full collection returns.

## 分類驗證

1. Select the source filter for `恐龍`.
2. Confirm only dinosaur results remain.
3. Select a diet, habitat, period, or discovery-location filter from another group.
4. Confirm results match all selected groups.
5. Select a different value in the same group.
6. Confirm it replaces the prior group selection.
7. Clear filters and confirm the full collection returns, unless a valid search query is still active.

## 語言與主題驗證

1. Set language to English using the existing language control.
2. Open `/explore` and confirm labels, result text, status text, filter labels, and accessible names use English where available.
3. Set language to an invalid value in `localStorage` key `storybook.language`.
4. Reload `/explore` and confirm the page falls back to `zh-TW` without blank labels.
5. Set theme to dark from the homepage.
6. Open `/explore` and confirm the effective dark theme is applied.
7. Confirm `/explore` does not render a new theme selector.

## 響應式與可及性驗證

檢查代表寬度:

- 375px
- 768px
- 1366px

每個寬度都需確認:

- No horizontal overflow.
- No overlapping text or controls.
- Search input, clear controls, filter controls, and result links are keyboard focusable.
- Focus indicators are visible.
- Operable targets are at least 44x44 CSS px where applicable.
- Status messages are announced by a polite live region.

## 狀態保存驗證

1. Search or select filters on `/explore`.
2. Confirm the browser URL remains `/explore` without query parameters.
3. Confirm browser history is not changed by search/filter interactions.
4. Reload `/explore`.
5. Confirm search and filters reset to the full collection.
