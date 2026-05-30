# UI Contract: 互動問答挑戰

## Routes

| Route | Page | Status | Contract |
|-------|------|--------|----------|
| `/quiz` | `Pages/Quiz/Index` | 200 | 顯示問答挑戰標題、scope 選擇、目前題目、答案選項、提交控制、友善提示、相關故事連結與下一題動作。 |
| `/quiz?scope={scope}` | `Pages/Quiz/Index` | 200 | 以 `all`、`dinosaurs` 或 `aquarium` 篩選有效題目；無效 scope 回退 `all`。 |
| `/quiz?scope={scope}&questionId={questionId}` | `Pages/Quiz/Index` | 200 | 顯示指定 scope 中的有效題目；未知 question id 回退該 scope 第一題並顯示友善提示。 |
| `/quiz?handler=Answer` | `Pages/Quiz/Index.OnPostAnswer` | 200 | 透過 antiforgery form post 評估目前單題答案，同頁顯示正誤/未選提示、解釋與下一題 link。 |
| `/` | `Pages/Index` | 200 | 至少提供一個前往 `/quiz` 的一般 anchor 問答入口。 |
| `/explore` | `Pages/Explore/Index` | 200 | 可提供前往 `/quiz` 的一般 anchor 問答入口；若首頁已提供，探索頁入口仍建議加入以符合跨站探索流程。 |
| `/dinosaurs/{slug}` | `Pages/Dinosaurs/Details` | 200/404 | 題目相關故事連結指向既有恐龍詳情頁；問答功能不得建立重複故事頁。 |
| `/aquarium/{slug}` | `Pages/Aquarium/Details` | 200/404 | 題目相關故事連結指向既有水族館詳情頁；問答功能不得建立重複故事頁。 |

## Navigation Contract

- 問答入口使用一般 anchor：`href="/quiz"` 或 Tag Helper 指向 `asp-page="/Quiz/Index"`。
- Scope 選擇使用一般 anchor，例如 `/quiz?scope=dinosaurs`、`/quiz?scope=aquarium`、`/quiz?scope=all`。
- 下一題使用一般 anchor，例如 `/quiz?scope=dinosaurs&questionId=triceratops-horns`；目前 scope 最後一題循環回同 scope 第一題。
- 相關故事連結使用站內 canonical route：`/dinosaurs/{slug}` 或 `/aquarium/{slug}`。
- 不得新增 `/quiz/{id}`、`/api/quiz`、JavaScript-only router、hash router、外部百科連結或即時翻譯 endpoint。

## Quiz Page DOM Contract

`/quiz` 必須輸出可由整合測試與漸進增強 script 驗證的穩定 metadata。

```html
<main data-quiz-page
      data-quiz-scope="dinosaurs"
      data-quiz-current-question-id="triceratops-horns"
      data-quiz-language-storage-key="storybook.language">
  <nav data-quiz-scope-nav aria-label="問答範圍">
    <a href="/quiz?scope=all" data-quiz-scope-option="all">全部故事</a>
    <a href="/quiz?scope=dinosaurs" data-quiz-scope-option="dinosaurs">恐龍</a>
    <a href="/quiz?scope=aquarium" data-quiz-scope-option="aquarium">水族館</a>
  </nav>

  <section data-quiz-question
           data-quiz-question-id="triceratops-horns"
           data-quiz-source="dinosaurs"
           data-quiz-difficulty="easy">
    <p data-quiz-source-label
       data-i18n-zh-tw="恐龍"
       data-i18n-en="Dinosaurs">恐龍</p>
    <h1 data-quiz-prompt
        data-i18n-zh-tw="三角龍頭上有什麼特別的地方？"
        data-i18n-en="What is special on a Triceratops head?">
      三角龍頭上有什麼特別的地方？
    </h1>

    <form method="post" asp-page-handler="Answer" data-quiz-answer-form>
      <input type="hidden" name="scope" value="dinosaurs" />
      <input type="hidden" name="questionId" value="triceratops-horns" />
      <fieldset>
        <legend>選一個答案</legend>
        <label>
          <input type="radio" name="selectedOptionId" value="three-horns" />
          三根角和大頭盾
        </label>
      </fieldset>
      <button type="submit" data-quiz-submit>送出答案</button>
    </form>
  </section>

  <section data-quiz-feedback role="status" aria-live="polite"></section>

  <section data-quiz-related-stories>
    <a href="/dinosaurs/triceratops" data-quiz-related-story>去讀三角龍故事</a>
  </section>

  <a href="/quiz?scope=dinosaurs&amp;questionId=tyrannosaurus-teeth"
     data-quiz-next-question>下一題</a>
</main>
```

