# Data Model: 水族館動物介紹故事書

## Entity: AquariumCatalog

代表水族館內容檔根物件。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `version` | string | Yes | 不得空白；用於內容審查與快取診斷。 |
| `habitatCategories` | `AquariumHabitatCategory[]` | Yes | 至少 3 種；分類代碼唯一；每種分類需雙語名稱。 |
| `animals` | `AquariumAnimalProfile[]` | Yes | 必須剛好 15 筆；slug 與 sortOrder 唯一。 |

## Entity: AquariumHabitatCategory

代表水族館生活區域分類，例如淡水、海水、深海、珊瑚礁、極地與潮間帶。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `code` | string | Yes | 全 catalog 唯一；lowercase kebab-case；建議初始值包含 `freshwater`、`saltwater`、`deep-sea`、`coral-reef`、`polar`、`tide-pool`。 |
| `sortOrder` | int | Yes | 正整數且不可重複；決定主頁分類呈現順序。 |
| `displayName` | `AquariumText` | Yes | `zhTW` 與 `en` 必填。 |
| `description` | `AquariumText` | No | 若顯示分類說明，雙語內容都需存在。 |

## Entity: AquariumAnimalProfile

代表一筆可閱讀、搜尋與逐頁瀏覽的水族館生物介紹。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `slug` | string | Yes | 全站唯一；lowercase kebab-case；只允許 `a-z`、`0-9`、`-`；用於 `/aquarium/{slug}`。 |
| `habitatCategory` | string | Yes | 必須對應既有 `AquariumHabitatCategory.code`。 |
| `sortOrder` | int | Yes | 1-15 且不可重複；決定開始閱讀與上一頁/下一頁順序。 |
| `names` | `AquariumText` | Yes | `zhTW` 與 `en` 必填。 |
| `habitat` | `AquariumText` | Yes | 生活環境描述，雙語必填。 |
| `diet` | `AquariumText` | Yes | 食性描述，雙語必填。 |
| `discoveryLocations` | `AquariumText` | Yes | 發現地點或常見分布，雙語必填。 |
| `summary` | `AquariumText` | Yes | 中文不超過 200 個可見中文字元；英文不超過 200 words；不得空白。 |
| `mainImage` | `AquariumImage` | Yes | 必須是站內圖片 path，並有雙語 alt text。 |
| `story` | `AquariumStory` | Yes | 每筆一則兒童友善小故事。 |
| `searchKeywords` | `AquariumText[]` | Yes | 至少一筆；應包含名稱、分類、生活環境與食性常見關鍵字。 |

## Initial Animal Set

| Sort | Slug | zh-TW Name | Habitat Category |
|------|------|------------|------------------|
| 1 | `clownfish` | 小丑魚 | `coral-reef` |
| 2 | `seahorse` | 海馬 | `saltwater` |
| 3 | `sea-turtle` | 海龜 | `saltwater` |
| 4 | `jellyfish` | 水母 | `deep-sea` |
| 5 | `octopus` | 章魚 | `saltwater` |
| 6 | `shark` | 鯊魚 | `saltwater` |
| 7 | `stingray` | 魟魚 | `saltwater` |
| 8 | `penguin` | 企鵝 | `polar` |
| 9 | `seal` | 海豹 | `polar` |
| 10 | `dolphin` | 海豚 | `saltwater` |
| 11 | `starfish` | 海星 | `tide-pool` |
| 12 | `crab` | 螃蟹 | `tide-pool` |
| 13 | `coral` | 珊瑚 | `coral-reef` |
| 14 | `goldfish` | 金魚 | `freshwater` |
| 15 | `axolotl` | 六角恐龍 | `freshwater` |

## Entity: AquariumText

代表同一欄位的雙語內容。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `zhTW` | string | Yes | 不得空白；使用繁體中文；作為所有缺漏內容的 fallback。 |
| `en` | string | Yes | 不得空白；缺漏時 UI 可回退中文，但內容驗證應標記為錯誤。 |

## Entity: AquariumStory

附屬於單一 `AquariumAnimalProfile` 的短篇故事。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `title` | `AquariumText` | Yes | 兒童友善；不得使用驚嚇、殘酷或過度科學術語。 |
| `body` | `AquariumText` | Yes | 每種語言 100-150 個可閱讀單位；中文以可見中文字元計，英文以 words 計。 |
| `illustration` | `AquariumImage` | Yes | 與故事情境一致，必須有雙語 alt text。 |

## Entity: AquariumImage

用於主圖與故事插圖。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `path` | string | Yes | 必須是站內 path，例如 `/images/aquarium/clownfish-main.png`；副檔名限 `png`、`jpg`、`jpeg`、`webp`。 |
| `altText` | `AquariumText` | Yes | 必須描述圖片內容，不得使用空字串或檔名代替。 |
| `caption` | `AquariumText?` | No | 若顯示 caption，必須雙語完整。 |
| `styleTag` | string | Yes | 初始值固定為 `storybook-cute`，用於檢查水族館插圖風格一致性。 |

## Entity: LanguagePreference

代表使用者目前顯示語言。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `code` | enum | Yes | `zh-TW` 或 `en`；無效值回退 `zh-TW`。 |
| `storageKey` | string | Yes | 固定為 `storybook.language`，與恐龍故事書共享。 |
| `source` | enum | Yes | `default`、`localStorage` 或 `userSelection`。 |

## Entity: SearchState

代表水族館主頁的即時搜尋狀態。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `query` | string | Yes | trim 並合併多餘空白；空白、只有符號或過短時顯示友善提示或完整集合，不造成例外。 |
| `language` | `LanguagePreference` | Yes | 決定搜尋結果顯示語言與 fallback。 |
| `matchingSlugs` | string[] | Yes | 只包含存在的 `AquariumAnimalProfile.slug`。 |
| `hasResults` | bool | Yes | 無結果時顯示友善提示與清除搜尋控制項。 |

## Relationships

- `AquariumCatalog` 1..* `AquariumHabitatCategory`
- `AquariumCatalog` exactly 15 `AquariumAnimalProfile`
- `AquariumAnimalProfile` many-to-1 `AquariumHabitatCategory`
- `AquariumAnimalProfile` 1:1 `mainImage`
- `AquariumAnimalProfile` 1:1 `AquariumStory`
- `AquariumStory` 1:1 `illustration`
- `SearchState` 0..15 `AquariumAnimalProfile`
- `LanguagePreference` 影響所有 `AquariumText` 欄位的顯示值

## State Transitions

### Aquarium Browsing

```text
Home
  -> AquariumHome(/aquarium)
  -> AquariumDetails(valid slug)
  -> AquariumDetails(previous/next valid slug)
  -> AquariumHome
  -> Home
```

Invalid slug transitions to `AquariumNotFound`, which returns HTTP 404 and provides links back to `Home` and `AquariumHome`.

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
  -> Persist en in localStorage key storybook.language
  -> Navigate aquarium pages with en applied
  -> User selects zh-TW
  -> Persist zh-TW in localStorage key storybook.language
```

If a localized field is missing or the stored language value is invalid, display Traditional Chinese fallback text and avoid rendering blank title, summary, story, alt text, or control labels.
