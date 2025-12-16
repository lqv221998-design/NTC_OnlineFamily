<div align="center">

# NTC_OnlineFamily
### Cloud-Native Revit Family Manager

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)]()
[![Revit Support](https://img.shields.io/badge/Revit-2020%20%7C%202021%20%7C%202022%20%7C%202023%20%7C%202024%20%7C%202025-blue?style=flat-square&logo=autodeskrevit)]()
[![Platform](https://img.shields.io/badge/.NET-4.8%20%7C%208.0-512BD4?style=flat-square&logo=dotnet)]()
[![License](https://img.shields.io/badge/license-MIT-orange?style=flat-square)]()

</div>

---

## 📖 Giới thiệu

**NTC_OnlineFamily** là một Add-in Revit mã nguồn mở hiệu suất cao, được thiết kế để hiện đại hóa quy trình quản lý thư viện component cho BIM Coordinators và Kiến trúc sư. Ứng dụng tạo ra một cầu nối trực tiếp giữa Autodesk Revit và Cloud (Supabase), cho phép người dùng tìm kiếm, xem trước và chèn Revit Family ngay lập tức.

Điểm đặc biệt của dự án này là **Kiến trúc cấp Doanh nghiệp (Enterprise-Grade Architecture)**. Nó giải quyết triệt để vấn đề "DLL Hell" khi bảo trì nhiều phiên bản Revit bằng cách sử dụng chiến lược **SDK-Style Multi-Targeting**. Chỉ một codebase duy nhất có thể biên dịch native cho cả `.NET Framework 4.8` (Revit 2020-2024) và `.NET 8.0` (Revit 2025).

## ✨ Tính năng nổi bật

- **🌐 Tương thích toàn diện:** Hỗ trợ mượt mà từ Revit 2020 đến Revit 2025 chỉ với một giải pháp duy nhất.
- **☁️ Kho lưu trữ đám mây:** Truy cập thời gian thực vào thư viện được lưu trữ trên Supabase (PostgreSQL + Storage).
- **⚡ Thiết kế Async-First:** Áp dụng triệt để mô hình `async/await` cho mọi thao tác mạng, đảm bảo trải nghiệm **Zero-Freeze UI** (không treo giao diện) ngay cả khi tải dữ liệu nặng.
- **🚀 Caching thông minh:** Chiến lược lưu bộ nhớ đệm cục bộ (local caching) giúp giảm thiểu các lệnh gọi API dư thừa và tăng tốc độ tải.
- **🎨 Giao diện hiện đại:** Được xây dựng bằng **WPF** và **Material Design**, mang lại giao diện người dùng đẹp mắt, linh hoạt và tách biệt hoàn toàn khỏi các hạn chế UI mặc định của Revit.

## 🏗 Kiến trúc hệ thống

Giải pháp sử dụng hướng tiếp cận **Clean Architecture** với sự phân tách rõ ràng các trách nhiệm, đảm bảo khả năng kiểm thử (testability) và tính mô-đun hóa.

```text
+-----------------------------------+             +----------------------------------+
|   Revit Environment (2020-2025)   |             |       Cloud Infrastructure       |
|                                   |             |                                  |
|   +---------------------------+   |             |   +--------------------------+   |
|   |      Autodesk Revit       |   |   HTTPS     |   |      Supabase Cloud      |   |
|   |         (Host)            |<--|------------>|   |    (PostgreSQL/Auth)     |   |
|   +-------------+-------------+   |   JSON      |   +-------------+------------+   |
|                 | Loads           |             |                 | Stores         |
|                 v                 |             |                 v                |
|   +---------------------------+   |             |   +--------------------------+   |
|   |      NTC.Revit.App        |   |             |   |      Cloud Storage       |   |
|   |    (UI / Entry Points)    |   |             |   |       (RFA Files)        |   |
|   +-------------+-------------+   |             |   +--------------------------+   |
|                 | References      |             |                                  |
|                 v                 |             |                                  |
|   +---------------------------+   |             |                                  |
|   |        NTC.Core           |   |             |                                  |
|   |    (Business Logic)       |   |             |                                  |
|   |    (.netstandard2.0)      |   |             |                                  |
|   +---------------------------+   |             |                                  |
+-----------------------------------+             +----------------------------------+
```

### 🧠 Chiến lược Multi-Targeting
Revit 2025 đánh dấu sự chuyển đổi lớn từ `.NET Framework 4.8` sang `.NET 8.0`. Thay vì tách dự án, **NTC_OnlineFamily** xử lý vấn đề này bằng cách tận dụng các thuộc tính SDK-Style:

1.  **Shared Kernel (`NTC.Core`)**: Được xây dựng trên `.netstandard2.0`, giúp logic nghiệp vụ tương thích với *cả* legacy và modern .NET runtimes.
2.  **Adaptive App (`NTC.Revit.App`)**: Cấu hình với `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>`.
3.  **Conditional Compilation**: Code dành riêng cho API mới sử dụng các chỉ thị tiền xử lý:
    ```csharp
    #if NET8_0_OR_GREATER
        // Triển khai riêng cho Revit 2025+ (.NET 8)
    #else
        // Triển khai cho Revit 2020-2024 (.NET 4.8)
    #endif
    ```

## 🛠 Công nghệ sử dụng

| Thành phần | Công nghệ | Mô tả |
| :--- | :--- | :--- |
| **Nền tảng** | **Revit API** | Hỗ trợ 2020 - 2025 |
| **Ngôn ngữ** | **C# 12** | Sử dụng các tính năng ngôn ngữ mới nhất |
| **Core Framework** | **.NET Standard 2.0** | Đảm bảo tương thích đa runtime |
| **UI Framework** | **WPF (MVVM)** | MaterialDesignInXamlToolkit |
| **Backend** | **Supabase** | Managed PostgreSQL & Auth |
| **Mạng (Networking)** | **RestSharp** | Non-blocking HTTP Requests |
| **Quản lý gói** | **Nice3point.Revit.Api** | Tự động xử lý Revit DLLs qua Nuget |

## 🚀 Bắt đầu

### Yêu cầu hệ thống
- **Visual Studio 2022** (Yêu cầu bắt buộc để hỗ trợ .NET 8).
- **Autodesk Revit** (Bất kỳ phiên bản nào từ 2020 đến 2025) đã được cài đặt để debug.

### Thiết lập môi trường phát triển
1.  **Clone Repository**
    ```bash
    git clone https://github.com/your-username/NTC_OnlineFamily.git
    cd NTC_OnlineFamily
    ```

2.  **Cấu hình**
    Để kết nối với backend, bạn cần cấu hình API keys. Tạo file `secrets.json` trong thư mục `NTC.Core` (hoặc sử dụng User Secrets) với cấu trúc sau:
    ```json
    {
      "SupabaseUrl": "YOUR_SUPABASE_URL",
      "SupabaseKey": "YOUR_SUPABASE_ANON_KEY"
    }
    ```

3.  **Build**
    Mở `NTC_OnlineFamily.sln` trong Visual Studio và Build Solution.
    - *Lưu ý:* Các gói NuGet sẽ tự động phân giải các Revit API DLLs chính xác dựa trên target framework.

## 🗺 Lộ trình phát triển (Roadmap)

- [ ] **Kéo & Thả (Drag & Drop):** Kéo family trực tiếp từ cửa sổ WPF vào viewport của Revit.
- [ ] **Batch Uploader:** Công cụ quản trị để upload hàng loạt file RFA lên Supabase.
- [ ] **Dashboard Phân tích:** Theo dõi các family được sử dụng nhiều nhất.
- [ ] **Chế độ Offline:** Đồng bộ database cục bộ để tăng tính ổn định.

## 👤 Tác giả

**Lê Quang Vũ**
*Senior Revit API Developer & Solution Architect*

---
*Được xây dựng với niềm đam mê dành cho cộng đồng BIM.*