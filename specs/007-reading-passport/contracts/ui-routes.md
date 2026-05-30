# UI Contract: 小小探險護照

## Routes

| Route | Page | Status | Contract |
|-------|------|--------|----------|
| `/passport` | `Pages/Passport/Index` | 200 | 顯示護照摘要、已讀數/總數、badge milestones、已讀清單、清除控制、empty/error/fallback states。 |
| `/dinosaurs/{slug}` | `Pages/Dinosaurs/Details` | 200/404 | 對有效故事顯示完成閱讀控制；找不到故事時不得顯示可保存的 completion control。 |
| `/aquarium/{slug}` | `Pages/Aquarium/Details` | 200/404 | 對有效故事顯示完成閱讀控制；找不到或資料載入失敗時不得顯示可保存的 completion control。 |
| `/` | `Pages/Index` and shared layout | 200 | 首頁內容與共用導覽都提供前往 `/passport` 的一般 anchor。 |

## Navigation Contract

- 護照入口使用一般 anchor：`href="/passport"` 或 Tag Helper 指向 `asp-page="/Passport/Index"`。
- 護照中的恐龍故事 item 使用 `href="/dinosaurs/{slug}"`。
- 護照中的水族館故事 item 使用 `href="/aquarium/{slug}"`。
- 不得新增 `/passport/{source}/{slug}`、`/api/passport`、query string completion route、hash router 或 JavaScript-only navigation。

## Detail Completion Control

有效詳情頁必須輸出一個 completion region。

```html
<section
  data-passport-story
  data-passport-source="dinosaurs"
  data-passport-slug="triceratops">
  <button
    type="button"
    data-passport-complete
    aria-label="把三角龍蓋到我的探險護照"
    data-aria-label-zh-tw="把三角龍蓋到我的探險護照"
    data-aria-label-en="Stamp Triceratops in my passport"
    data-i18n-zh-tw="我讀完了"
    data-i18n-en="I finished reading">
    我讀完了
  </button>
  <p data-passport-status role="status" aria-live="polite"></p>
  <a href="/passport" data-passport-link>我的探險護照</a>
</section>
```

### Detail Rules

- `data-passport-source` 只可為 `dinosaurs` 或 `aquarium`。
- `data-passport-slug` 必須是目前有效故事 slug。
- Button click 是唯一保存觸發；page load、scroll、time-on-page、image modal open 都不得保存。
- 已讀故事再次點擊時，state 不新增重複 item，status 顯示已經讀完。
- Storage 讀寫失敗時，故事內容、圖片、搜尋、pager 與一般導覽仍可用。

## Passport Page DOM Contract

`/passport` 必須輸出可由 `passport.js` 讀取的 metadata，不把個人閱讀狀態寫入 HTML。

```html
<section
  data-passport-page
  data-passport-storage-key="storybook.passport"
  data-passport-version="1"
  data-passport-language-storage-key="storybook.language">
  <p data-passport-summary role="status" aria-live="polite"></p>
  <ol data-passport-read-list></ol>
  <div data-passport-empty></div>
  <div data-passport-storage-warning hidden></div>
  <button type="button" data-passport-clear>清除護照</button>
  <div data-passport-clear-confirm hidden></div>
</section>
```

Each catalog story item must be present as a template/source node:

```html
<li
  data-passport-story-item
  data-passport-story-id="dinosaurs:triceratops"
  data-passport-source="dinosaurs"
  data-passport-slug="triceratops"
  data-passport-href="/dinosaurs/triceratops"
  data-passport-source-order="1"
  data-passport-story-order="2"
  data-i18n-name-zh-tw="三角龍"
  data-i18n-name-en="Triceratops"
  data-i18n-summary-zh-tw="..."
  data-i18n-summary-en="..."
  data-i18n-source-zh-tw="恐龍"
  data-i18n-source-en="Dinosaurs">
</li>
```

Each badge must be present as a shell:

```html
<article
  data-passport-badge
  data-passport-badge-code="all-dinosaurs"
  data-passport-badge-milestone="CompletedAllInSource"
  data-passport-source="dinosaurs">
  <h2 data-passport-badge-label></h2>
  <p data-passport-badge-description></p>
  <p data-passport-badge-status></p>
</article>
```

### Passport Page Rules

- Page renders default zh-TW text before JavaScript runs.
- `passport.js` may hide/show DOM regions and fill read list, but must not remove canonical anchors from generated list items.
- Empty state appears when normalized completed item count is 0.
- Storage read-blocked state appears when localStorage cannot be read.
- Invalid data state appears when localStorage JSON is malformed, version unsupported, or shape invalid.
- Clear confirmation must require an explicit confirmation click before removing/replacing `storybook.passport`.
- Clear action must not call `localStorage.clear()` and must not remove `storybook.language` or `storybook.theme`.
- Page must not contain `data-theme-selector`, `data-theme-option`, login controls, external links, `fetch`, `XMLHttpRequest`, `sessionStorage`, `document.cookie`, `pushState` or `replaceState` for passport behavior.

## Language And Theme Contract

- Language storage key remains `storybook.language`.
- Invalid or missing language falls back to `zh-TW`.
- All user-visible passport text must have zh-TW and en values or a zh-TW fallback path.
- Existing layout theme attributes remain present: `data-bs-theme`, `data-storybook-theme-mode`, `data-storybook-effective-theme`.
- `/passport` applies effective theme via existing CSS tokens and `theme.js`, but does not render a theme selector.

## Accessibility Contract

- Completion button, passport entry, clear control, confirmation controls and story links have accessible names.
- Status messages use `role="status"` or `aria-live="polite"` where they update after user action.
- Keyboard users can focus and activate all controls.
- Focus remains visible and is not trapped by the clear confirmation UI.
- Layout must avoid horizontal overflow and overlapping controls at 375px, 768px and 1366px widths.
