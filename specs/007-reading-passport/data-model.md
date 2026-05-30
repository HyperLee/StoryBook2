# Data Model: 小小探險護照

## ReadingPassportState

瀏覽器 `localStorage` key `storybook.passport` 中保存的根物件。此物件只存在於同一瀏覽器，不由伺服器讀取或保存。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `version` | `number` | Yes | state schema version；初始版本固定為 `1`。 |
| `completedStories` | `CompletedStoryItem[]` | Yes | 使用者明確標記讀完的故事項目集合。 |

### Validation Rules

- 根值必須是 JSON object，不可為 array、string、number 或 null。
- `version` 必須等於 `1`；缺漏或不支援版本視為 invalid state。
- `completedStories` 必須是 array；缺漏或非 array 視為 invalid state。
- invalid state 不得讓頁面崩潰；`passport.js` 顯示友善 fallback，並在下一次成功保存或清除時覆寫為有效 state。
- State 不得包含姓名、年齡、班級、學校、自由輸入文字、閱讀時間、標題快照、摘要快照、徽章快照、theme 或 language 偏好。

## CompletedStoryItem

一筆已讀故事紀錄。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `source` | `string` | Yes | 來源故事書 code，只允許 `dinosaurs` 或 `aquarium`。 |
| `slug` | `string` | Yes | 來源 catalog 中的既有故事 slug。 |

### Validation Rules

- `source` 僅允許 `dinosaurs`、`aquarium`。
- `slug` 必須符合 kebab-case pattern `^[a-z0-9]+(?:-[a-z0-9]+)*$`。
- 同一 `{ source, slug }` 在 normalized state 中只能出現一次。
- 若 item 指向目前 catalog 不存在的故事，`/passport` 不顯示失效連結，並在下次成功保存時移除該 item。
- Extra properties 在 normalization 時忽略，不得重新保存。

## PassportStoryItem

伺服器端由既有 catalog 產生、輸出到 `/passport` DOM 的可完成故事 projection。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `StableId` | `string` | Yes | `{source}:{slug}`，例如 `dinosaurs:triceratops`。 |
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `SourceCode` | `string` | Yes | localStorage 使用的小寫 code。 |
| `Slug` | `string` | Yes | 來源故事 slug。 |
| `SortOrder` | `int` | Yes | 來源內既有故事順序。 |
| `SourceSortOrder` | `int` | Yes | 來源故事書排序，恐龍先於水族館。 |
| `DetailHref` | `string` | Yes | `/dinosaurs/{slug}` 或 `/aquarium/{slug}`。 |
| `SourceLabelZhTW` | `string` | Yes | 繁中來源標籤。 |
| `SourceLabelEn` | `string` | Yes | 英文來源標籤。 |
| `NameZhTW` | `string` | Yes | 繁中故事朋友名稱。 |
| `NameEn` | `string` | Yes | 英文故事朋友名稱，缺漏時回退繁中。 |
| `SummaryZhTW` | `string` | Yes | 繁中摘要。 |
| `SummaryEn` | `string` | Yes | 英文摘要，缺漏時回退繁中。 |
| `ImagePath` | `string?` | No | 可選縮圖路徑；不是連結有效性的必要條件。 |
| `ImageAltTextZhTW` | `string?` | No | 顯示縮圖時的繁中 alt。 |
| `ImageAltTextEn` | `string?` | No | 顯示縮圖時的英文 alt，缺漏時回退繁中。 |

### Validation Rules

- `DetailHref` 必須是同站一般 anchor，且只指向既有 canonical detail route。
- 不得輸出空白標題、空白摘要、空白來源標籤或 unknown story link。
- 排序先依 `SourceSortOrder`，再依 `SortOrder`，最後依 `StableId`。

## PassportBadgeDefinition

