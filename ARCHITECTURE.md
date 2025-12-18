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
│   └── build.yml               # [CI/CD] Tự động Build & Test mỗi khi Push code. Đảm bảo code không bị lỗi compilation trên cả 2 nền tảng .NET.
│
├── NTC_OnlineFamily.addin      # [MANIFEST] "Tấm bản đồ" giúp Revit tìm thấy và load file DLL khi khởi động.
├── NTC_OnlineFamily.sln        # [SOLUTION] File quản lý toàn bộ dự án Visual Studio.
│
├── src/
│   ├── NTC.Core/               # [THE BRAIN] Bộ não xử lý logic, độc lập với Revit API.
│   │   ├── Models/
│   │   │   ├── BaseModel.cs        # Class cha chứa các trường chung (Id, CreatedAt) cho mọi bảng DB.
│   │   │   ├── FamilyModel.cs      # Map trực tiếp với bảng 'families' trong DB. Chứa Validation (DAMA) cho dữ liệu.
│   │   │   └── Enums.cs            # Định nghĩa các hằng số (Status: Pending/Approved) để tránh Hardcode string.
│   │   ├── DTOs/
│   │   │   ├── FamilyUploadDto.cs  # "Gói tin" chứa dữ liệu thô từ Form Upload gửi vào Service.
│   │   │   └── FamilySearchDto.cs  # "Gói tin" chứa tiêu chí tìm kiếm family.
│   │   ├── Services/
│   │   │   └── SupabaseService.cs  # [SINGLETON] Quản lý kết nối Cloud. Xử lý Upload/Download, Auth và Retry logic.
│   │   ├── Interfaces/
│   │   │   └── ISupabaseService.cs # [CONTRACT] Bản cam kết interface, giúp ViewModel không phụ thuộc implementation cụ thể (Dễ Unit Test).
│   │   ├── Exceptions/         # Các lỗi tùy chỉnh (Custom Exception) để debug dễ hơn.
│   │   ├── secrets.json        # [SENSITIVE] Chứa API Key & URL Supabase (Tuyệt đối KHÔNG commit lên Git).
│   │   └── NTC.Core.csproj     # Target .netstandard2.0 để tương thích với cả Net4.8 và Net8.0.
│   │
│   ├── NTC.Revit/              # [THE BODY] Cơ thể chứa giao diện và tương tác với Revit.
│   │   ├── Commands/
│   │   │   ├── CmdShowFamilyBrowser.cs # Lệnh mở kho thư viện. Xử lý AssemblyResolve để load DLL phụ thuộc.
│   │   │   └── CmdShowUploadWindow.cs  # Lệnh mở cửa sổ Upload.
│   │   ├── ViewModels/         # [MVVM]
│   │   │   ├── Base/
│   │   │   │   ├── ViewModelBase.cs    # Implement INotifyPropertyChanged.
│   │   │   │   └── AsyncRelayCommand.cs# Xử lý các Button Click dạng Async (Tránh đóng băng UI Revit).
│   │   │   ├── FamilyBrowserViewModel.cs # Logic "Search & Download". Gọi SupabaseService để lấy list family.
│   │   │   └── UploadViewModel.cs        # Logic "Upload & Validate". Gọi SupabaseService để đẩy file.
│   │   ├── Views/              # [WPF]
│   │   │   ├── FamilyBrowserWindow.xaml  # Cửa sổ chính xem thư viện.
│   │   │   ├── FamilyBrowserView.xaml    # UserControl chứa giao diện danh sách (tách nhỏ để dễ quản lý).
│   │   │   └── UploadWindow.xaml         # Cửa sổ Upload (Thiết kế style AlphaBIM).
│   │   ├── Resources/
│   │   │   └── Styles.xaml     # [UI KIT] Định nghĩa màu sắc (Revit Blue), Button style, Font chữ dùng chung.
│   │   ├── Utils/
│   │   │   └── RevitFileHelper.cs # Helper đọc phiên bản Revit của file .rfa (Dùng binary reading để không cần mở file).
│   │   └── NTC.Revit.csproj    # [CRITICAL] Cấu hình Multi-targeting: `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>`.
│   │
│   └── NTC.Core.Tests/         # [QUALITY CONTROL]
│       ├── GovernanceComplianceTests.cs # Kiểm tra tuân thủ quy tắc quản trị dữ liệu.
│       └── secrets.json        # Mock secrets cho môi trường test.
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
