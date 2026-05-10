<p align="center">
  <img src="StoryBook/wwwroot/favicon.ico" width="32" height="32" alt="StoryBook icon" />
</p>

# StoryBook

StoryBook 是一個以 **.NET 10 / ASP.NET Core Razor Pages** 製作的兒童友善恐龍介紹網站。它用伺服器端 Razor Pages 呈現首頁、恐龍清單與詳情頁，並以本機 JSON 內容檔提供 8 筆雙語史前生物介紹。

[功能特色](#功能特色) • [快速開始](#快速開始) • [公開路由](#公開路由) • [專案結構](#專案結構) • [測試](#測試) • [規格文件](#規格文件)

> [!NOTE]
> 本專案目前聚焦桌面與筆電瀏覽情境。手機深度最佳化不是目前範圍，但 768px 以上寬度應維持基本回應式版面與鍵盤可操作性。

## 功能特色

- **首頁入口**：首頁提供清楚的「恐龍介紹」入口，連到 `/dinosaurs`。
- **8 筆史前生物內容**：包含暴龍、三角龍、劍龍、腕龍、迅猛龍、翼龍、甲龍與副櫛龍。
- **可直接連結的詳情頁**：每筆內容都有 `/dinosaurs/{slug}` canonical route。
- **清單與即時搜尋**：`/dinosaurs` 顯示第一筆介紹、完整卡片清單、搜尋框、清除搜尋與無結果提示。
- **一般 anchor 導覽**：上一頁、下一頁、搜尋結果與回首頁都使用一般連結，保留瀏覽器 history。
- **兒童友善故事**：每筆內容都有短篇小故事與故事插圖。
- **圖片大圖檢視**：詳情頁主圖可用滑鼠或鍵盤開啟大圖，支援關閉按鈕、背景點擊與 Escape 關閉。
- **雙語切換**：支援繁體中文 `zh-TW` 與英文 `en`，語言偏好保存於 `localStorage` key `storybook.language`。
- **友善找不到狀態**：不存在的 slug 會回傳 404，並顯示可回首頁或回清單的提示。

## 內容範圍

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

## 技術架構

- **Web app**：單一 ASP.NET Core Razor Pages 專案 `StoryBook/StoryBook.csproj`
- **Target framework**：`net10.0`
- **UI**：Razor Pages、Tag Helpers、Bootstrap 5、共用 layout
- **互動邏輯**：原生 JavaScript，集中於 `StoryBook/wwwroot/js/dinosaurs.js`
- **樣式**：站台共用樣式加上 feature-specific `StoryBook/wwwroot/css/dinosaurs.css`
- **資料來源**：本機 JSON catalog，不使用資料庫、外部 CMS、百科 API 或即時翻譯服務
- **服務層**：catalog 載入、內容驗證、搜尋、導覽與語言偏好放在 injectable services
- **測試**：xUnit 與 `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory<Program>`

核心資料流程：

```text
StoryBook/Data/dinosaurs.json
  -> DinosaurCatalogService
  -> Razor PageModel
  -> Razor Pages HTML
  -> dinosaurs.js enhances search, language switching, and image modal
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
| `/` | 首頁，提供「恐龍介紹」入口 |
| `/dinosaurs` | 恐龍清單、第一筆介紹與即時搜尋 |
| `/dinosaurs/tyrannosaurus-rex` | 暴龍詳情頁 |
| `/dinosaurs/pteranodon` | 翼龍詳情頁，包含非恐龍分類提示 |
| `/dinosaurs/not-a-real-slug` | 友善 404 找不到內容狀態 |

## 專案結構

```text
StoryBook2.sln
StoryBook/
├── Program.cs
├── Data/
│   └── dinosaurs.json
├── Models/
├── Services/
├── Pages/
│   ├── Index.cshtml
│   └── Dinosaurs/
│       ├── Index.cshtml
│       ├── Index.cshtml.cs
│       ├── Details.cshtml
│       └── Details.cshtml.cs
└── wwwroot/
    ├── css/
    │   └── dinosaurs.css
    ├── js/
    │   └── dinosaurs.js
    └── images/
        └── dinosaurs/

StoryBook.Tests/
├── Unit/
├── Integration/
└── Support/
```

> [!IMPORTANT]
> `StoryBook/bin/` 與 `StoryBook/obj/` 是 .NET 建置產物，不應手動修改，也不應作為功能文件或內容來源。

## 測試

目前測試專案涵蓋：

- catalog JSON 載入、快取、排序與 slug lookup
- 內容驗證：8 筆資料、kebab-case slug、雙語欄位、故事長度、圖片 alt text、翼龍分類
- 搜尋：名稱、時期、食性、地點、體型、簡介、關鍵字、空白與標點處理
- 語言偏好：`zh-TW`、`en`、無效值 fallback 與 `storybook.language`
- Razor Pages 整合：首頁入口、清單頁、詳情頁、上一頁/下一頁、圖片 modal contract、語言切換 attributes、404 fallback

執行全部測試：

```bash
dotnet test StoryBook2.sln
```

手動驗收流程記錄在 `specs/001-dinosaur-intro-site/quickstart.md`，包含搜尋、語言保存、modal 焦點回復、瀏覽器 history 與 responsive 檢查。

## 規格文件

本功能使用 Spec Kit 文件描述需求、設計、資料模型、路由契約與驗收方式：

- `specs/001-dinosaur-intro-site/spec.md`
- `specs/001-dinosaur-intro-site/plan.md`
- `specs/001-dinosaur-intro-site/data-model.md`
- `specs/001-dinosaur-intro-site/contracts/ui-routes.md`
- `specs/001-dinosaur-intro-site/contracts/content-catalog.schema.json`
- `specs/001-dinosaur-intro-site/quickstart.md`
- `specs/001-dinosaur-intro-site/tasks.md`

專案治理與開發原則位於 `.specify/memory/constitution.md`。
