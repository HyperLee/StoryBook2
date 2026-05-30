# Quickstart: 互動問答挑戰

## Automated Checks

1. Restore and build:

   ```bash
   dotnet restore StoryBook2.sln
   dotnet build StoryBook2.sln
   ```

2. Run the full test suite:

   ```bash
   dotnet test StoryBook2.sln
   ```

3. Expected coverage from tasks:

   - Unit tests for `QuizContentValidator` question id uniqueness, source enum, difficulty enum, bilingual text, option count, unique correct answer, related story reference shape and minimum valid question counts.
   - Unit tests for `QuizCatalogService` JSON loading, caching, stable sorting, scope filtering, source status, related story resolution, invalid question filtering, no blank fallback text and next-question cycling.
   - Unit tests for answer evaluation: correct answer, wrong answer, unknown option, no option selected and no persistent answer state.
   - Integration tests for `/quiz`, scope links, answer form post with antiforgery, no-selection friendly prompt, feedback/explanation rendering, related story anchors, homepage/explore entry, theme selector absence and friendly empty/error states.
   - Script contract tests for `quiz.js` if present: no jQuery dependency, no `localStorage`/`sessionStorage`/cookie answer persistence, no fetch/API, no History API mutation, no timer/countdown, no drag/drop or precision gesture dependency, and no correctness calculation in client-only code.

## Local Run

1. Start the app:

   ```bash
   dotnet run --project StoryBook/StoryBook.csproj
   ```

2. Open the URL printed by `dotnet run`, usually one of:

   - `http://localhost:5059`
   - `https://localhost:7111`

## Manual Acceptance

## Implementation Evidence

- 2026-05-30 US1 automated route and scope checkpoint: `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter "FullyQualifiedName~Quiz"` passed after adding the `/quiz` service, route, starter catalog, home entry, explore entry, scope links, question DOM metadata, and next-question projection.
- 2026-05-30 US2 automated answer-flow checkpoint: `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter "FullyQualifiedName~Quiz"` passed after adding transient answer evaluation, antiforgery answer posts, correct/wrong/no-selection feedback, explanations, next-question links, and no score/progress UI.
- 2026-05-30 US3 automated related-story checkpoint: `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter "FullyQualifiedName~Quiz"` passed after resolving related story references through existing story catalogs, rejecting duplicate/missing references, and rendering canonical review links with source/slug metadata.
- 2026-05-30 US4 automated validation/fallback checkpoint: `dotnet test StoryBook.Tests/StoryBook.Tests.csproj --filter "FullyQualifiedName~Quiz"` passed after adding full content validation, invalid-question filtering, source failure statuses, quiz fixture catalog replacement, friendly empty/error states, and sanitized output checks.

### Manual Acceptance Evidence Checklist

- [ ] `/quiz` route, home entry and explore/shared entry verified.
- [ ] Scope selection verified for 全部故事, 恐龍 and 水族館.
- [ ] Correct answer flow shows friendly correct feedback, explanation, next question and related story links within 1 second.
- [ ] Wrong answer flow shows non-shaming feedback, explanation and review link.
- [ ] No-selection submit shows friendly prompt and does not mark the answer wrong.
- [ ] Last question in each scope cycles to that scope's first question.
- [ ] Invalid/missing question data and unavailable story references do not render broken questions or links.
- [ ] Language, theme, keyboard and responsive layout checks recorded.
- [ ] Privacy/data inspection confirms answer state is not stored in URL, localStorage, sessionStorage, cookie or server-side file.
- [ ] Warm-load checks for `/quiz`, one dinosaur detail link and one aquarium detail link recorded.
- [ ] Performance smoke evidence recorded for `/quiz`, scope navigation, next-question navigation and answer feedback timing.
- [ ] SC-001 evidence recorded: at least 20 quiz-entry discovery or equivalent browser task records, with at least 19 successful entries within 5 seconds.
- [ ] SC-010 evidence recorded: at least 10 explanation-learning records, with at least 9 successful key learning points.

### 1. Find Quiz Entry

1. Open `/`.
2. Confirm there is a visible「問答挑戰」entry.
3. Open `/explore`.
4. Confirm there is a visible「問答挑戰」entry.
5. Activate both quiz entries with mouse and keyboard.

Expected: Browser navigates to `/quiz` through a normal link. The page shows scope options and either a first valid question or a friendly unavailable state. No theme selector appears.

### 2. Select Scopes

1. Open `/quiz`.
2. Select「全部故事」.
3. Select「恐龍」.
4. Select「水族館」.

Expected: Each scope uses a normal link and displays a valid question from the selected range. `全部故事` may show either dinosaur or aquarium questions. 恐龍 scope only shows dinosaur questions; 水族館 scope only shows aquarium questions.

### 3. Correct Answer

1. Open a known valid quiz question.
2. Select the correct option.
3. Submit the form.

