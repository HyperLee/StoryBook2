# UI Routes Contract: 主題學習旅程

## Route: `GET /journeys`

**Purpose**: 顯示可出發的主題學習旅程列表。

**預期回應**

- Status: `200 OK`，包含列表、空狀態或友善錯誤狀態。
- Content-Type: `text/html; charset=utf-8`.
- Layout: 使用 shared layout 與 `_Layout.cshtml` 的有效主題 attributes。
- 本頁不得呈現 theme selector。
- 本頁不得要求 authentication。

**伺服器輸出合約**

Rendered page 必須包含:

- 可用 `zh-TW` 與 `en` 呈現的頁面主標題。
- 使用一般 `<a>` element 的回首頁 link。
- 使用一般 `<a>` element 的 `/explore` link 或可返回探索頁的導覽。
- 旅程列表 container: `data-journeys-list`.
- 每條可出發旅程卡片:
  - `data-journey-card`
  - `data-journey-slug="{slug}"`
  - 雙語 title、summary、learning goal summary、reading time、age guidance
  - story item count，且必須為 3-5
  - 一般 anchor 指向 `/journeys/{slug}`
  - accessible name in `zh-TW` and `en`
- Partial source failure status:
  - `data-journeys-partial-failure`
  - child-friendly text
  - no file paths, exception details, stack traces, internal configuration, or secrets
- All unavailable/all failed state:
  - `data-journeys-all-unavailable`
  - child-friendly text
  - normal home anchor

**列表合約**

- 完整資料情境下至少顯示 3 條可開啟旅程。
- 旅程排序先依 journey `SortOrder`，再依 slug。
- 有效故事項目少於 3 筆或超過 5 筆的旅程不得出現在可選列表。
- 列表不得顯示空白 title、summary、learning goals、reading time 或 age guidance。
- 列表不得顯示失效 story detail link。
- Query parameters MUST NOT change visible journeys or initialize reading/progress state.
- 本頁 MUST NOT write journey state to `localStorage`, `sessionStorage`, cookies, query string, browser history, or server state.

## Route: `GET /journeys/{slug}`

**Purpose**: 顯示單一學習旅程詳情、學習目標、建議閱讀順序與開始閱讀動作。

**預期回應**

- Status: `200 OK` for available, unavailable, and not-found friendly states.
- Content-Type: `text/html; charset=utf-8`.
- Layout: 使用 shared layout 與 `_Layout.cshtml` 的有效主題 attributes。
- 本頁不得呈現 theme selector。
- 本頁不得要求 authentication。

**可出發旅程輸出合約**

Rendered page 必須包含:

- `data-journey-detail`
- `data-journey-slug="{slug}"`
- 雙語旅程 title、summary、learning goals、reading time、age guidance
- 開始閱讀 action:
  - normal `<a>`
  - `data-journey-start-reading`
  - href 指向旅程排序中的第一筆有效故事項目
- 返回旅程列表 action:
  - normal `<a>`
  - href `/journeys`
- 旅程故事項目清單:
  - 每個 item 有 `data-journey-story-item`
  - stable id `{source}:{slug}`
  - source label、story name、story summary、order number
  - normal `<a>` detail link
  - dinosaur item href `/dinosaurs/{slug}`
  - aquarium item href `/aquarium/{slug}`
- Status region:
  - `aria-live="polite"` when the page displays unavailable or partial messages.

**不可用旅程狀態合約**

- 有效故事項目少於 3 筆時:
  - Render `data-journey-unavailable`
  - Show child-friendly message that the route is temporarily not ready.
  - Do not show a misleading start-reading link.
- 有效故事項目超過 5 筆時:
  - Render `data-journey-unavailable`
  - Show child-friendly message and a maintainer-oriented but non-sensitive hint that the route needs a smaller story range.
  - Do not expose internal diagnostics.
- 來源部分不可用造成旅程不完整時:
  - Render available valid items only if useful.
  - Do not render broken links.
  - Show child-friendly partial message.
- 不存在 slug:
  - Render `data-journey-not-found`
  - Provide normal anchors back to `/journeys` and `/`.

**詳情合約**

- 旅程內故事項目排序依 journey reference `SortOrder`，同值時依 stable id。
- 開始閱讀 action 必須指向第一筆有效故事 detail route。
- Story item links must be normal anchors; no JavaScript-only router.
- Reloading or revisiting the page MUST NOT restore or save progress/completion state.
- Theme changes may update visual appearance but MUST NOT change journey content, item order, language preference, history, scroll position, or route.

## 首頁入口合約: `GET /`

首頁必須包含清楚的 "學習旅程" / "Learning journeys" 入口:

- Uses a normal anchor to `/journeys`.
- Is discoverable in the primary home action area.
- Does not remove or change existing dinosaur, aquarium, `/explore`, `/compare`, language, or theme selector behavior.
- Does not add an extra theme selector.

## 探索頁入口合約: `GET /explore`

`/explore` 必須包含清楚的 "學習旅程" / "Learning journeys" 入口:

- Uses a normal anchor to `/journeys`.
- Does not change existing exploration search/filter state behavior.
- Does not add a theme selector.
- Does not turn exploration result links into JavaScript-only navigation.

## 既有故事詳情頁導覽合約

旅程故事項目與開始閱讀 action 必須只導向既有 routes:

- Dinosaur detail: `/dinosaurs/{slug}`
- Aquarium detail: `/aquarium/{slug}`

本功能不得新增 `/journeys/story/{slug}`、`/journeys/{journeySlug}/{storySlug}` 或其他旅程專用故事詳情頁。

## 語言與主題合約

- Default rendered text is `zh-TW`.
- Existing `storybook.language` controls page text; invalid or missing values fall back to `zh-TW`.
- Rendered bilingual elements expose `data-i18n-zh-tw` and `data-i18n-en` where language switching is needed.
- Page applies current effective theme via shared layout attributes.
- Journey pages must not render a theme selector.

## 整合測試期望

整合測試應驗證:

- `/journeys` returns `200 OK` and includes at least 3 journey cards in full catalog state.
- `/journeys/{slug}` returns `200 OK` for a known available journey and renders title, summary, goals, reading time, age guidance, start-reading link, story items, and back link.
- Homepage contains a normal link to `/journeys`.
- `/explore` contains a normal link to `/journeys`.
- Journey cards expose slugs, story counts, localized text, and links to `/journeys/{slug}`.
- Story item links point only to `/dinosaurs/{slug}` or `/aquarium/{slug}`.
- `/journeys` and `/journeys/{slug}` do not render a theme selector.
- Unknown journey slug renders `data-journey-not-found` without internal details.
- Not-enough and too-many journey fixtures render `data-journey-unavailable` and are hidden from `/journeys` list.
- Partial and all-source failure fixtures render friendly states without blank pages or sensitive diagnostics.