### Page Rules

- 初始未作答 HTML 不得輸出正確答案旗標、`correctOptionId`、score、answer count 或 progress count。
- Answer form 必須包含 antiforgery token。
- 使用者未選答案即提交時，feedback region 顯示「請先選一個答案」類友善提示，且不得視為答錯。
- 作答後 feedback 必須包含正確/錯誤文字與簡短解釋；狀態不得只靠顏色傳達。
- 作答結果只存在目前 response/view state，不得寫入 query string、localStorage、sessionStorage、cookie、server-side file 或外部服務。
- 快速連續提交時，畫面以最後一次有效 server response 為準，不顯示互相矛盾的回饋。
- `/quiz` 不得顯示 theme selector。

## Scope And Empty State Contract

- `data-quiz-scope` 只可為 `all`、`dinosaurs`、`aquarium`。
- `all` scope 聚合所有有效題目，但題目 `data-quiz-source` 仍只能是 `dinosaurs` 或 `aquarium`。
- 某 scope 無有效題目時，頁面顯示該 scope 的友善空狀態與返回全部故事/首頁 action。
- 全部題庫不可用時，頁面顯示問答暫時不能開始的友善狀態，不顯示空白頁、內部例外、檔案路徑或 diagnostics。

## Related Story Contract

每個有效題目至少輸出一個 related story anchor。

```html
<a href="/aquarium/sea-turtle"
   data-quiz-related-story
   data-quiz-related-source="aquarium"
   data-quiz-related-slug="sea-turtle"
   data-i18n-zh-tw="去讀海龜故事"
   data-i18n-en="Read the Sea Turtle story">
  去讀海龜故事
</a>
```

### Related Story Rules

- `data-quiz-related-source` 只可為 `dinosaurs` 或 `aquarium`。
- href 必須指向既有 canonical detail route。
- 找不到或載入失敗的故事 reference 不得輸出為 anchor。
- 多個 related stories 必須各自有清楚名稱與來源標籤。

## Language And Theme Contract

- Language storage key remains `storybook.language`.
- Invalid or missing language falls back to `zh-TW`.
- 問答頁所有 user-visible text、aria-label、scope label、option text、feedback、explanation、empty/error state 與 related story label 都必須有 zh-TW 與 en 值或 zh-TW fallback path。
- Existing layout theme attributes remain present: `data-bs-theme`, `data-storybook-theme-mode`, `data-storybook-effective-theme`.
- `/quiz` applies effective theme via existing CSS tokens and `theme.js`, but does not render a theme selector.

## Accessibility Contract

- Scope options, answer radios, submit button, next question link and related story links have accessible names.
- Answer options are grouped in a semantic `fieldset` with a visible or screen-reader-accessible `legend`.
- Feedback updates use `role="status"` or `aria-live="polite"`.
- Keyboard users can focus and activate every control; focus remains visible after form post and validation messages.
- Touch/click targets are at least 44x44 CSS px.
- Layout must avoid horizontal overflow and overlapping controls at 375px, 768px and 1366px widths.

## Prohibited Client Behavior

Quiz behavior must not rely on:

- `localStorage` or `sessionStorage` for answer state
- `document.cookie`
- `fetch` or `XMLHttpRequest`
- `history.pushState` or `history.replaceState`
- jQuery selectors/events for core behavior
- external scripts, external styles, external APIs or external content lookup
