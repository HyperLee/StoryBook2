# Data Model: 互動問答挑戰

## QuizCatalog

`StoryBook/Data/quiz-questions.json` 的根物件。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Version` | `int` | Yes | 題庫 schema version；MVP 固定為 `1`。 |
| `Questions` | `IReadOnlyList<QuizQuestion>` | Yes | 人工維護與審核的問答題目集合。 |

### Validation Rules

- 根物件不可為 null。
- `Version` 必須等於 `1`；不支援版本視為題庫不可用。
- `Questions` 不可為 null；完整資料情境下至少 12 題有效題目，且恐龍與水族館各至少 5 題有效題目。
- 題庫驗證失敗或部分題目無效時，不得在使用者頁面暴露檔案路徑、exception detail、stack trace、secret 或內部設定。

## QuizQuestion

一筆人工撰寫與審核的單選題。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | `string` | Yes | 全題庫唯一、穩定、kebab-case question id。 |
| `Source` | `ExplorationSourceType` or source code | Yes | 題目來源，只允許 `dinosaurs` 或 `aquarium`。 |
| `Difficulty` | `QuizDifficulty` | Yes | 題目難度，只允許 `easy` 或 `medium`。 |
| `SortOrder` | `int` | Yes | 同來源內穩定排序值。 |
| `Prompt` | `QuizText` | Yes | 雙語題幹。 |
| `Options` | `IReadOnlyList<QuizAnswerOption>` | Yes | 2-4 個可選答案。 |
| `CorrectOptionId` | `string` | Yes | 對應 `Options` 中唯一正確答案的 option id。 |
| `CorrectFeedback` | `QuizText` | Yes | 答對時的雙語鼓勵回饋。 |
| `IncorrectFeedback` | `QuizText` | Yes | 答錯時的雙語鼓勵回饋。 |
| `Explanation` | `QuizText` | Yes | 作答後顯示的雙語簡短解釋。 |
| `RelatedStories` | `IReadOnlyList<QuizStoryReference>` | Yes | 至少一筆既有故事複習連結。 |

### Validation Rules

- `Id` 必須符合 `^[a-z0-9]+(?:-[a-z0-9]+)*$`，且全題庫唯一。
- `Source` 僅允許 `dinosaurs`、`aquarium`；`all` 不得作為題目資料值。
- `Difficulty` 僅允許 `easy`、`medium`。
- `SortOrder` 必須為正整數；排序先依 source sort order，再依 `SortOrder`，最後依 `Id`。
- `Prompt`、`CorrectFeedback`、`IncorrectFeedback`、`Explanation` fallback 後不得為空。
- `Options` 必須有 2-4 筆；option id 必須在同題內唯一。
- `CorrectOptionId` 必須剛好對應一筆 option；多個正確答案或沒有正確答案都無效。
- `RelatedStories` 至少一筆，且每筆都必須能解析到既有恐龍或水族館詳情頁。
- 無效題目不得顯示給使用者，也不得產生失效故事連結。

## QuizAnswerOption

題目中的一個單選答案。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | `string` | Yes | 同題內唯一、穩定的 option id。 |
| `Text` | `QuizText` | Yes | 雙語答案文字。 |
| `SortOrder` | `int` | Yes | 顯示順序。 |

### Validation Rules

- `Id` 必須符合 `^[a-z0-9]+(?:-[a-z0-9]+)*$`。
- `Text` fallback 後不得為空。
- `SortOrder` 必須為正整數；同值時依 `Id` 穩定排序。
- 答案狀態不得只靠顏色呈現，UI 必須搭配文字或狀態提示。

## QuizText

問答題庫使用的雙語文字。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `ZhTW` | `string` | Yes | 預設繁體中文文字。 |
| `En` | `string` | Yes | 英文文字。 |

### Validation Rules

- `ZhTW` trim 後不得為空。
- `En` trim 後不得為空；若 runtime 遇到缺漏或空白，顯示層仍必須回退 `ZhTW`，不得顯示空白。
- 使用者可見文字必須適合 5-10 歲孩童理解，回饋不得羞辱、驚嚇或責備。

## QuizStoryReference

題目指向既有故事詳情頁的複習連結。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` or source code | Yes | 來源故事書，只允許 `dinosaurs` 或 `aquarium`。 |
| `Slug` | `string` | Yes | 對應來源 catalog 中的既有故事 slug。 |
| `SortOrder` | `int` | Yes | 同題內複習連結顯示順序。 |

### Validation Rules

- `Source` 必須等於題目 `Source` 或明確允許跨來源複習；MVP 預設同來源，跨來源只在題目內容明確關聯時使用。
- `Slug` 必須符合 kebab-case，並在對應來源 catalog 中存在。
- 同題不可重複引用相同 `{Source, Slug}`。
- 顯示名稱、來源 label 與 href 不存入此物件，必須由既有 catalog 與 `ExplorationSourceType` 衍生。

## QuizScope

問答頁使用者可選擇的題目集合。

| Value | Description |
|-------|-------------|
| `all` | UI 聚合篩選，包含所有有效恐龍與水族館題目。不得保存到題目資料。 |
| `dinosaurs` | 只顯示 `Source = dinosaurs` 的有效題目。 |
| `aquarium` | 只顯示 `Source = aquarium` 的有效題目。 |

### Validation Rules

- URL/form 中缺漏、空白或無效 scope 時，回退 `all`。
- Scope 選擇不得保存到 localStorage、cookie、session 或伺服器端使用者狀態。

## QuizDifficulty

題目難度 enum。

| Value | Description |
|-------|-------------|
| `easy` | 低門檻複習題，適合快速回想故事中的明確事實。 |
| `medium` | 需要比較、推論或連結故事細節的題目。 |

