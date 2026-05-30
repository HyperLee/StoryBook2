# Quickstart: 小小探險護照

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

   - Unit tests for `PassportPreferenceService` storage key/version/allowed sources/badge metadata.
   - Unit tests for `PassportCatalogService` total count, source ordering, route hrefs, source failure status and no blank fallback text.
   - Script contract tests for `passport.js` storage key, state version, de-duplication, clear scope and no cookie/session/history/fetch usage.
   - Integration tests for `/passport`, detail completion controls, navigation entry, theme selector absence and HTML data contract.

## Local Run

1. Start the app:

   ```bash
   dotnet run --project StoryBook/StoryBook.csproj
   ```

2. Open the URL printed by `dotnet run`, usually one of:

   - `http://localhost:5059`
   - `https://localhost:7111`

## Manual Acceptance

### 1. Find Passport Entry

1. Open `/`.
2. Confirm there is a visible「我的探險護照」entry in shared navigation and a home page action.
3. Activate it with mouse and keyboard.
4. Confirm the browser navigates to `/passport` through a normal link.

Expected: `/passport` opens, no theme selector appears, and the page shows an empty friendly state when no stories are complete.

### 2. Mark A Dinosaur Complete

1. Open `/dinosaurs/triceratops`.
2. Confirm story text, image, modal trigger, previous/next links and search/navigation behavior still work.
3. Activate「我讀完了」with keyboard.
4. Refresh the page.

Expected: The page still shows 三角龍 as read in a friendly status. Repeating the action does not increase the count twice.

### 3. Mark An Aquarium Story Complete

1. Open `/aquarium/sea-turtle`.
2. Activate「我讀完了」with mouse or touch.
3. Open `/passport`.

Expected: The read list contains links to `/dinosaurs/triceratops` and `/aquarium/sea-turtle`, the read count is 2 of the current total, and both links work with browser back/open-new-tab behavior.

### 4. Badge Milestones

1. Complete one story.
2. Complete three distinct stories.
3. Complete all dinosaur stories.
4. Complete all aquarium stories.

Expected: The five fixed badge milestones update from locked to unlocked only when their required counts are met: first story, 3 stories, all dinosaurs, all aquarium, all stories.

### 5. Clear Passport Only

1. Set language to English.
2. Set theme to dark mode on the home page.
3. Complete at least two stories.
4. Open `/passport`.
5. Activate clear passport and confirm.

Expected: Read list becomes empty, badge statuses reset, `storybook.passport` is removed or reset, `storybook.language` remains `en`, and `storybook.theme` remains `dark`.

### 6. Invalid Data Fallback

In DevTools console, run:

```javascript
localStorage.setItem("storybook.passport", "{\"version\":99,\"completedStories\":[{\"source\":\"bad\",\"slug\":\"missing\"}]}");
```

Reload `/passport`.

Expected: Page shows a child-friendly invalid/fallback message, no blank links, no internal exception details, and no invalid story link. Completing a valid story later rewrites `storybook.passport` to version 1 with only valid `{ source, slug }` items.

### 7. Storage Blocked Fallback

Use a browser profile or DevTools override that blocks localStorage access, then open:

- `/dinosaurs/triceratops`
- `/aquarium/sea-turtle`
- `/passport`

Expected: Story content remains readable. Completion controls and `/passport` show friendly text explaining the passport cannot be read or saved right now. No state is moved to URL, cookie, sessionStorage, server request or external service.

### 8. Language And Theme

1. Switch language to English.
2. Open `/passport`, `/dinosaurs/triceratops`, `/aquarium/sea-turtle`.
3. Switch back to 繁體中文.
4. Change theme mode between light, dark and system on the home page, then return to `/passport`.

Expected: Passport text, status messages, clear confirmation, badge text and completion controls follow the selected language with zh-TW fallback. The effective theme applies everywhere, and only the home page shows the theme selector.

### 9. Keyboard And Responsive Layout

Check widths 375px, 768px and 1366px.

1. Tab through the passport entry, completion button, passport link, read list links, clear button and confirmation controls.
2. Activate each control with Enter or Space where appropriate.
3. Inspect the page for horizontal overflow, overlapping text, clipped buttons or hidden focus rings.

Expected: All controls are focusable and activatable. Focus is visible. Text and controls do not overlap or overflow.

### 10. Privacy/Data Check

After completing several stories, inspect:

```javascript
localStorage.getItem("storybook.passport")
```

Expected: JSON contains only:

```json
{
  "version": 1,
  "completedStories": [
    { "source": "dinosaurs", "slug": "triceratops" }
  ]
}
```

No names, ages, class/school fields, free text, timestamps, titles, summaries, badges, theme values or language values are present.
