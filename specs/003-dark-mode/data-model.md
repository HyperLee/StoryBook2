# Data Model: 網站深色模式與主題切換

## Entity: ThemeMode

代表使用者可選擇並可被保存的主題模式。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `value` | enum/string | Yes | 只允許 `light`、`dark`、`system`。其他值視為無效並回退 `system`。 |
| `displayName` | `LocalizedText` | Yes | `zh-TW` 與 `en` 必填；缺漏或無效語言回退繁體中文，不得顯示空白。 |
| `description` | `LocalizedText` | Yes | 用於首頁輔助說明或 accessible description；內容需簡短、清楚、兒童可理解。 |
| `sortOrder` | int | Yes | 固定排序：`light` = 1、`dark` = 2、`system` = 3。 |
| `isDefault` | bool | Yes | 只有 `system` 為 true。 |

## Entity: EffectiveTheme

代表網站實際呈現的亮色或深色外觀，由 `ThemeMode` 與系統外觀偏好共同決定。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `value` | enum/string | Yes | 只允許 `light` 或 `dark`。 |
| `sourceMode` | `ThemeMode` | Yes | 產生有效主題的使用者模式。 |
| `systemPreference` | enum/string | Conditional | `sourceMode = system` 時可為 `light`、`dark` 或 `unknown`。 |
| `resolvedAt` | runtime timestamp | No | 僅供執行期間判斷；不得持久化為使用者偏好。 |

## Entity: UserThemePreference

代表同一瀏覽器與裝置保存的使用者主題選擇。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `storageKey` | string | Yes | 固定為 `storybook.theme`。 |
| `selectedMode` | `ThemeMode` | Yes | 只能保存 `light`、`dark` 或 `system`。無效、缺漏或不可讀取時視為 `system`。 |
| `storageSource` | enum/string | Yes | `default`、`localStorage`、`userSelection` 或 `unavailable`。 |
| `canPersist` | bool | Yes | localStorage 可用時為 true；不可用時仍需套用安全有效主題。 |

## Entity: ThemeControlState

代表首頁主題 selector 的互動狀態。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `availableModes` | `ThemeMode[]` | Yes | 必須剛好三筆：亮色、深色、跟隨系統。 |
| `selectedMode` | `ThemeMode` | Yes | 必須對應目前有效使用者偏好。 |
| `effectiveTheme` | `EffectiveTheme` | Yes | 切換選項後 1 秒內更新。 |
| `language` | enum/string | Yes | `zh-TW` 或 `en`；無效值回退 `zh-TW`。 |
| `accessibleName` | `LocalizedText` | Yes | selector 群組必須有 accessible name。 |
| `targetSize` | CSS length | Yes | 每個可操作 label/control 至少 44x44 CSS px。 |

## Entity: ThemeSyncState

代表主題模式在同一網站多分頁與系統偏好間的同步狀態。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `currentMode` | `ThemeMode` | Yes | 本分頁目前採用的模式。 |
| `effectiveTheme` | `EffectiveTheme` | Yes | 本分頁目前套用的有效主題。 |
| `lastEventSource` | enum/string | Yes | `initialLoad`、`userSelection`、`systemPreferenceChange`、`storageEvent` 或 `fallback`。 |
| `syncDeadline` | duration | Yes | 系統偏好變更與跨分頁更新都必須在 2 秒內反映。 |

## Entity: ThemedPageScope

代表本功能必須套用一致有效主題的頁面範圍。

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `route` | string | Yes | `/`、`/Privacy`、`/Error`、`/dinosaurs`、`/dinosaurs/{slug}`、`/aquarium`、`/aquarium/{slug}`。 |
| `hasThemeSelector` | bool | Yes | 只有 `/` 為 true；其他頁面必須為 false。 |
| `usesSharedLayout` | bool | Yes | 必須為 true，確保早期 theme boot 與 theme.js 套用。 |
| `preservesExistingState` | bool | Yes | 主題切換不得修改搜尋、語言、圖片 modal、導覽位置或故事內容。 |

## Relationships

- `UserThemePreference` 1:1 `ThemeMode`
- `ThemeMode` 1:1 `EffectiveTheme` at runtime
- `ThemeControlState` 1:3 `ThemeMode`
- `ThemeControlState` 1:1 `UserThemePreference`
- `ThemeSyncState` 1:1 `EffectiveTheme`
- `ThemedPageScope` 0..1 `ThemeControlState`;只有首頁擁有 selector

## State Transitions

### Initial Theme Resolution

```text
Page starts loading
  -> Read storybook.theme
  -> Invalid/missing/unavailable storage returns ThemeMode(system)
  -> If mode is light/dark, EffectiveTheme matches mode
  -> If mode is system, read prefers-color-scheme
  -> Unknown system preference returns EffectiveTheme(light)
  -> Set html data-bs-theme before first visible render
```

### User Selection

```text
Home theme selector loaded
  -> User selects light/dark/system
  -> Persist selected ThemeMode only
  -> Re-resolve EffectiveTheme
  -> Update html attributes and selector checked state
  -> Other same-site tabs receive storage event within 2 seconds
```

### System Preference Change

```text
Current ThemeMode(system)
  -> Browser emits prefers-color-scheme change
  -> Re-resolve EffectiveTheme
  -> Update all visible themed UI within 2 seconds
```

If current mode is `light` or `dark`, system preference changes do not affect `EffectiveTheme`.

### Invalid Storage Recovery

```text
localStorage storybook.theme = unsupported value
  -> Treat as ThemeMode(system)
  -> Do not render blank selector state
  -> Next user selection overwrites stored value with valid mode
```
