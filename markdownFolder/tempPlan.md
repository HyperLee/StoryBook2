## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (ASP.NET Core 10.0)  
**Primary Dependencies**: ASP.NET Core Razor Pages, Bootstrap 5, jQuery (已包含在專案中), Serilog  
**Storage**: JSON 檔案 (`wwwroot/data/dinosaurs.json`) - 練習用途，不使用資料庫  
**Testing**: xUnit + Moq (單元測試) + WebApplicationFactory (整合測試)；備案：TestServer 或 Mock-based 測試  
**Logging**: Serilog（結構化日誌）+ Console Sink + File Sink  
**Target Platform**: 桌面瀏覽器 (Chrome, Firefox, Safari, Edge) - 不需考慮手機版  
**Project Type**: Web Application (Razor Pages)  
**Performance Goals**: API p95 回應時間 < 200ms，頁面載入 < 3 秒，換頁 < 1 秒  
**Constraints**: 單一請求記憶體使用 < 100MB，恐龍介紹 ≤ 200 字，純靜態內容展示  
**Scale/Scope**: 8 隻恐龍資料，2 種語言（中文/英文），約 5 個頁面

### 前端技術選擇

**推薦方案**: 原生 JavaScript + Bootstrap 5
- **理由**: 專案已包含 Bootstrap 和 jQuery，兒童故事書網站互動需求簡單
- **優點**: 無需額外設定建構工具、學習曲線低、效能好