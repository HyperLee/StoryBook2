# UI Routes Contract: 內容比較器

## Route: `GET /compare`

**Purpose**: 顯示內容比較器，讓使用者選擇兩位故事朋友並查看欄位比較。

**預期回應**

- Status: 至少一個來源可呈現頁面狀態時回傳 `200 OK`。
- Content-Type: `text/html; charset=utf-8`.
- Layout: 使用 shared layout 與 `_Layout.cshtml` 的有效主題 attributes。
- 本頁不得呈現 theme selector。
- 本頁不得要求 authentication。

**伺服器輸出合約**

Rendered page 必須包含:

- 可用 `zh-TW` 與 `en` 呈現的比較器主標題。
- 使用一般 `<a>` element 的回首頁 link。
- 第一位故事朋友選擇控制:
  - accessible name in `zh-TW` and `en`
  - `data-compare-first-select`
- 第二位故事朋友選擇控制:
  - accessible name in `zh-TW` and `en`
  - `data-compare-second-select`
- 清除選擇控制:
  - `type="button"`
  - `data-compare-clear-selection`
  - accessible name in `zh-TW` and `en`
- 候選 metadata:
  - each candidate exposes stable id `{source}:{slug}`
  - source code, slug, localized name, source label, summary, diet, living area, period, discovery location, and detail href
  - source-specific not-applicable values are present as display text, not empty values
- 比較表 container:
  - `data-compare-table`
  - hidden until two different candidates are selected
  - each row exposes one field via `data-compare-field="{fieldCode}"`
  - detail actions are normal `<a>` elements pointing to existing detail routes
- Status regions:
  - `data-compare-status` with `aria-live="polite"` for empty, one-selected, duplicate, ready, not-enough, partial-failure, and all-failed states
  - `data-compare-duplicate-message`
  - `data-compare-one-selected-message`
  - `data-compare-not-enough`
  - `data-compare-partial-failure` when one source is unavailable
  - `data-compare-all-failed` when no source is available

**狀態合約**

- Initial `/compare` render shows selection controls and no comparison table when at least two candidates are available.
- Query parameters MUST NOT initialize first/second candidate state.
- JavaScript MUST NOT call `pushState` or `replaceState` for selection changes.
- Selection state MUST NOT be written to `localStorage`, `sessionStorage`, cookies, or server state.
- Reloading or revisiting `/compare` returns to the empty selection state.
- Theme changes may update visual appearance but MUST NOT clear or mutate current selections.

**候選合約**

- Full catalog state includes 23 candidates: 8 dinosaur items and 15 aquarium items.
- Candidate stable id format is `{sourceType}:{slug}`.
- Slug uniqueness is only required within each source storybook.
- Default candidate order:
  1. `dinosaurs`
  2. `aquarium`
- Within each source, preserve the source catalog `SortOrder`.

**比較合約**

- Selecting two different candidates shows the comparison table within 1 second.
- Selecting the same candidate in both controls shows a child-friendly duplicate message and hides the comparison table.
- Selecting only one candidate shows a child-friendly prompt to choose one more friend.
- Clearing selection returns both controls and messages to the initial state.
- Table rows include:
  - source
  - name
  - diet
  - living area
  - period
  - discovery location
  - summary
  - detail link
- A field that is not applicable to one source must show child-friendly not-applicable text.
- The table must not show blank values, `null`, `undefined`, internal field names, file paths, or exception details.

**失敗狀態合約**

- If one source fails, the page displays available source candidates plus a child-friendly partial failure message.
- If fewer than two candidates are available, the page displays a child-friendly "need at least two friends" state and does not offer a misleading comparison table.
- If all sources fail, the page displays a child-friendly error state and a home link.
- Internal exception details, file paths, and stack traces are not visible to users.

## 首頁入口合約: `GET /`

首頁必須包含清楚的 "比較故事朋友" / "Compare story friends" 入口:

- Uses a normal anchor to `/compare`.
- Is discoverable in the primary home action area.
- Does not remove or change existing dinosaur, aquarium, `/explore`, or theme selector behavior.
- Does not add an extra theme selector.

## 探索頁入口合約: `GET /explore`

`/explore` 必須包含清楚的 "比較故事朋友" / "Compare story friends" 入口:

- Uses a normal anchor to `/compare`.
- Does not change existing exploration search/filter state behavior.
- Does not add a theme selector.
- Does not turn exploration result links into JavaScript-only navigation.

## 既有詳情頁導覽合約

比較表中的 detail links 必須只導向既有 routes:

- Dinosaur detail: `/dinosaurs/{slug}`
- Aquarium detail: `/aquarium/{slug}`

本功能不得新增 `/compare/{slug}` 或其他比較專用詳情頁。

## 整合測試期望

整合測試應驗證:

- `/compare` returns `200 OK` and includes dinosaur and aquarium candidates when catalogs load.
- Homepage contains a link to `/compare`.
- `/explore` contains a link to `/compare`.
- Candidate options expose stable ids and localized labels.
- Rendered HTML contains first select, second select, clear, status, table, and candidate metadata data attributes.
- Rendered HTML does not contain a theme selector and does not contain `pushState` / `replaceState`.
- Detail links point to existing detail routes.
- Partial, not-enough, and all-source failure fixtures render friendly states without blank pages.
