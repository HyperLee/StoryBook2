# Data Model: 主題學習旅程

## JourneyCatalog

`StoryBook/Data/journeys.json` 的根物件。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Journeys` | `IReadOnlyList<LearningJourney>` | Yes | 專案維護者策展的旅程定義集合。 |

### Validation Rules

- `Journeys` 不可為 null，且完整資料情境下至少包含 3 條可出發旅程。
- 旅程集合依 `SortOrder` 再依 `Slug` 穩定排序。
- 驗證失敗時不得在使用者頁面或新增 journey logs 中暴露檔案路徑、exception detail 或 secret。

## LearningJourney

一條主題學習旅程的資料定義。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Slug` | `string` | Yes | 全站唯一、穩定、kebab-case slug，用於 `/journeys/{slug}`。 |
| `SortOrder` | `int` | Yes | `/journeys` 列表排序，先依此值，再依 slug。 |
| `Title` | `JourneyText` | Yes | 雙語旅程標題；fallback 後不得為空。 |
| `Summary` | `JourneyText` | Yes | 雙語短簡介；fallback 後不得為空。 |
| `LearningGoals` | `IReadOnlyList<JourneyText>` | Yes | 1-3 筆雙語學習目標。 |
| `SuggestedReadingMinutes` | `int` | Yes | 建議閱讀分鐘數，用 localized template 顯示。 |
| `AgeGuidance` | `JourneyText` | Yes | 雙語建議年齡提示，例如「5-7 歲」。 |
| `StoryReferences` | `IReadOnlyList<JourneyStoryReference>` | Yes | 旅程內排序的既有故事引用。 |

### Validation Rules

- `Slug` 必須符合 `^[a-z0-9]+(?:-[a-z0-9]+)*$`，並在旅程集合中唯一。
- `SortOrder` 必須為正整數；重複時仍以 slug 穩定排序。
- `Title`、`Summary`、`AgeGuidance` 與每筆 `LearningGoals` fallback 到 `zh-TW` 後不得為空。
- `LearningGoals` 至少 1 筆，最多 3 筆。
- `SuggestedReadingMinutes` 必須大於 0，建議 MVP 介於 5-30 分鐘。
- `StoryReferences` 的資料定義可多於或少於 3-5 筆，但 projection 後的有效故事項目必須剛好落在 3-5 筆才可出現在 `/journeys` 列表。

## JourneyText

旅程專用雙語文字。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `ZhTW` | `string` | Yes | 預設繁體中文文字。 |
| `En` | `string?` | No | 英文文字；缺漏或空白時回退 `ZhTW`。 |

### Validation Rules

- `ZhTW` trim 後不得為空。
- `En` 可缺漏，但顯示層不得因此出現空白。
- `Get(LanguageCode)` 使用 `LanguageCode.ZhTW` 作為安全 fallback。

## JourneyStoryReference