固定徽章里程碑定義，由伺服器輸出，解鎖狀態由瀏覽器依已讀項目推導。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Code` | `string` | Yes | Stable badge code：`first-story`、`three-stories`、`all-dinosaurs`、`all-aquarium`、`all-stories`。 |
| `Milestone` | `PassportBadgeMilestone` | Yes | 判斷方式。 |
| `SourceCode` | `string?` | Conditional | `all-dinosaurs` 使用 `dinosaurs`，`all-aquarium` 使用 `aquarium`，全站 badge 為 null。 |
| `TargetCount` | `int?` | Conditional | `first-story` 為 1，`three-stories` 為 3；all-source badge 由 source total 推導。 |
| `SortOrder` | `int` | Yes | Badge 顯示順序。 |
| `LabelZhTW` | `string` | Yes | 繁中徽章名稱。 |
| `LabelEn` | `string` | Yes | 英文徽章名稱。 |
| `DescriptionZhTW` | `string` | Yes | 繁中說明。 |
| `DescriptionEn` | `string` | Yes | 英文說明。 |

### Validation Rules

- `Code` 必須唯一。
- 固定 badge 必須剛好 5 個。
- Label/description fallback 後不得為空。
- Badge unlock state 不得保存到 localStorage；每次由 normalized completed items 與 catalog totals 推導。

## PassportBadgeMilestone

Badge 判斷方式 enum。

| Value | Description |
|-------|-------------|
| `CompletedCountAtLeast` | 全部有效已讀 item 數量達到 `TargetCount`。 |
| `CompletedAllInSource` | 指定 `SourceCode` 的有效故事全部已讀。 |
| `CompletedAllStories` | 所有有效故事全部已讀。 |

## PassportCatalogSnapshot

`PassportCatalogService` 回傳給 PageModel 的目前護照資料快照。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Stories` | `IReadOnlyList<PassportStoryItem>` | Yes | 目前可被護照識別與連結的故事。 |
| `Badges` | `IReadOnlyList<PassportBadgeDefinition>` | Yes | 固定 badge 定義。 |
| `SourceStatuses` | `IReadOnlyList<PassportSourceStatus>` | Yes | 每個來源故事書的可用狀態。 |
| `TotalStoryCount` | `int` | Yes | `Stories.Count`。 |
| `HasAnyStory` | `bool` | Yes | 是否至少有一筆可探索故事。 |
| `HasPartialSourceFailure` | `bool` | Yes | 是否部分來源載入失敗但仍有故事可顯示。 |
| `HasAllSourcesFailed` | `bool` | Yes | 是否所有來源都無法載入。 |

### Failure Rules

- 單一來源失敗時，顯示其他可用來源故事與友善 partial message，不輸出該失敗來源的失效連結。
- 全部來源失敗時，`/passport` 顯示友善不可讀狀態與回故事入口 action。
- Source failure log 僅可包含 source code、reason code、count 等非敏感摘要。

## PassportSourceStatus

描述恐龍或水族館 catalog 在護照 projection 中的可用狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `SourceCode` | `string` | Yes | `dinosaurs` 或 `aquarium`。 |
| `IsAvailable` | `bool` | Yes | 是否成功載入並可提供故事。 |
| `StoryCount` | `int` | Yes | 此來源可用故事數；失敗時為 0。 |
| `FriendlyMessageZhTW` | `string?` | No | 來源失敗時顯示的繁中友善訊息。 |
| `FriendlyMessageEn` | `string?` | No | 來源失敗時顯示的英文友善訊息。 |
| `ReasonCode` | `string?` | No | 非敏感診斷 reason code。 |

## BrowserPassportViewState

`passport.js` 在執行期產生的頁面狀態，不保存到伺服器。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `storageStatus` | enum | Yes | `available`、`read-blocked`、`write-blocked`、`invalid-data`。 |
| `completedStories` | normalized item list | Yes | 已過濾、去重、只含目前 catalog 有效 item 的清單。 |
| `ignoredItemCount` | number | Yes | 因格式、來源、slug 或不存在而忽略的 item 數。 |
| `completedCount` | number | Yes | 有效已讀數。 |
| `totalCount` | number | Yes | DOM 中的有效故事總數。 |

### State Transitions

- `empty/no-key` -> `valid-empty`: localStorage 無資料時使用空 state 顯示鼓勵開始探索。
- `valid-empty` -> `completed`: 使用者在詳情頁按下完成控制並成功保存。
- `completed` -> `completed`: 同一故事再次操作，不新增重複 item，只顯示已讀狀態。
- `completed` -> `cleared`: 使用者在 `/passport` 確認清除，移除或重設 `storybook.passport`。
- `invalid-data` -> `valid-normalized`: 下一次成功保存時只寫回 valid、deduplicated、known items。
- `read-blocked` 或 `write-blocked` -> friendly degraded state: 故事內容與一般導覽維持可用，護照顯示暫時不能保存/讀取。
