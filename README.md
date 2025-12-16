# NTC_OnlineFamily - Trình quản lý Family Revit thông minh

## 🚀 Tổng quan
NTC_OnlineFamily là một Add-in Revit hiệu suất cao được thiết kế để quản lý, tìm kiếm và chèn các Family Revit trực tiếp từ đám mây bằng giao diện WPF hiện đại.

## 🏗 Kiến trúc
- **Mẫu**: MVVM (Model-View-ViewModel)

- **Lõi**: .NET Framework 4.8
- **Giao diện người dùng**: WPF + Material Design XAML
- **Phần phụ trợ**: Supabase (PostgreSQL + Xác thực)

## 📂 Cấu trúc dự án
- **NTC.Core**: Chứa logic nghiệp vụ, dịch vụ API và ViewModels (độc lập với Revit nếu có thể).

- **NTC.Revit.App**: Chứa các điểm truy cập Revit (IExternalApplication) và các View WPF.

## 👨‍💻 Công nghệ sử dụng
- Revit API 2024
- C# Async/Await
- RestSharp