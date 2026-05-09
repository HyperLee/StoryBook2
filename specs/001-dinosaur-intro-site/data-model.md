# Data Model: 兒童恐龍介紹網站

## Entity: DinosaurProfile

代表一筆可瀏覽的史前生物介紹。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `slug` | string | Yes | 全站唯一；lowercase kebab-case；只允許 `a-z`、`0-9`、`-`；用於 `/dinosaurs/{slug}`。 |
| `category` | enum/string | Yes | `dinosaur` 或 `prehistoric-flying-reptile`；翼龍必須使用後者。 |
| `sortOrder` | int | Yes | 1-8 且不可重複；決定上一頁/下一頁順序。 |
| `names` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填。 |
| `periods` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填。 |
| `diet` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填。 |
| `discoveryLocations` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填。 |
| `sizeDescription` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填。 |
| `summary` | `LocalizedText` | Yes | 中文不超過 200 字；英文不超過 200 words；語氣適合 5-10 歲。 |
| `notDinosaurNote` | `LocalizedText?` | Conditional | `category` 為 `prehistoric-flying-reptile` 時必填，用於標示翼龍不是真正恐龍。 |
| `mainImage` | `Illustration` | Yes | 必須有合法 web path 與雙語 alt text。 |
| `story` | `DinosaurStory` | Yes | 每筆一則。 |
| `searchKeywords` | `LocalizedText[]` | Yes | 至少包含名稱、時期、食性、地點與常見關鍵字。 |

## Entity: LocalizedText

代表同一欄位的雙語內容。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `zhTW` | string | Yes | 不得空白；使用繁體中文。 |
| `en` | string | Yes | 不得空白；缺漏時 UI 可回退中文，但資料驗證應標記為內容缺口。 |

## Entity: DinosaurStory

附屬於單一 `DinosaurProfile` 的短篇故事。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `title` | `LocalizedText` | Yes | 兒童友善，不使用驚嚇或暴力語氣。 |
| `body` | `LocalizedText` | Yes | 每種語言 100-150 個可閱讀單位；中文以字計，英文以 words 計。 |
| `illustration` | `Illustration` | Yes | 與故事情境一致，必須有雙語 alt text。 |

## Entity: Illustration

用於主圖與故事插圖。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `path` | string | Yes | 必須是站內 path，例如 `/images/dinosaurs/t-rex-main.png`；不得指向外部未審核資源。 |
| `altText` | `LocalizedText` | Yes | 必須描述圖片內容，不使用空字串或檔名代替。 |
| `caption` | `LocalizedText?` | No | 若顯示 caption，必須支援雙語。 |
| `styleTag` | string | Yes | 初始值統一為 `storybook-cute`，用於檢查風格一致性。 |

## Entity: LanguagePreference

代表使用者目前顯示語言。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `code` | enum | Yes | `zh-TW` 或 `en`；無效值回退 `zh-TW`。 |
| `storageKey` | string | Yes | 固定為 `storybook.language`。 |
| `source` | enum | Yes | `default`、`localStorage` 或 `userSelection`。 |

## Entity: SearchState

代表恐龍清單頁的即時搜尋狀態。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `query` | string | Yes | trim 後比對；特殊字元不得造成例外。 |
| `language` | `LanguagePreference` | Yes | 決定搜尋結果顯示語言。 |
| `matchingSlugs` | string[] | Yes | 只包含存在的 `DinosaurProfile.slug`。 |
| `hasResults` | bool | Yes | 無結果時顯示友善提示與清除搜尋控制項。 |

## Relationships

- `DinosaurProfile` 1:1 `DinosaurStory`
- `DinosaurProfile` 1:1 `mainImage`
- `DinosaurStory` 1:1 `illustration`
- `SearchState` 0..8 `DinosaurProfile`
- `LanguagePreference` 影響所有 `LocalizedText` 欄位的顯示值

## State Transitions

### Dinosaur Browsing

```text
Home
  -> DinosaurList
  -> DinosaurDetails(valid slug)
  -> DinosaurDetails(previous/next valid slug)
  -> Home
```

Invalid slug transitions to `DinosaurNotFound`, which provides links back to `Home` and `DinosaurList`.

### Image Modal

```text
Closed
  -> Opening(main image selected)
  -> Open
  -> Closing(close button, backdrop, or Escape)
  -> Closed(focus returns to triggering image/button)
```

### Language Preference

```text
Default zh-TW
  -> User selects en
  -> Persist en in localStorage
  -> Navigate pages with en applied
  -> User selects zh-TW
  -> Persist zh-TW in localStorage
```

If a localized field is missing, display the available fallback text and avoid rendering a blank UI region.
