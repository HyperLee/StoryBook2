# Implementation Plan: 水族館動物介紹故事書

**Branch**: `002-aquarium-storybook` | **Date**: 2025-11-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-aquarium-storybook/spec.md`

## Summary

建立水族館動物介紹故事書系統，採用童書翻頁模式展示 15 種水族館動物。系統參照現有恐龍故事書架構，新增生活區域分類（淡水/海水/深海）功能，支援中英文雙語、即時搜尋、Lightbox 大圖檢視和換頁瀏覽。使用 ASP.NET Core Razor Pages + Bootstrap 5 + 原生 JavaScript 實作。

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (ASP.NET Core 10.0)  
**Primary Dependencies**: ASP.NET Core Razor Pages, Bootstrap 5, jQuery (已包含在專案中), Serilog  
**Storage**: JSON 檔案 (`wwwroot/data/aquarium.json`) - 練習用途，不使用資料庫  
**Testing**: xUnit + Moq (單元測試) + WebApplicationFactory (整合測試)  
**Logging**: Serilog（結構化日誌）+ Console Sink + File Sink  
**Target Platform**: 桌面瀏覽器 (Chrome, Firefox, Safari, Edge) - 不需考慮手機版  
**Project Type**: Web Application (Razor Pages)  
**Performance Goals**: API p95 回應時間 < 200ms，頁面載入 < 3 秒，換頁 < 1 秒  
**Constraints**: 單一請求記憶體使用 < 100MB，動物介紹 ≤ 200 字，純靜態內容展示  
**Scale/Scope**: 15 隻動物資料，2 種語言（中文/英文），約 5 個頁面

### 前端技術選擇

**推薦方案**: 原生 JavaScript + Bootstrap 5
- **理由**: 專案已包含 Bootstrap 和 jQuery，兒童故事書網站互動需求簡單
- **優點**: 無需額外設定建構工具、學習曲線低、效能好

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 狀態 | 說明 |
|------|------|------|
| I. 程式碼品質至上 | ✅ PASS | 遵循 C# 14 最佳實踐、XML 文件註解、檔案範圍命名空間 |
| II. 測試優先開發 | ✅ PASS | 使用 xUnit + Moq + WebApplicationFactory，遵循 TDD 流程 |
| III. 使用者體驗一致性 | ✅ PASS | 參照恐龍故事書 UI 風格，使用 Bootstrap 5，支援無障礙設計 |
| IV. 效能與延展性 | ✅ PASS | JSON 快取於 Singleton 服務，圖片延遲載入，靜態檔案快取 |
| V. 可觀察性與監控 | ✅ PASS | 使用 Serilog 結構化日誌，正確使用日誌層級 |
| VI. 安全優先 | ✅ PASS | 純靜態內容展示，無敏感資料，輸入驗證（搜尋關鍵字） |