旅程資料中保存的一筆既有故事引用。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` or source code | Yes | 來源故事書，目前只允許 `dinosaurs` 或 `aquarium`。 |
| `Slug` | `string` | Yes | 來源 catalog 內的既有故事 slug。 |
| `SortOrder` | `int` | Yes | 旅程內閱讀順序。 |

### Validation Rules

- `Source` 只允許 `dinosaurs`、`aquarium`；未知來源視為 invalid reference。
- `Slug` 必須符合 kebab-case，並在對應來源 catalog 中存在才是有效故事項目。
- 同一旅程不可重複引用相同 `{Source, Slug}`；重複項目不得產生重複卡片或重複連結。
- 顯示名稱、摘要與詳情 route 不得存放在此物件，必須由來源 catalog 衍生。

## JourneyStoryItem

已解析、可顯示於旅程詳情頁的故事項目 projection。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `StableId` | `string` | Yes | `{source}:{slug}`，例如 `dinosaurs:tyrannosaurus-rex`。 |
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `Slug` | `string` | Yes | 來源故事 slug。 |
| `DetailHref` | `string` | Yes | `/dinosaurs/{slug}` 或 `/aquarium/{slug}`。 |
| `SortOrder` | `int` | Yes | 旅程內排序。 |
| `SourceLabel` | bilingual text | Yes | 來源故事書名稱，依語言 fallback 顯示。 |
| `Name` | bilingual text | Yes | 來源故事顯示名稱，fallback 後不得為空。 |
| `Summary` | bilingual text | Yes | 來源故事摘要，fallback 後不得為空。 |
| `ImagePath` | `string?` | No | 可用於卡片縮圖；不是旅程有效性的必要條件。 |
| `ImageAltText` | bilingual text? | No | 若顯示圖片，fallback 後不得為空。 |

### Validation Rules

- `DetailHref` 必須使用一般 anchor 可直接開啟。
- 不得產生指向不存在故事的 link。
- 排序依 `SortOrder`，同值時依 `StableId` 穩定排序。

## JourneyAvailabilityStatus

描述一條旅程在目前來源資料狀態下是否可出發。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `JourneySlug` | `string` | Yes | 對應旅程 slug。 |
| `State` | enum | Yes | `available`、`not-found`、`not-enough-items`、`too-many-items`、`missing-required-text`、`source-unavailable`、`invalid-reference`。 |
| `ValidItemCount` | `int` | Yes | 解析後有效且去重的故事項目數。 |
| `CanAppearInList` | `bool` | Yes | 是否可出現在 `/journeys` 可選列表。 |
| `FriendlyMessage` | bilingual text | Yes when unavailable | 使用者可見的兒童友善提示。 |
| `DiagnosticSummary` | `JourneyDiagnosticSummary?` | No | 維護者可用的非敏感摘要。 |

### State Rules

- `available`: 有效故事項目數為 3-5，且必要文字 fallback 後不為空。
- `not-enough-items`: 有效故事項目少於 3；列表隱藏，詳情顯示暫時不能出發。
- `too-many-items`: 有效故事項目超過 5；列表隱藏，詳情顯示需要縮小旅程範圍。
- `source-unavailable`: 至少一個來源無法解析，且造成旅程不完整；不得產生失效連結。
- `not-found`: `/journeys/{slug}` 沒有對應旅程時使用。
- 所有 unavailable states 都不得顯示空白、內部例外、檔案路徑或 stack trace。

## JourneyCatalogSnapshot

`LearningJourneyCatalogService` 回傳給 PageModel 的目前旅程快照。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `AvailableJourneys` | `IReadOnlyList<LearningJourney>` or list projection | Yes | 可顯示於 `/journeys` 的完整旅程。 |
| `UnavailableStatuses` | `IReadOnlyList<JourneyAvailabilityStatus>` | Yes | 被隱藏或不可用的旅程狀態摘要。 |
| `SourceStatuses` | `IReadOnlyList<JourneySourceStatus>` | Yes | 恐龍與水族館來源可用狀態。 |
| `HasAnyAvailableJourney` | `bool` | Yes | 是否至少有一條可出發旅程。 |
| `HasPartialSourceFailure` | `bool` | Yes | 是否有部分來源失敗但仍有旅程可呈現。 |
| `HasAllSourcesFailed` | `bool` | Yes | 是否所有 configured sources 都不可用。 |

### Failure Rules

- 單一來源失敗：顯示可用旅程與 partial failure message；缺少有效項目的旅程依 status 隱藏或詳情提示。
- 沒有可出發旅程：`/journeys` 顯示友善錯誤/空狀態與回首頁 action。
- 全部來源失敗：`/journeys` 顯示友善錯誤狀態，不顯示空白列表。

## JourneySourceStatus

描述每個來源故事書在旅程解析中的可用狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `IsAvailable` | `bool` | Yes | 是否成功提供故事項目查找。 |
| `ItemCount` | `int` | Yes | 可查找的來源故事數；失敗時為 0。 |
| `FriendlyMessage` | bilingual text? | No | 來源失敗時顯示給使用者的兒童友善訊息。 |
| `DiagnosticSummary` | `JourneyDiagnosticSummary?` | No | 非敏感診斷摘要。 |

## JourneyDiagnosticSummary

只供 logging 與測試驗證的非敏感診斷摘要。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `ReasonCode` | `string` | Yes | 穩定 reason code，例如 `invalid-reference`、`source-load-failed`。 |
| `JourneySlug` | `string?` | No | 發生問題的旅程 slug。 |
| `SourceCode` | `string?` | No | 發生問題的來源 code。 |
| `ReferenceSlug` | `string?` | No | 發生問題的來源故事 slug。 |
| `Count` | `int?` | No | 可用於記錄項目數或錯誤數。 |

### Safety Rules

- 不得包含檔案系統路徑、exception type、exception message、stack trace、secret、token、connection string、使用者識別資訊或閱讀狀態。
- 可寫入 `ILogger<T>`，也可在測試中檢查格式。

## State Transitions

- `catalog-loaded` -> `validated`: JSON 可解析且基本 schema/內容驗證完成。
- `validated` -> `sources-resolved`: 恐龍與水族館來源成功或部分成功載入查找表。
- `sources-resolved` -> `available`: 單一旅程有 3-5 筆有效、去重、可連結故事項目。
- `sources-resolved` -> `unavailable`: 單一旅程有效項目少於 3、超過 5、必要文字缺漏或引用無效。
- `unavailable` -> detail friendly state: 直接開啟該 slug 時顯示不可用提示與返回動作。
- `unknown slug` -> `not-found`: 顯示找不到旅程提示與返回旅程列表/首頁動作。
