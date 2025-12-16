<div align="center">

# NTC_OnlineFamily
### Enterprise Data-Centric Revit CMS

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)]()
[![Methodology](https://img.shields.io/badge/Methodology-DAMA--DMBOK-blue?style=flat-square)]()
[![Architecture](https://img.shields.io/badge/Architecture-Clean-orange?style=flat-square)]()
[![Platform](https://img.shields.io/badge/Revit-2020%20--%202025-red?style=flat-square)]()

</div>

---

## 📖 Executive Summary

**NTC_OnlineFamily** không chỉ là một Add-in Revit thông thường. Đây là một **Hệ thống Quản lý Nội dung (CMS)** được thiết kế dựa trên các nguyên lý kỹ thuật dữ liệu tiên tiến, coi Revit Family là tài sản dữ liệu cốt lõi của doanh nghiệp.

Dự án này được xây dựng dựa trên triết lý từ 3 tác phẩm kinh điển:
1.  **DAMA-DMBOK:** Chuẩn hóa Quản trị dữ liệu (Data Governance) và kiểm soát Metadata.
2.  **Designing Data-Intensive Applications (DDIA - Martin Kleppmann):** Đảm bảo tính Tin cậy (Reliability), Khả năng mở rộng (Scalability) và Bảo trì (Maintainability).
3.  **Fundamentals of Data Engineering:** Tối ưu hóa pipeline dữ liệu từ Ingestion đến Serving.

---

## 1. 🧠 Conceptual Framework (Khung lý thuyết)

### Data as an Asset (Dữ liệu là Tài sản)
Trong kiến trúc này, một Revit Family không chỉ là một file `.rfa` vô tri. Nó là một thực thể dữ liệu bao gồm:
-   **Core Data (Blob):** File nhị phân `.rfa`.
-   **Metadata:** Thông tin mô tả (Category, Parameters, Version, Tags) giúp khả năng tìm kiếm (Discoverability) đạt hiệu quả cao.

### Reliability First (Ưu tiên tính Tin cậy)
Lấy cảm hứng từ *DDIA*, hệ thống được thiết kế để "Crash-free".
-   **Async/Await Pattern:** Mọi tác vụ I/O (Network, Disk) đều được xử lý bất đồng bộ để đảm bảo **Zero-blocking UI**. Giao diện Revit không bao giờ bị "treo" (Not Responding) khi đang tải dữ liệu.
-   **Fail-Safe Mechanisms:** Sử dụng `Try-Catch` ở các ranh giới kiến trúc (Boundaries) để cô lập lỗi. Nếu kết nối mạng thất bại, hệ thống sẽ degrade (giảm cấp) nhẹ nhàng thay vì crash toàn bộ ứng dụng.

---

## 2. 🏗 Architecture & Data Flow (Luồng dữ liệu)

Hệ thống tuân thủ vòng đời dữ liệu chuẩn của *Data Engineering*: **Ingestion -> Storage -> Serving**.

```text
+-------------------------------------------------------------+
|               Data Engineering Lifecycle                    |
+-------------------------------------------------------------+

[1. INGESTION]          [2. STORAGE]            [3. SERVING]
(Generation)                                    (Consumption)

+-------------+        +-------------+         +-------------+
| Revit Admin |----->  |  SUPABASE   |  -----> | Revit User  |
| (Uploader)  | HTTPS  | ( The Lake) |  HTTPS  | (Consumer)  |
+-------------+        +-------------+         +-------------+
       |                      |                       |
       | Extract              | Split                 | Lazy Load
       v                      v                       v
 +------------+        +---------------+       +---------------+
 | Validation |        | PostgreSQL    |       |  Metadata     |
 | & Metadata |        | (Structured)  |       |  First        |
 +------------+        +---------------+       | (Search UI)   |
                       | Storage Bkt   |       +---------------+
                       | (Unstructured)|               |
                       +---------------+               v
                                               +---------------+
                                               |  Download     |
                                               |  On-Demand    |
                                               +---------------+
```

### Chi tiết pipeline:
1.  **Ingestion (Nạp dữ liệu):**
    -   Hệ thống tự động trích xuất Metadata từ file Revit trước khi upload.
    -   Validate dữ liệu đầu vào (Naming naming convention Check) ngay tại Client để giảm thiểu "Garbage In, Garbage Out".
2.  **Storage (Lưu trữ - Hybrid approach):**
    -   **PostgreSQL:** Lưu trữ Metadata có cấu trúc (Tên, Loại, Kích thước) cho các truy vấn SQL phức tạp và nhanh chóng (High Throughput).
    -   **Object Storage:** Lưu trữ file `.rfa` và ảnh thumbnail `.png` dưới dạng Unstructured Data (Blob).
3.  **Serving (Phân phối):**
    -   **Lazy Loading:** Client chỉ tải Metadata (nhẹ, dạng JSON) để hiển thị danh sách. File `.rfa` (nặng) chỉ được tải xuống khi người dùng thực sự thực hiện lệnh "Insert". Giảm độ trễ (Latency) và tiết kiệm băng thông.

---

## 3. 🛡 Data Governance & Security (Quản trị dữ liệu)

Theo chuẩn **DAMA-DMBOK**:

### Single Source of Truth (SSOT)
Loại bỏ tình trạng "Data Silos" (dữ liệu phân mảnh trên từng máy cá nhân). Supabase đóng vai trò là kho lưu trữ tập trung duy nhất, đảm bảo tính nhất quán (Consistency).

### Access Control (Kiểm soát truy cập)
Sử dụng **Supabase Auth (RLS - Row Level Security)**:
-   **Read-Only:** Người dùng phổ thông chỉ có quyền `SELECT`.
-   **Admin/Manager:** Chỉ nhóm quản trị mới có quyền `INSERT`, `UPDATE`, `DELETE`.
Mô hình này bảo vệ tính toàn vẹn dữ liệu (Data Integrity) ngay từ lớp Database.

### Metadata Management
Mỗi Family được gắn tag phiên bản Revit (2020-2025). Hệ thống tự động filter để đảm bảo người dùng Revit 2020 không tải nhầm Family của bản 2024 (tránh lỗi phiên bản không tương thích).

---

## 4. 💻 Technical Implementation (Cài đặt kỹ thuật)

### Multi-Targeting Strategy
Giải quyết bài toán phân mảnh phiên bản Revit mà không cần duy trì nhiều nhánh code.
-   **Shared Kernel (.netstandard 2.0):** Chứa Business Logic thuần túy, tái sử dụng cho mọi phiên bản.
-   **Adaptive UI (.NET 4.8 / .NET 8):** Build song song cho 2 nền tảng runtime.

### Maintainability (Khả năng bảo trì)
Tuân thủ **Clean Architecture**:
-   **Core:** Entities, Interfaces (Không phụ thuộc bên ngoài).
-   **Infrastructure:** Triển khai API, Database (Phụ thuộc Core).
-   **Presentation (App):** WPF MVVM (Phụ thuộc Core).

---

## 5. 🚀 Getting Started

### Yêu cầu cài đặt
1.  **Visual Studio 2022** (hỗ trợ .NET 8 SDK).
2.  **Supabase Project:** Tạo project mới và lấy URL/Anon Key.

### Cấu hình
Tạo file `secrets.json` trong project `NTC.Core`:
```json
{
  "SupabaseUrl": "https://xyz.supabase.co",
  "SupabaseKey": "eyJh..."
}
```

### Roadmap (Kế hoạch phát triển)
-   [ ] **Analytics Dashboard:** Đo lường mức độ sử dụng Family (User Adoption Rate).
-   [ ] **Version Control:** Theo dõi lịch sử thay đổi của từng Family.
-   [ ] **Offline Sync:** Cơ chế Eventual Consistency cho phép làm việc khi mất mạng.

---
**Author:** Le Quang Vu - *Data-Driven Solution Architect*