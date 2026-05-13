# UI Routes Contract: 全站探索與分類搜尋

## Route: `GET /explore`

**Purpose**: 顯示全站探索頁，整合恐龍與水族館故事內容。

**預期回應**

- Status: 至少一個來源可呈現頁面狀態時回傳 `200 OK`。
- Content-Type: `text/html; charset=utf-8`.
- Layout: 使用 shared layout 與 `_Layout.cshtml` 的有效主題 attributes。
- 本頁不得呈現 theme selector。
- 本頁不得要求 authentication。

**伺服器輸出合約**

Rendered page 必須包含:

- 可用 `zh-TW` 與 `en` 呈現的全站探索主標題。
- 使用一般 `<a>` element 的回首頁 link 與結果 link。
- Search input:
  - `type="search"`
  - accessible name in `zh-TW` and `en`
  - `data-explore-search-input`
- Clear search control:
  - `type="button"`
  - `data-explore-clear-search`
  - accessible name in `zh-TW` and `en`
- Filter controls:
  - grouped by source, diet, habitat/living area, period, or discovery location when values exist
  - each group exposes `data-explore-filter-group="{groupCode}"`
  - each selectable value exposes `data-explore-filter-value="{valueCode}"`
  - only one value per group can be active
- Result cards:
  - anchor `href` points to `/dinosaurs/{slug}` or `/aquarium/{slug}`
  - `data-explore-result-item`
  - `data-explore-source="{sourceCode}"`
  - `data-explore-search-text="{bilingualSearchText}"`
  - `data-explore-facets` contains machine-readable group/value pairs
  - visible source label, name, short summary, image or meaningful alternative text, and explicit detail action
- Status regions:
  - `data-explore-result-status` with `aria-live="polite"` for all/search/filter/intersection summary
  - `data-explore-too-short` for normalized query length under 2
  - `data-explore-no-results`
  - `data-explore-partial-failure` when one source is unavailable
  - `data-explore-all-failed` when no source is available

**狀態合約**

- Initial `/explore` render shows the full available collection.
- Query parameters MUST NOT initialize search or filter state.
- JavaScript MUST NOT call `pushState` or `replaceState` for search/filter changes.
- Search/filter state MUST NOT be written to `localStorage`, `sessionStorage`, cookies, or server state.
- Reloading or revisiting `/explore` returns to the full available collection.

**搜尋合約**

- Normalization: NFKC, lowercase invariant, keep only Unicode letters and digits.
- Empty, punctuation-only, or normalized query length `< 2` shows a friendly too-short prompt and keeps all available results visible.
- Valid query matches against bilingual `zh-TW` + `en` search text regardless of current UI language.
- English matching is case-insensitive.
- Results update within 1 second of input.

**分類合約**

- Source filter includes at least `dinosaurs` and `aquarium` when those sources are available.
- Each group allows one selected value at a time.
- Selecting a new value in the same group replaces the prior selection.
- Different groups combine with AND.
- Search and filter together show only results matching both the valid query and every selected group.
- Clearing filters restores the full collection or current search result set, depending on whether a valid search query remains.

**排序合約**

- Default, searched, and filtered results are grouped by source order:
  1. `dinosaurs`
  2. `aquarium`
- Within each source, preserve the source catalog `SortOrder`.
- No personalization or relevance ranking is used.

**失敗狀態合約**

- If one source fails, the page displays available source content plus a child-friendly partial failure message.
- If all sources fail, the page displays a child-friendly error state and a home link.
- Internal exception details, file paths, and stack traces are not visible to users.

## 首頁入口合約: `GET /`

首頁必須包含清楚的 "探索全部故事" / "Explore all stories" 入口:

- Uses a normal anchor to `/explore`.
- Is discoverable in the primary home action area.
- Does not change existing dinosaur or aquarium entry links.
- Does not add a theme selector outside the existing homepage selector.

## 既有詳情頁導覽合約

探索結果 links 必須只導向既有 routes:

- Dinosaur detail: `/dinosaurs/{slug}`
- Aquarium detail: `/aquarium/{slug}`

本功能不得新增 `/explore/{slug}` 這類探索專用詳情頁。

## 整合測試期望

整合測試應驗證:

- `/explore` returns `200 OK` and includes dinosaur and aquarium results when catalogs load.
- Homepage contains a link to `/explore`.
- Result links point to existing detail routes.
- Rendered HTML contains search, clear, filter, status, and result data attributes.
- Rendered HTML does not contain a theme selector and does not contain `pushState` / `replaceState`.
- Partial and all-source failure fixtures render friendly states without blank pages.