### Validation Rules

- 只有 `easy` 與 `medium` 是 MVP 合法值。
- 未知 difficulty 的題目不得顯示給使用者。

## QuizQuestionView

`QuizCatalogService` / PageModel 輸出給 Razor view 的安全顯示 projection。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | `string` | Yes | Question id。 |
| `Scope` | `QuizScope` | Yes | 目前 UI scope。 |
| `SourceCode` | `string` | Yes | `dinosaurs` 或 `aquarium`。 |
| `SourceLabelZhTW` | `string` | Yes | 繁中來源標籤。 |
| `SourceLabelEn` | `string` | Yes | 英文來源標籤。 |
| `Difficulty` | `QuizDifficulty` | Yes | 題目難度。 |
| `PromptZhTW` | `string` | Yes | 繁中題幹。 |
| `PromptEn` | `string` | Yes | 英文題幹。 |
| `Options` | `IReadOnlyList<QuizAnswerOptionView>` | Yes | 安全排序後的答案選項，不包含正確答案旗標。 |
| `RelatedStories` | `IReadOnlyList<QuizRelatedStoryView>` | Yes | 可開啟的站內複習連結。 |
| `NextQuestionHref` | `string` | Yes | 同 scope 下一題 URL；最後一題循環回第一題。 |

### Validation Rules

- View projection 不得輸出空白題幹、空白選項、空白來源標籤或失效連結。
- 初始未作答 DOM 不得暴露 `CorrectOptionId` 或正確答案旗標。
- `NextQuestionHref` 必須是一般站內 anchor，格式為 `/quiz?scope={scope}&questionId={id}`。

## QuizAnswerResult

作答提交後目前 response 中的 transient result。此物件不保存到 browser storage、URL 或 server storage。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `QuestionId` | `string` | Yes | 被作答的 question id。 |
| `SelectedOptionId` | `string?` | No | 使用者選擇的 option id；未選時為 null。 |
| `IsAnswered` | `bool` | Yes | 是否有有效選項被提交。 |
| `IsCorrect` | `bool?` | Conditional | 有效作答時表示正誤；未選時為 null。 |
| `FeedbackZhTW` | `string` | Yes | 繁中友善回饋或未選提示。 |
| `FeedbackEn` | `string` | Yes | 英文友善回饋或未選提示。 |
| `ExplanationZhTW` | `string?` | No | 有效作答後的繁中解釋。 |
| `ExplanationEn` | `string?` | No | 有效作答後的英文解釋。 |

### Validation Rules

- 未選答案提交時，`IsAnswered = false`，不得把未選視為答錯。
- 答錯回饋不得羞辱、驚嚇或責備。
- Result 不得出現在 query string、localStorage、sessionStorage、cookie、server-side file 或可分享位置。

## QuizCatalogSnapshot

`QuizCatalogService` 回傳給 PageModel 的目前題庫快照。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Questions` | `IReadOnlyList<QuizQuestion>` or projection | Yes | 所有有效題目。 |
| `SourceStatuses` | `IReadOnlyList<QuizSourceStatus>` | Yes | 恐龍與水族館來源可用狀態。 |
| `InvalidQuestionCount` | `int` | Yes | 被過濾的無效題目數。 |
| `HasAnyQuestion` | `bool` | Yes | 是否至少有一題有效題目。 |
| `HasPartialSourceFailure` | `bool` | Yes | 是否有部分故事來源不可用但仍有題目可顯示。 |
| `HasAllSourcesFailed` | `bool` | Yes | 是否所有來源都無法解析。 |

### Failure Rules

- 部分題目無效時，顯示其餘有效題目並記錄非敏感摘要。
- 某 scope 無有效題目時，顯示該 scope 友善空狀態。
- 全部題目或來源不可用時，`/quiz` 顯示友善不可開始狀態與返回首頁/故事入口 action。

## QuizSourceStatus

描述恐龍或水族館 catalog 在問答題庫解析中的可用狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `SourceCode` | `string` | Yes | `dinosaurs` 或 `aquarium`。 |
| `IsAvailable` | `bool` | Yes | 是否成功載入並可解析 related story references。 |
| `StoryCount` | `int` | Yes | 此來源可用故事數；失敗時為 0。 |
| `QuestionCount` | `int` | Yes | 此來源有效題目數。 |
| `FriendlyMessageZhTW` | `string?` | No | 來源失敗或無題時顯示的繁中友善訊息。 |
| `FriendlyMessageEn` | `string?` | No | 來源失敗或無題時顯示的英文友善訊息。 |
| `ReasonCode` | `string?` | No | 非敏感診斷 reason code。 |

## State Transitions

- `catalog-file-loaded` -> `schema-validated`: JSON 可解析且根物件/版本/題目集合初步有效。
- `schema-validated` -> `content-validated`: 每題必要欄位、選項、正確答案、difficulty 與雙語內容驗證完成。
- `content-validated` -> `stories-resolved`: related story references 解析到既有恐龍/水族館 catalog。
- `stories-resolved` -> `scope-ready`: 依 `all`、`dinosaurs` 或 `aquarium` 取得穩定排序題目序列。
- `scope-ready` -> `question-displayed`: `/quiz` 顯示目前題目、答案選項與相關故事連結。
- `question-displayed` -> `feedback-displayed`: 使用者提交有效答案後顯示正誤、鼓勵回饋與解釋。
- `question-displayed` -> `needs-selection`: 使用者未選答案就提交時顯示友善提示，不改變題目。
- `feedback-displayed` -> `next-question`: 使用一般 anchor 前往同 scope 下一題；最後一題循環回第一題。
- Any load/validation failure -> friendly empty/error state: 顯示兒童可理解訊息與返回首頁或故事入口 action。
