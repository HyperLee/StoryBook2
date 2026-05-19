# Quickstart: 內容比較器

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
2. Confirm the homepage includes a clear "比較故事朋友" entry.
3. Activate the entry and confirm the browser navigates to `/compare`.
4. Return to `/explore`.
5. Confirm `/explore` includes a clear "比較故事朋友" entry.
6. Activate the entry and confirm the browser navigates to `/compare`.
7. Confirm `/compare` shows first and second story friend selection controls.

## 代表性可用性紀錄

用於驗證 SC-001 與 SC-010；可由人工可用性驗收或同等瀏覽器任務紀錄完成。紀錄不可包含兒童個資、帳號、token、secret 或可識別個人的資料。

### SC-001 入口尋找任務紀錄格式

至少紀錄 20 次代表性 entry-finding attempts，且其中至少 95% 需在 5 秒內從首頁或 `/explore` 找到並進入 `/compare`。

| Attempt | Start Page (`/` or `/explore`) | Completed Within 5s? | Navigated To `/compare`? | Notes |
|---------|--------------------------------|----------------------|---------------------------|-------|
| 1 | | | | |

### SC-010 比較理解任務紀錄格式

至少紀錄 10 位代表性測試使用者或同等人工驗收任務；其中至少 90% 需能正確說出兩位故事朋友至少一個相同點或不同點。

| Attempt | Compared Friends | User Identified Same/Different Point? | Correct? | Notes |
|---------|------------------|----------------------------------------|----------|-------|
| 1 | | | | |

## 比較流程驗證

1. On `/compare`, select `暴龍` as the first story friend.
2. Confirm the page shows a child-friendly prompt to choose one more friend and does not show a comparison table.
3. Select `鯊魚` as the second story friend.
4. Confirm the comparison table appears within 1 second.
5. Confirm the table includes source, name, diet, living area, period, discovery location, summary, and detail link rows.
6. Confirm dinosaur living area and aquarium period use child-friendly not-applicable text.
7. Activate each detail link and confirm it opens `/dinosaurs/{slug}` or `/aquarium/{slug}`.
8. Return to `/compare`, select the same story friend in both controls, and confirm the duplicate-selection prompt appears and the table is hidden.
9. Use the clear selection control and confirm the page returns to the initial empty state.

## 比較互動瀏覽器驗證

此段補足 `CompareScriptContractTests` 無法直接執行 DOM runtime 的行為。

1. Select the same story friend in both controls and confirm the duplicate-selection prompt appears within 1 second.
2. Confirm the comparison table remains hidden while the duplicate-selection prompt is visible.
3. Select two different story friends and confirm the ready comparison table appears within 1 second.
4. Use the clear selection control and confirm both controls reset, status text returns to the initial prompt, and the table is hidden.
5. With two different story friends selected, change the effective theme from another tab or homepage control and return to `/compare`.
6. Confirm only visual appearance changes; selected candidates and visible comparison content remain unchanged.

## 語言與主題驗證

1. Set language to English using the existing language control.
2. Open `/compare` and confirm labels, candidate names, table fields, prompts, not-applicable text, and accessible names use English where available.
3. Set language to an invalid value in `localStorage` key `storybook.language`.
4. Reload `/compare` and confirm the page falls back to `zh-TW` without blank labels or values.
5. Set theme to dark from the homepage.
6. Open `/compare` and confirm the effective dark theme is applied.
7. Confirm `/compare` does not render a new theme selector.
8. Change theme while a comparison is visible and confirm only visual appearance changes; selected candidates and table content remain unchanged.

## 響應式與可及性驗證

檢查代表寬度:

- 375px
- 768px
- 1366px

每個寬度都需確認:

- No horizontal overflow.
- No overlapping text or controls.
- First select, second select, clear control, and detail links are keyboard focusable.
- Focus indicators are visible.
- Operable targets are at least 44x44 CSS px where applicable.
- Status messages are announced by a polite live region.
- Comparison rows remain readable; mobile layout may stack values but must preserve field labels.

## 狀態保存驗證

1. Select two different story friends on `/compare`.
2. Confirm the browser URL remains `/compare` without query parameters.
3. Confirm browser history is not changed by selection interactions.
4. Confirm no comparison state appears in `localStorage`, `sessionStorage`, cookies, or server state.
5. Reload `/compare`.
6. Confirm selections reset to the initial empty state.

## 來源失敗驗證

1. Simulate aquarium source unavailable using test fixture or development configuration.
2. Open `/compare`.
3. Confirm dinosaur candidates remain available and a child-friendly partial failure message appears.
4. Simulate only one total candidate available.
5. Confirm the page explains that at least two story friends are needed.
6. Simulate all sources unavailable.
7. Confirm the page shows a child-friendly error state and a home link within 1 second, without internal exception details.

## 負向範圍稽核

交付前確認本功能沒有引入規格排除的能力:

- `/compare` does not require authentication and does not add login, account sync, favorites, recommendations, or saved history.
- `/compare` does not search external websites, call external encyclopedia APIs, or add a new external content source.
- `/compare` does not add real-time translation; it only uses existing bilingual content and `zh-TW` fallback rules.
- `/compare` does not add comparison URL query state, server-side comparison state, cookies, `localStorage`, or `sessionStorage` writes.
- `/compare` detail actions remain normal anchors to `/dinosaurs/{slug}` or `/aquarium/{slug}` and do not introduce `/compare/{slug}` detail routes.
