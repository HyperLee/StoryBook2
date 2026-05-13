# Data Model: 全站探索與分類搜尋

## ExplorationItem

一筆可在 `/explore` 顯示與搜尋的故事內容 projection，來源於既有恐龍或水族館 catalog。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `StableId` | `string` | Yes | 全站唯一識別，格式為 `{source}:{slug}`，例如 `dinosaurs:tyrannosaurus-rex`。 |
| `Source` | `ExplorationSourceType` | Yes | `dinosaurs` 或 `aquarium`。 |
| `SourceLabel` | bilingual text | Yes | 來源故事書顯示名稱，依目前語言顯示並回退 `zh-TW`。 |
| `Slug` | `string` | Yes | 來源 catalog 的 kebab-case slug。 |
| `DetailHref` | `string` | Yes | 既有詳情頁路徑：`/dinosaurs/{slug}` 或 `/aquarium/{slug}`。 |
| `SortOrder` | `int` | Yes | 來源 catalog 既有 `SortOrder`。 |
| `Name` | bilingual text | Yes | 顯示名稱。不得在任一支援語言 fallback 後為空。 |
| `Summary` | bilingual text | Yes | 短摘要。不得在任一支援語言 fallback 後為空。 |
| `ImagePath` | `string` | Yes | 主要圖片路徑；若圖片無法顯示，卡片仍保留替代說明與詳情連結。 |
| `ImageAltText` | bilingual text | Yes | 圖片替代說明，fallback 後不得為空。 |
| `SearchText` | `string` | Yes | 由 `zh-TW` 與 `en` 欄位組成的未顯示索引文字；瀏覽器端再以 NFKC、lowercase、letter/digit 正規化。 |
| `Facets` | `IReadOnlyList<ExplorationFacetValue>` | Yes | 此項目可匹配的來源、食性、生活區域、生活時期、發現地點等分類值。 |

### Validation Rules

- `StableId` 必須全站唯一。
- `Slug` 必須保持來源 catalog 的 kebab-case identity，不另建探索專用 slug。
- `DetailHref` 必須是一般 anchor 可直接開啟的既有 canonical detail route。
- 顯示文字、摘要、來源標籤與 alt text 使用現有 bilingual fallback，fallback 後不得為空。
- `SearchText` 必須包含兩種語言的可搜尋欄位與 curated keywords，不受目前 UI 語言限制。

## ExplorationSourceType

目前支援的來源故事書。

| Value | Label zh-TW | Label en | Detail route prefix | Source order |
|-------|-------------|----------|---------------------|--------------|
| `dinosaurs` | 恐龍 | Dinosaurs | `/dinosaurs` | 1 |
| `aquarium` | 水族館 | Aquarium | `/aquarium` | 2 |

### Validation Rules

- 預設、搜尋與篩選結果先依 `Source order` 分組，再依各來源 `SortOrder` 排列。
- 未來新增來源時不得改變既有來源 stable id 或 detail route。

## ExplorationFacetGroup

可供使用者點選的分類群組。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Code` | `string` | Yes | 群組代碼，例如 `source`、`diet`、`habitat`、`period`、`discovery-location`。 |
| `Label` | bilingual text | Yes | 群組顯示名稱。 |
| `SortOrder` | `int` | Yes | UI 顯示順序。 |
| `SelectionMode` | `string` | Yes | 固定為 `single`：每個群組一次只能有一個有效值。 |
| `Values` | `IReadOnlyList<ExplorationFacetValue>` | Yes | 目前可用分類值，依內容與固定群組順序產生。 |

### Validation Rules

- 群組內一次只能選取一個 value；選新 value 會取代同群組舊 value。
- 清除分類必須移除所有群組選擇。
- 無結果時仍保留清除分類控制。

## ExplorationFacetValue

單一可選分類值，可能來自來源類型、食性、生活區域、生活時期或發現地點。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `GroupCode` | `string` | Yes | 對應 `ExplorationFacetGroup.Code`。 |
| `ValueCode` | `string` | Yes | 正規化後的穩定值；需適合 HTML data attribute 與測試比對。 |
| `Label` | bilingual text | Yes | 顯示文字，fallback 後不得為空。 |
| `SourceScope` | `ExplorationSourceType?` | No | 若此 facet 只適用單一來源，可標示來源。 |
| `SortOrder` | `int` | Yes | 群組內顯示順序。 |

### Validation Rules

- 同一 `GroupCode` + `ValueCode` 不得重複。
- `ValueCode` 不直接顯示給使用者；顯示一律使用 bilingual `Label`。
- 來源 filter 至少包含 `dinosaurs` 與 `aquarium`。

## ExplorationSearchState

目前 `/explore` 頁面生命週期內的搜尋與篩選狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `RawQuery` | `string` | No | 使用者目前輸入文字；只存在於頁面記憶體與 DOM input。 |
| `NormalizedQuery` | `string` | No | NFKC + lowercase + letters/digits only 的搜尋字串。 |
| `SelectedFacetValues` | `Dictionary<string,string>` | Yes | 每個群組最多一個選取值。 |
| `Language` | `LanguageCode` | Yes | 目前顯示語言；無效時回退 `zh-TW`。 |
| `ResultMode` | enum | Yes | `all`、`search`、`filter`、`intersection`、`too-short`、`no-results`、`partial-failure`、`all-failed`。 |
| `VisibleResultCount` | `int` | Yes | 目前可見結果數。 |

### State Rules

- 狀態不得寫入 URL query、browser history、localStorage 或 session storage。
- 重新載入或重新進入 `/explore` 時回到完整集合。
- `NormalizedQuery` 為空、只有標點或少於 2 個有效字元時顯示友善提示並保留完整集合。
- 搜尋與分類同時存在時，結果必須同時符合搜尋字串與所有已選分類群組。

## ExplorationSourceStatus

描述每個來源 catalog 在探索頁中的可用狀態。

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Source` | `ExplorationSourceType` | Yes | 來源故事書。 |
| `IsAvailable` | `bool` | Yes | 此來源是否成功提供 projection。 |
| `ItemCount` | `int` | Yes | 可用內容筆數；失敗時為 0。 |
| `FriendlyMessage` | bilingual text | No | 來源失敗時顯示給使用者的兒童友善訊息。 |

### Failure Rules

- 單一來源失敗：顯示其他可用來源內容與 partial failure message，並記錄 failure。
- 全部來源失敗：顯示友善錯誤狀態與返回首頁 action，不顯示空白頁。
- 使用者可見錯誤不得暴露檔案路徑、exception type 或 stack trace。
