# NTC_OnlineFamily Solution Architecture

> **Document Status:** Live  
> **Maintainer:** Technical Lead  
> **Context:** Clean Architecture + MVVM + Multi-Targeting Strategy

Tài liệu này mô tả chi tiết kiến trúc phần mềm, luồng dữ liệu và vai trò của từng tệp tin trong dự án `NTC_OnlineFamily`.

---

## 1. High-Level Architecture (Kiến trúc Tổng quan)

Hệ thống được thiết kế theo mô hình 3 lớp (3-Tier) tách biệt rõ ràng, đảm bảo tính dễ bảo trì và mở rộng.

```mermaid
graph TD
    subgraph Client_Workstation [Revit Environment]
        style Client_Workstation fill:#e3f2fd,stroke:#333,stroke-width:2px
        
        subgraph Presentation_Layer [NTC.Revit (UI & Commands)]
            CMD[Revit External Commands] -->|Open| VIEW[WPF Views]
            VIEW <-->|Binding| VM[ViewModels]
        end
        
        subgraph Core_Layer [NTC.Core (Business Logic)]
            VM -->|Insects| SERVICE[Supabase Service]
            SERVICE -->|Uses| MODEL[Data Models]
            SERVICE -->|Validates| DTO[DTOs]
        end
    end

    subgraph Cloud_Infrastructure [Supabase Cloud]
        style Cloud_Infrastructure fill:#e8f5e9,stroke:#333,stroke-width:2px
        SERVICE <== HTTPS/REST ==> API[PostgREST API]
        API <--> DB[(PostgreSQL Database)]
        API <--> STORAGE[[Storage Bucket]]
    end
    
    %% Relationships
    CMD -.->|Dependency| VM
    VM -.->|Dependency| SERVICE
```

### Nguyên lý thiết kế

1. **Dependency Rule:** Lớp `Core` không biết gì về `Revit` (Dependency hướng vào trong). Điều này giúp Logic Supabase có thể tái sử dụng cho Website hoặc Console App khác.
2. **Multi-Targeting:** `NTC.Revit` được biên dịch song song cho `.NET Framework 4.8` (Revit 2020-2024) và `.NET 8.0` (Revit 2025+).
3. **MVVM (Model-View-ViewModel):** Tách biệt hoàn toàn giao diện (View) khỏi logic xử lý (ViewModel).

---

## 2. Project Structure & File Dictionary (Giải phẫu thư mục)

Dưới đây là bản đồ chi tiết chức năng của từng file trong dự án.

```text
NTC_OnlineFamily/
├── .github/workflows/
│   └── build.yml               # [CI/CD] Tự động Build & Test
│
├── AlphaBIM_TemplateRevit2023_WPF/ # [TEMPLATE] Mã nguồn tham khảo giao diện
├── Database/
│   └── init_schema.sql         # [DB] Script khởi tạo PostgreSQL Supabase
│
├── NTC_OnlineFamily.addin      # [MANIFEST] File đăng ký Add-in với Revit
├── NTC_OnlineFamily.sln
│
├── src/
│   ├── NTC.Core/               # [THE BRAIN] Logic cốt lõi (.netstandard2.0)
│   │   ├── DTOs/               # Data Transfer Objects
│   │   │   ├── FamilyUploadDto.cs
│   │   │   └── FamilySearchDto.cs
│   │   ├── Exceptions/         # Custom Exceptions (SupabaseException...)
│   │   ├── Interfaces/         # Contracts (ISupabaseService)
│   │   ├── Models/             # Data Models (FamilyModel, BaseModel...)
│   │   ├── Secrets/            # [SENSITIVE] Quản lý Config bảo mật
│   │   ├── Services/
│   │   │   └── SupabaseService.cs # [IMPL] Giao tiếp Supabase API
│   │   └── NTC.Core.csproj
│   │
│   ├── NTC.Revit/              # [THE BODY] Revit Add-in UI & Commands
│   │   ├── Commands/
│   │   │   ├── CmdShowFamilyBrowser.cs # Lệnh mở kho thư viện
│   │   │   └── CmdShowUploadWindow.cs  # Lệnh mở cửa sổ Upload
│   │   ├── Resources/
│   │   │   └── Styles.xaml     # [UI KIT] Global Styles (Colors, Buttons)
│   │   ├── Revit/              # Revit-specific Logic
│   │   │   └── App.cs          # (Nếu có) IExternalApplication
│   │   ├── Utils/
│   │   │   └── RevitFileHelper.cs # Utility đọc file .rfa
│   │   ├── ViewModels/         # ViewModels cho WPF
│   │   │   ├── FamilyBrowserViewModel.cs
│   │   │   ├── UploadViewModel.cs
│   │   │   └── Base/           # Base Classes (AsyncRelayCommand)
│   │   ├── Views/              # WPF Windows & UserControls
│   │   │   ├── FamilyBrowserWindow.xaml
│   │   │   ├── FamilyBrowserView.xaml
│   │   │   └── UploadWindow.xaml
│   │   └── NTC.Revit.csproj    # Multi-targeting (net48;net8.0-windows)
│
└── tests/                      # [TESTING]
    └── NTC.Core.Tests/
        ├── GovernanceComplianceTests.cs
        └── secrets.json        # Test Configuration
│
└── README.md                   # Tài liệu hướng dẫn cài đặt và sử dụng cơ bản.
```

## 3. Key Technical Decisions (Các quyết định kỹ thuật quan trọng)

### 3.1. Tại sao dùng `.netstandard2.0` cho NTC.Core?

* Để viết code **một lần** nhưng chạy được trên cả môi trường Revit cũ (.NET Framework 4.8) và Revit mới 2025 (.NET 8).
* Giúp tách biệt logic thuần túy khỏi sự phụ thuộc vào thư viện `RevitAPI.dll` nặng nề.

### 3.2. Tại sao dùng Singleton cho SupabaseService?

* Việc khởi tạo kết nối HTTP Client (`RestClient`) tốn tài nguyên. Singleton đảm bảo chỉ có **duy nhất một kết nối** được tái sử dụng trong suốt vòng đời của Revit session, tối ưu hiệu năng.

### 3.3. Assembly Resolve trong Commands

* Môi trường Revit Add-in rất kén chọn việc load các file DLL bên thứ 3 (như `RestSharp`, `MaterialDesignThemes`).
* Code trong `CmdShow...cs` đăng ký sự kiện `AssemblyResolve` để chỉ đường cho Revit tìm thấy đúng file DLL nằm trong thư mục Add-in.

---
*Created by NTC Technical Team*
