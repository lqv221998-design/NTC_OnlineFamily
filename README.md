<div align="center">

# NTC_OnlineFamily
### Cloud-Native Revit Family Manager

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)]()
[![Revit Support](https://img.shields.io/badge/Revit-2020%20%7C%202021%20%7C%202022%20%7C%202023%20%7C%202024%20%7C%202025-blue?style=flat-square&logo=autodeskrevit)]()
[![Platform](https://img.shields.io/badge/.NET-4.8%20%7C%208.0-512BD4?style=flat-square&logo=dotnet)]()
[![License](https://img.shields.io/badge/license-MIT-orange?style=flat-square)]()

</div>

---

## 📖 Introduction

**NTC_OnlineFamily** is a high-performance, open-source Revit Add-in designed to modernize how BIM Coordinators and Architects manage their component libraries. It creates a direct bridge between Autodesk Revit and the Cloud (Supabase), allowing users to search, preview, and insert Revit Families instantly.

What sets this project apart is its **Enterprise-Grade Architecture**. It eliminates the "DLL Hell" of maintaining separate projects for different Revit versions by utilizing **SDK-Style Multi-Targeting**. A single codebase compiles natively for both `.NET Framework 4.8` (Revit 2020-2024) and `.NET 8.0` (Revit 2025).

## ✨ Key Features

- **🌐 Universal Compatibility:** Seamlessly supports Revit 2020 through Revit 2025 using a single solution backbone.
- **☁️ Cloud Repository:** Real-time access to your library hosted on Supabase (PostgreSQL + Storage).
- **⚡ Async-First Design:** Implements modern `async/await` patterns for all network operations, ensuring a **Zero-Freeze UI** experience even during heavy downloads.
- **🚀 Smart Caching:** Intelligent local caching strategy to minimize redundant API calls and accelerate load times.
- **🎨 Modern UI:** Built with **WPF** and **Material Design**, offering a sleek, responsive user interface decoupled from Revit's native UI limitations.

## 🏗 System Architecture

The solution uses a **Clean Architecture** approach with strict separation of concerns, ensuring testability and modularity.

```mermaid
graph TD
    subgraph "Revit Environment"
        Revit[Autodesk Revit (2020-2025)]
    end

    subgraph "NTC_OnlineFamily Solution"
        direction TB
        UI[NTC.Revit.App (UI & Entry Pts)]
        Core[NTC.Core (Logic & Models)]
    end

    subgraph "Cloud Infrastructure"
        Supabase[(Supabase Cloud)]
        DB[(PostgreSQL)]
        Storage[File Storage]
    end

    Revit -.->|Loads| UI
    UI -->|References| Core
    Core -->|RestSharp API| Supabase
    Supabase --> DB
    Supabase --> Storage
```

### 🧠 The Multi-Targeting Strategy
Revit 2025 introduced a major shift from `.NET Framework 4.8` to `.NET 8.0`. Instead of splitting the project, **NTC_OnlineFamily** handles this utilizing SDK-Style properties:

1.  **Shared Kernel (`NTC.Core`)**: Built on `.netstandard2.0`, making business logic compatible with *both* legacy and modern .NET runtimes.
2.  **Adaptive App (`NTC.Revit.App`)**: Configured with `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>`.
3.  **Conditional Compilation**: Code specific to newer APIs uses preprocessor directives:
    ```csharp
    #if NET8_0_OR_GREATER
        // Revit 2025+ (.NET 8) specific implementation
    #else
        // Revit 2020-2024 (.NET 4.8) implementation
    #endif
    ```

## 🛠 Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Platform** | **Revit API** | 2020 - 2025 Support |
| **Language** | **C# 12** | Latest language features |
| **Core Framework** | **.NET Standard 2.0** | ensuring cross-runtime compatibility |
| **UI Framework** | **WPF (MVVM)** | MaterialDesignInXamlToolkit |
| **Backend** | **Supabase** | Managed PostgreSQL & Auth |
| **Networking** | **RestSharp** | Non-blocking HTTP Requests |
| **Package Mgmt** | **Nice3point.Revit.Api** | Dynamic Nuget handling for Revit DLLs |

## 🚀 Getting Started

### Prerequisites
- **Visual Studio 2022** (Required for .NET 8 support).
- **Autodesk Revit** (Any version from 2020 to 2025) installed for debugging.

### Development Setup
1.  **Clone the Repository**
    ```bash
    git clone https://github.com/your-username/NTC_OnlineFamily.git
    cd NTC_OnlineFamily
    ```

2.  **Configuration**
    To connect to the backend, you must configure your API keys. Create a `secrets.json` file in `NTC.Core` (or use User Secrets) with the following structure:
    ```json
    {
      "SupabaseUrl": "YOUR_SUPABASE_URL",
      "SupabaseKey": "YOUR_SUPABASE_ANON_KEY"
    }
    ```

3.  **Build**
    Open `NTC_OnlineFamily.sln` in Visual Studio and Build the Solution.
    - *Note:* NuGet packages will automatically resolve the correct Revit API DLLs based on the target framework.

## 🗺 Roadmap

- [ ] **Drag & Drop Insertion:** Drag families directly from WPF window into Revit viewport.
- [ ] **Batch Uploader:** Admin tool for bulk uploading RFA files to Supabase.
- [ ] **Analytics Dashboard:** Track most used families.
- [ ] **Offline Mode:** Local database sync for improved resilience.

## 👤 Author

**Le Quang Vu**
*Senior Revit API Developer & Solution Architect*

---
*Built with ❤️ for the BIM Community.*