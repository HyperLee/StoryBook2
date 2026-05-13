<p align="center">
  <img src="StoryBook/wwwroot/favicon.ico" width="32" height="32" alt="StoryBook icon" />
</p>

# StoryBook

StoryBook 是一個以 **.NET 10 / ASP.NET Core Razor Pages** 製作的兒童友善自然故事書網站。它維持單一 Razor Pages web application，以伺服器端頁面、本機 JSON catalog、Bootstrap 5 與少量原生 JavaScript 呈現雙語故事內容。

目前已實作兩個故事書主題與一個整站外觀功能：

- **恐龍介紹**：8 筆史前生物介紹，包含翼龍並明確標示其不是真正恐龍。
- **水族館故事書**：15 筆水族館生物介紹，涵蓋 6 種生活區域分類。
- **網站主題切換**：首頁可選擇亮色模式、深色模式或跟隨系統，並套用到所有主要頁面。

[功能特色](#功能特色) • [快速開始](#快速開始) • [公開路由](#公開路由) • [專案結構](#專案結構) • [測試](#測試) • [規格文件](#規格文件)

> [!NOTE]
> 本專案主要瀏覽情境仍以桌面與筆電為主；深色模式與主要頁面需依規格在 375px、768px 與 1366px 代表寬度維持可讀、可操作且不重疊。

## 功能特色

- **首頁雙入口**：首頁提供「恐龍介紹」與「水族館動物介紹」入口，分別連到 `/dinosaurs` 與 `/aquarium`。
- **可直接連結的內容頁**：每筆內容都有穩定的 kebab-case slug，可用 `/dinosaurs/{slug}` 或 `/aquarium/{slug}` 直接開啟。
- **一般 anchor 導覽**：開始閱讀、上一頁、下一頁、搜尋結果、回主題首頁與回首頁都使用一般連結，保留瀏覽器 history。
- **即時搜尋**：主題首頁提供搜尋框、清除搜尋與友善提示；水族館搜尋會處理過短查詢、空白、標點與英文大小寫。
- **雙語切換**：支援繁體中文 `zh-TW` 與英文 `en`，語言偏好保存於 `localStorage` key `storybook.language`，並在兩個故事書主題間共享。
- **整站主題切換**：首頁提供「亮色模式」、「深色模式」與「跟隨系統」三種模式，使用 `localStorage` key `storybook.theme` 保存偏好；首頁以外頁面套用有效主題但不顯示主題 selector。
- **兒童友善故事**：每筆內容都有簡短介紹、小故事、主圖與故事插圖，文字與圖片替代文字皆提供雙語內容。
- **圖片大圖檢視**：詳情頁主圖可用滑鼠或鍵盤開啟大圖，支援明確關閉按鈕、背景點擊、Escape 關閉與焦點回復。
- **友善錯誤狀態**：不存在的 slug 會回傳 404；水族館 catalog 載入失敗時會顯示可重新嘗試與回首頁的孩童友善訊息。

## 內容範圍

### 恐龍介紹

內容來源是 `StoryBook/Data/dinosaurs.json`，圖片位於 `StoryBook/wwwroot/images/dinosaurs/`。

| 順序 | Slug | 中文名稱 | English | 分類 |
| --- | --- | --- | --- | --- |
| 1 | `tyrannosaurus-rex` | 暴龍 | Tyrannosaurus Rex | `dinosaur` |
| 2 | `triceratops` | 三角龍 | Triceratops | `dinosaur` |
| 3 | `stegosaurus` | 劍龍 | Stegosaurus | `dinosaur` |
| 4 | `brachiosaurus` | 腕龍 | Brachiosaurus | `dinosaur` |
| 5 | `velociraptor` | 迅猛龍 | Velociraptor | `dinosaur` |
| 6 | `pteranodon` | 翼龍 | Pteranodon | `prehistoric-flying-reptile` |
| 7 | `ankylosaurus` | 甲龍 | Ankylosaurus | `dinosaur` |
| 8 | `parasaurolophus` | 副櫛龍 | Parasaurolophus | `dinosaur` |

翼龍會被明確標示為史前飛行爬行類，而不是真正恐龍。

### 水族館故事書

內容來源是 `StoryBook/Data/aquarium.json`，圖片位於 `StoryBook/wwwroot/images/aquarium/`。

| 順序 | Slug | 中文名稱 | English | 生活區域 |
| --- | --- | --- | --- | --- |
| 1 | `clownfish` | 小丑魚 | Clownfish | `coral-reef` |
| 2 | `seahorse` | 海馬 | Seahorse | `saltwater` |
| 3 | `sea-turtle` | 海龜 | Sea turtle | `saltwater` |
| 4 | `jellyfish` | 水母 | Jellyfish | `deep-sea` |
| 5 | `octopus` | 章魚 | Octopus | `saltwater` |
| 6 | `shark` | 鯊魚 | Shark | `saltwater` |
| 7 | `stingray` | 魟魚 | Stingray | `saltwater` |
| 8 | `penguin` | 企鵝 | Penguin | `polar` |
| 9 | `seal` | 海豹 | Seal | `polar` |
| 10 | `dolphin` | 海豚 | Dolphin | `saltwater` |
| 11 | `starfish` | 海星 | Starfish | `tide-pool` |
| 12 | `crab` | 螃蟹 | Crab | `tide-pool` |
| 13 | `coral` | 珊瑚 | Coral | `coral-reef` |
| 14 | `goldfish` | 金魚 | Goldfish | `freshwater` |
| 15 | `axolotl` | 六角恐龍 | Axolotl | `freshwater` |

水族館生活區域分類包含 `freshwater`、`saltwater`、`deep-sea`、`coral-reef`、`polar` 與 `tide-pool`。

## 技術架構

- **Web app**：單一 ASP.NET Core Razor Pages 專案 `StoryBook/StoryBook.csproj`
- **Target framework**：`net10.0`
- **UI**：Razor Pages、Tag Helpers、Bootstrap 5、共用 layout
- **互動邏輯**：原生 JavaScript；故事書互動分在 `StoryBook/wwwroot/js/dinosaurs.js` 與 `StoryBook/wwwroot/js/aquarium.js`，整站主題切換在 `StoryBook/wwwroot/js/theme.js`
- **樣式**：`site.css` 提供全站 layout 與 theme tokens，feature-specific 樣式位於 `dinosaurs.css` 與 `aquarium.css`
- **資料來源**：本機 JSON catalog，不使用資料庫、外部 CMS、百科 API 或即時翻譯服務
- **服務層**：catalog 載入、內容驗證、搜尋、導覽、語言偏好與主題偏好放在 injectable services
- **測試**：xUnit 與 `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory<Program>`

核心資料流程：

```text
StoryBook/Data/*.json
  -> CatalogService + ContentValidator
  -> Razor PageModel
  -> Razor Pages HTML
  -> theme.js applies site theme
  -> feature JavaScript enhances search, language switching, and image modal
```

## 快速開始

### Prerequisites

- .NET SDK 10.x
- 桌面瀏覽器，例如 Chrome、Firefox、Safari 或 Edge

### Restore, build, and test

```bash
dotnet restore StoryBook2.sln
dotnet build StoryBook2.sln
dotnet test StoryBook2.sln
```

### Run locally

```bash
dotnet run --project StoryBook/StoryBook.csproj
```

`launchSettings.json` 目前包含下列本機 URL；實際請以 `dotnet run` 輸出的 URL 為準。

- `http://localhost:5059`
- `https://localhost:7111`

## 公開路由

| Route | 說明 |
| --- | --- |
| `/` | 首頁，提供恐龍與水族館兩個故事書入口，以及唯一的主題模式 selector |
| `/Privacy` | Privacy 範本頁，套用目前有效主題但不顯示主題 selector |
| `/Error` | Error 範本頁，套用目前有效主題但不顯示主題 selector |
| `/dinosaurs` | 恐龍清單、第一筆介紹與即時搜尋 |
| `/dinosaurs/tyrannosaurus-rex` | 暴龍詳情頁 |
| `/dinosaurs/pteranodon` | 翼龍詳情頁，包含非恐龍分類提示 |
| `/dinosaurs/not-a-real-slug` | 恐龍友善 404 找不到內容狀態 |
| `/aquarium` | 水族館故事書主頁、封面、開始閱讀、搜尋與完整集合 |
| `/aquarium/clownfish` | 小丑魚詳情頁，水族館閱讀順序第一筆 |
| `/aquarium/axolotl` | 六角恐龍詳情頁，水族館閱讀順序最後一筆 |
| `/aquarium/not-a-real-slug` | 水族館友善 404 找不到內容狀態 |

## 專案結構

```text
StoryBook2.sln
StoryBook/
├── Program.cs
├── Data/
│   ├── dinosaurs.json
│   └── aquarium.json
├── Models/
├── Services/
├── Pages/
│   ├── Index.cshtml
│   ├── Privacy.cshtml
│   ├── Error.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml
│   ├── Dinosaurs/
│   │   ├── Index.cshtml
│   │   ├── Index.cshtml.cs
│   │   ├── Details.cshtml
│   │   └── Details.cshtml.cs
│   └── Aquarium/
│       ├── Index.cshtml
│       ├── Index.cshtml.cs
│       ├── Details.cshtml
│       └── Details.cshtml.cs
└── wwwroot/
    ├── css/
    │   ├── site.css
    │   ├── dinosaurs.css
    │   └── aquarium.css
    ├── js/
    │   ├── theme.js
    │   ├── dinosaurs.js
    │   └── aquarium.js
    └── images/
        ├── dinosaurs/
        └── aquarium/

StoryBook.Tests/
├── Unit/
├── Integration/
└── Support/
```

> [!IMPORTANT]
> `StoryBook/bin/`、`StoryBook/obj/`、`StoryBook.Tests/bin/` 與 `StoryBook.Tests/obj/` 是 .NET 建置產物，不應手動修改，也不應作為功能文件或內容來源。

## 測試

目前測試專案涵蓋：

- Dinosaur 與 Aquarium catalog JSON 載入、快取、排序、slug lookup、上一頁/下一頁與搜尋
- 內容驗證：固定資料筆數、kebab-case slug、雙語欄位、故事長度、圖片 alt text、圖片路徑與分類規則
- 搜尋：中英文內容、名稱、分類、時期或生活環境、食性、地點、摘要、關鍵字、空白、標點、過短查詢與英文大小寫
- 語言偏好：`zh-TW`、`en`、無效值 fallback、缺漏內容 fallback 與 `storybook.language`
- 主題偏好：`storybook.theme`、`light`/`dark`/`system` 允許值、default/fallback、雙語 label metadata、layout boot script、首頁 selector、非首頁 selector absence 與 theme assets contract
- Razor Pages 整合：首頁入口、清單頁、詳情頁、上一頁/下一頁、圖片 modal contract、語言切換 attributes、404 fallback 與資料載入失敗狀態

執行全部測試：

```bash
dotnet test StoryBook2.sln
```

最近一次本機驗證狀態請以當次 `dotnet test StoryBook2.sln` 輸出為準。

手動驗收流程記錄在：

- `specs/001-dinosaur-intro-site/quickstart.md`
- `specs/002-aquarium-storybook/quickstart.md`
- `specs/003-dark-mode/quickstart.md`

## 規格文件

本專案使用 Spec Kit 文件描述需求、設計、資料模型、路由契約、任務與驗收方式。

恐龍介紹：

- `specs/001-dinosaur-intro-site/spec.md`
- `specs/001-dinosaur-intro-site/plan.md`
- `specs/001-dinosaur-intro-site/data-model.md`
- `specs/001-dinosaur-intro-site/contracts/ui-routes.md`
- `specs/001-dinosaur-intro-site/contracts/content-catalog.schema.json`
- `specs/001-dinosaur-intro-site/quickstart.md`
- `specs/001-dinosaur-intro-site/tasks.md`

水族館故事書：

- `specs/002-aquarium-storybook/spec.md`
- `specs/002-aquarium-storybook/plan.md`
- `specs/002-aquarium-storybook/data-model.md`
- `specs/002-aquarium-storybook/contracts/ui-routes.md`
- `specs/002-aquarium-storybook/contracts/content-catalog.schema.json`
- `specs/002-aquarium-storybook/quickstart.md`
- `specs/002-aquarium-storybook/tasks.md`

網站深色模式與主題切換：

- `specs/003-dark-mode/spec.md`
- `specs/003-dark-mode/plan.md`
- `specs/003-dark-mode/research.md`
- `specs/003-dark-mode/data-model.md`
- `specs/003-dark-mode/contracts/theme-ui.md`
- `specs/003-dark-mode/quickstart.md`
- `specs/003-dark-mode/tasks.md`
- `specs/003-dark-mode/checklists/requirements.md`

專案治理與開發原則位於 `.specify/memory/constitution.md`。
