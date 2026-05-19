# Data Model: 內容比較器

## ComparisonCandidate

一筆可在 `/compare` 被選入比較器的故事朋友 projection，來源於既有恐龍或水族館 catalog。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `StableId` | `string` | Yes | 穩定識別，格式為 `{sourceType}:{slug}`，例如 `dinosaurs:tyrannosaurus-rex`。 |
| `Source` | `ExplorationSourceType` | Yes | `dinosaurs` 或 `aquarium`。 |
| `Slug` | `string` | Yes | 來源 catalog 的 kebab-case slug；只需在各來源內唯一。 |
| `DetailHref` | `string` | Yes | 既有詳情頁路徑：`/dinosaurs/{slug}` 或 `/aquarium/{slug}`。 |
| `SortOrder` | `int` | Yes | 來源 catalog 既有 `SortOrder`。 |
| `SourceLabel` | bilingual text | Yes | 來源故事書顯示名稱，依目前語言顯示並回退 `zh-TW`。 |
| `Name` | bilingual text | Yes | 顯示名稱，fallback 後不得為空。 |
| `Summary` | bilingual text | Yes | 短摘要，fallback 後不得為空。 |
| `Diet` | bilingual text | Yes | 食性文字，fallback 後不得為空。 |
| `LivingArea` | `ComparisonFieldValue` | Yes | 水族館使用 habitat/living area；恐龍使用 not-applicable。 |
| `Period` | `ComparisonFieldValue` | Yes | 恐龍使用生活時期；水族館使用 not-applicable。 |
| `DiscoveryLocations` | bilingual text | Yes | 發現地點或常見分布文字，fallback 後不得為空。 |
| `ImagePath` | `string?` | No | 若比較器顯示圖片時使用；圖片不是比較核心資訊。 |
| `ImageAltText` | bilingual text? | No | 若顯示圖片，fallback 後不得為空。 |

### Validation Rules

- `StableId` 必須在候選集合中唯一，且由 source code 與 slug 組成，不另建全站 slug。
- `DetailHref` 必須是一般 anchor 可直接開啟的既有 canonical detail route。
- 預設候選排序先依 `Source.GetSortOrder()`，再依來源 catalog `SortOrder`。
- 顯示名稱、摘要、食性、發現地點與來源標籤使用既有 bilingual fallback；fallback 後不得為空。
- 不適用欄位不得使用空字串、`null`、`undefined` 或內部欄位名稱。

## ComparisonFieldDefinition

比較表中的固定欄位定義。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Code` | `string` | Yes | 欄位代碼，例如 `source`、`name`、`diet`、`living-area`、`period`、`discovery-location`、`summary`、`detail-link`。 |
| `Label` | bilingual text | Yes | 使用者可見欄位名稱。 |
| `SortOrder` | `int` | Yes | 比較表列順序。 |
| `NotApplicableText` | bilingual text | Yes | 來源不適用時的兒童友善文字。 |

### Validation Rules

- 每列只比較一個欄位。
- 欄位 label 與 not-applicable text 需支援 `zh-TW` 與 `en`，缺漏時回退 `zh-TW`。
- 手機寬度可改為上下排列或卡片式排版，但欄位順序不可改變語意。

## ComparisonFieldValue

某候選在某比較欄位中的顯示值。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `State` | enum | Yes | `available` 或 `not-applicable`。 |
| `Text` | bilingual text | Yes | 可用值或不適用文字。 |
| `Href` | `string?` | No | 只有 `detail-link` 欄位需要，一律指向既有詳情頁。 |

### Validation Rules

- `available` 狀態的 `Text` fallback 後不得為空。
- `not-applicable` 狀態必須顯示兒童友善文字，不顯示空格或技術用語。
- `detail-link` 必須是一般 `<a>`，不得由 JavaScript-only router 取代。

## ComparisonCatalogSnapshot

`ComparisonCatalogService` 回傳給 PageModel 的不可變比較資料快照。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Candidates` | `IReadOnlyList<ComparisonCandidate>` | Yes | 目前可比較的候選集合。 |
| `FieldDefinitions` | `IReadOnlyList<ComparisonFieldDefinition>` | Yes | 固定比較欄位。 |
| `SourceStatuses` | `IReadOnlyList<ComparisonSourceStatus>` | Yes | 各來源可用狀態。 |
| `HasEnoughCandidates` | `bool` | Yes | 是否至少有兩筆候選可開始比較。 |
| `HasPartialFailure` | `bool` | Yes | 是否有部分來源失敗但仍有候選可用。 |
| `HasAllSourcesFailed` | `bool` | Yes | 是否所有 configured sources 都不可用。 |

### Failure Rules

- 單一來源失敗：顯示可用來源候選與 partial failure message，並記錄 failure。
- 可用候選少於兩筆：顯示「需要至少兩位故事朋友才能比較」的友善狀態。
- 全部來源失敗：顯示友善錯誤狀態與返回首頁 action，不顯示空白頁。
- 使用者可見錯誤不得暴露檔案路徑、exception type 或 stack trace。

## ComparisonSourceStatus

描述每個來源 catalog 在比較頁中的可用狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `IsAvailable` | `bool` | Yes | 此來源是否成功提供候選。 |
| `CandidateCount` | `int` | Yes | 可用候選筆數；失敗時為 0。 |
| `FriendlyMessage` | bilingual text | No | 來源失敗時顯示給使用者的兒童友善訊息。 |

## ComparisonSelectionState

使用者目前在 `/compare` 頁面生命週期內的選擇狀態，由 `compare.js` 在 DOM 記憶體中維護。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `FirstCandidateId` | `string?` | No | 第一位故事朋友的 `StableId`。 |
| `SecondCandidateId` | `string?` | No | 第二位故事朋友的 `StableId`。 |
| `Status` | enum | Yes | `empty`、`one-selected`、`duplicate`、`ready`、`not-enough-candidates`。 |
| `Language` | `LanguageCode` | Yes | 目前顯示語言；無效時回退 `zh-TW`。 |

### State Rules

- 初始狀態為 `empty`，不顯示比較表。
- 只選一位時狀態為 `one-selected`，提示再選一位。
- 兩個選擇相同時狀態為 `duplicate`，顯示友善提示且不得顯示比較表。
- 兩個不同候選都存在時狀態為 `ready`，顯示比較表。
- 清除選擇回到 `empty`。
- 狀態不得寫入 URL query、browser history、localStorage、sessionStorage、cookie 或 server state。
