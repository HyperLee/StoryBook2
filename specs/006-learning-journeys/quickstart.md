# Quickstart: 主題學習旅程

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
2. Confirm the homepage includes a clear "學習旅程" entry.
3. Activate the entry and confirm the browser navigates to `/journeys`.
4. Return to `/explore`.
5. Confirm `/explore` includes a clear "學習旅程" entry.
6. Activate the entry and confirm the browser navigates to `/journeys`.
7. Confirm `/journeys` shows at least 3 available learning journeys in the full catalog state.

## 代表性可用性紀錄

用於驗證 SC-001 與 SC-010；可由人工可用性驗收或同等瀏覽器任務紀錄完成。紀錄不可包含兒童個資、帳號、token、secret 或可識別個人的資料。

### SC-001 入口尋找任務紀錄格式

至少紀錄 20 次代表性 entry-finding attempts，且其中至少 95% 需在 5 秒內從首頁或 `/explore` 找到並進入 `/journeys`。

| Attempt | Start Page (`/` or `/explore`) | Completed Within 5s? | Navigated To `/journeys`? | Notes |
|---------|--------------------------------|----------------------|----------------------------|-------|
| 1 | | | | |

### SC-010 旅程理解任務紀錄格式

至少紀錄 10 位代表性測試使用者或同等人工驗收任務；其中至少 90% 需能在閱讀旅程詳情後說出這條旅程要認識的主題與下一步要讀的故事。

| Attempt | Journey | User Identified Topic? | User Identified Next Story? | Correct? | Notes |
|---------|---------|------------------------|-----------------------------|----------|-------|
| 1 | | | | | |

## 旅程列表驗證

1. Open `/journeys`.
2. Confirm the page title, intro, available journey cards, reading time, age guidance, and story counts are visible.
3. Confirm every visible journey card has a normal anchor to `/journeys/{slug}`.
4. Confirm each visible journey has 3-5 story items.
5. Confirm journeys with fewer than 3 valid items or more than 5 valid items are hidden from the list in test fixtures.
6. Confirm the list order follows journey `SortOrder`, then slug.

## 旅程詳情驗證

1. Open a known available `/journeys/{slug}`.
2. Confirm the page shows journey title, summary, learning goals, suggested reading time, age guidance, story item list, start-reading action, and back-to-list action.
3. Confirm "開始閱讀" opens the first valid story item in journey order.
4. Confirm dinosaur story items open `/dinosaurs/{slug}`.
5. Confirm aquarium story items open `/aquarium/{slug}`.
6. Confirm story item display names and summaries match existing source story content.
7. Confirm browser back/forward, open in new tab, and copy link work for journey and story links.

## 主要內容效能驗證

用於驗證 SC-011；在完整 catalog 狀態下，先開啟一次 `/journeys` 與一個已知 `/journeys/{slug}` 作為 warm-up，再對兩個 route 各連續載入 5 次。可使用瀏覽器任務紀錄、DevTools timing 或同等整合測試紀錄；紀錄不得包含檔案路徑、secret、token、個資或例外細節。

| Route | Attempt | Main Content Available Within 1s? | Measurement Method | Non-sensitive Notes |
|-------|---------|-----------------------------------|--------------------|---------------------|
| `/journeys` | 1 | | | |
| `/journeys` | 2 | | | |
| `/journeys` | 3 | | | |
| `/journeys` | 4 | | | |
| `/journeys` | 5 | | | |
| `/journeys/{slug}` | 1 | | | |
| `/journeys/{slug}` | 2 | | | |
| `/journeys/{slug}` | 3 | | | |
| `/journeys/{slug}` | 4 | | | |
| `/journeys/{slug}` | 5 | | | |

## 語言與主題驗證

1. Set language to English using the existing language control.
2. Open `/journeys` and `/journeys/{slug}`.
3. Confirm labels, journey titles, summaries, goals, age guidance, prompts, and accessible names use English where available.
4. Set language to an invalid value in `localStorage` key `storybook.language`.
5. Reload journey pages and confirm the page falls back to `zh-TW` without blank labels or values.
6. Set theme to dark from the homepage.
7. Open journey pages and confirm the effective dark theme is applied.
8. Confirm journey pages do not render a new theme selector.
9. Change theme while on a journey page and confirm only visual appearance changes; journey content, item order, route, history, and scroll position are not changed.

## 響應式與可及性驗證

檢查代表寬度:

- 375px
- 768px
- 1366px

每個寬度都需確認:

- No horizontal overflow.
- No overlapping text or controls.
- Journey card links, start-reading, story item links, and back actions are keyboard focusable.
- Focus indicators are visible.
- Operable targets are at least 44x44 CSS px where applicable.
- Status messages use understandable child-friendly text.
- Heading hierarchy is clear.

## 不可用與錯誤狀態驗證

1. Open `/journeys/unknown-slug`.
2. Confirm the page shows a child-friendly not-found message and normal anchors back to `/journeys` and `/`.
3. Simulate a journey with fewer than 3 valid story items.
4. Confirm it does not appear on `/journeys`.
5. Open its direct `/journeys/{slug}` route and confirm a friendly unavailable message appears without a start-reading link.
6. Simulate a journey with more than 5 valid story items.
7. Confirm it is hidden from `/journeys` and the direct detail page asks for a smaller route without internal diagnostics.
8. Simulate a duplicate or missing story reference.
9. Confirm no duplicate cards or broken links appear.

## 來源失敗驗證

1. Simulate aquarium source unavailable using test fixture or development configuration.
2. Open `/journeys`.
3. Confirm journeys that remain complete are visible and a child-friendly partial failure message appears.
4. Open a journey whose missing source makes it incomplete.
5. Confirm the detail page shows a friendly unavailable state and no broken links.
6. Simulate all sources unavailable.
7. Confirm `/journeys` shows a child-friendly error state and a home link within 1 second.
8. Confirm rendered HTML and journey-level logs do not include file paths, exception details, stack traces, internal configuration, secrets, or personal data.

## 狀態保存驗證

1. Open an available `/journeys/{slug}` and activate a story link.
2. Return to the journey page.
3. Confirm the page does not mark any story as completed.
4. Reload `/journeys` and `/journeys/{slug}`.
5. Confirm no reading progress, completion state, or personalized recommendation is restored.
6. Confirm no journey progress appears in `localStorage`, `sessionStorage`, cookies, query string, browser history changes, or server state.

## 負向範圍稽核

交付前確認本功能沒有引入規格排除的能力:

- Journey pages do not require authentication and do not add login, account sync, favorites, comments, recommendations, badges, or saved history.
- Journey pages do not search external websites, call external encyclopedia APIs, or add a new external content source.
- Journey pages do not add real-time translation; they only use maintained bilingual content and `zh-TW` fallback rules.
- Journey pages do not add journey progress URL query state, server-side state, cookies, `localStorage`, or `sessionStorage` writes.
- Story item detail actions remain normal anchors to `/dinosaurs/{slug}` or `/aquarium/{slug}` and do not introduce journey-specific story detail routes.