Expected: The page shows friendly correct feedback and a short explanation within 1 second. It also shows at least one related story link and a next-question action. No cumulative score, progress count or answer statistics appear.

### 4. Wrong Answer

1. Open a valid quiz question.
2. Select a wrong option.
3. Submit the form.

Expected: The page shows a kind incorrect feedback message without blame or scary wording, plus the explanation and a related story link for review. The answer state is not saved after reload or navigation away.

### 5. No Selection

1. Open a valid quiz question.
2. Submit without selecting any option.

Expected: The page shows a friendly「請先選一個答案」style prompt. It does not mark the question wrong and does not show contradictory feedback.

### 6. Related Story Links

1. Submit an answer or inspect the current question.
2. Activate each related story link.
3. Use browser Back to return to the quiz.

Expected: Dinosaur references open `/dinosaurs/{slug}` and aquarium references open `/aquarium/{slug}`. Links are normal anchors and can be opened in a new tab. No broken story link appears.

### 7. Next Question Cycle

1. Navigate through questions in `?scope=dinosaurs`.
2. Continue until the final dinosaur question.
3. Activate「下一題」.
4. Repeat for `?scope=aquarium`.

Expected: The next action cycles to the first question in the same scope. The URL may include `scope` and `questionId`, but not selected answer or correctness.

### 8. Data Fallback

Use a test configuration or fixture that points `QuizCatalogOptions.ContentPath` to a catalog with:

- fewer than 2 options
- more than 4 options
- missing correct option
- invalid difficulty
- missing bilingual prompt or explanation
- related story slug that does not exist

Expected: Invalid questions are not displayed. If no valid questions remain, `/quiz` shows a child-friendly unavailable state and does not show internal exception details, file paths, stack traces or diagnostics.

### 9. Language And Theme

1. Switch language to English.
2. Open `/quiz` and answer a question.
3. Switch back to 繁體中文.
4. Change theme mode between light, dark and system on the home page, then return to `/quiz`.

Expected: Scope labels, prompt, options, feedback, explanation, no-selection prompt, next action and related story labels follow the selected language with zh-TW fallback. The effective theme applies, and only the home page shows the theme selector.

### 10. Keyboard And Responsive Layout

Check widths 375px, 768px and 1366px.

1. Tab through scope options, answer options, submit, next question and related story links.
2. Select radio options with keyboard.
3. Activate submit and next controls with Enter or Space where appropriate.
4. Inspect the page for horizontal overflow, overlapping text, clipped buttons or hidden focus rings.

Expected: All controls are focusable and activatable. Focus is visible. Touch/click targets are at least 44x44 CSS px. Text and controls do not overlap or overflow.

Also inspect the core quiz flow for interaction constraints:

1. Confirm answering, submitting, reviewing feedback and moving to the next question do not require drag/drop.
2. Confirm there is no countdown, time limit or timer-gated answer flow.
3. Confirm the flow can be completed with keyboard and ordinary click/tap targets, without precision gestures.

Expected: The core quiz flow remains usable with keyboard, mouse and touch, and does not depend on drag, timing pressure or fine motor precision.

### 11. Privacy/Data Check

After answering several questions, inspect browser storage and URL:

```javascript
localStorage.getItem("storybook.quiz")
sessionStorage.getItem("storybook.quiz")
document.cookie
location.href
```

Expected: No quiz answer result, score, selected option, correct/incorrect state or history is stored in localStorage, sessionStorage, cookies or shareable URL. Existing `storybook.language`, `storybook.theme` and `storybook.passport` keys remain unchanged by answering quiz questions.

### 12. Performance And Outcome Evidence

Record local warm-load timing with browser DevTools, automated browser task logs or equivalent local evidence:

1. Load `/quiz` at least 10 times after the first warm-up load and record FCP/LCP plus server or task timing when available.
2. Navigate scope links and next-question links at least 10 times each.
3. Submit at least 10 answer forms and record time from submit activation to visible feedback/explanation.

Expected: `/quiz` Page handler and catalog projection p95 is below 200ms when server timing is available. FCP is below 1.5 seconds, LCP below 2.5 seconds, feedback/explanation appears within 1 second, and scope/next navigation responds within 1 second.

Record SC-001 entry-discovery evidence:

1. Use at least 20 representative manual attempts or equivalent browser task records.
2. Include attempts from both `/` and `/explore`.
3. For each attempt, record source page, elapsed time to reach `/quiz`, and pass/fail.

Expected: At least 19 of 20 attempts reach `/quiz` within 5 seconds.

Record SC-010 explanation-learning evidence:

1. Use at least 10 representative manual attempts or equivalent task records.
2. After answer feedback appears, record one key learning point repeated or selected from the explanation.
3. Mark each attempt pass/fail based on whether the recorded point matches the explanation.

Expected: At least 9 of 10 attempts record a valid key learning point.
