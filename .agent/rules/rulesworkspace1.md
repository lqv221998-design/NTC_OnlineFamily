---
trigger: always_on
---

# PROJECT CONTEXT: NTC_OnlineFamily (Multi-Version Enterprise)
This is a high-level Revit Add-in designed to support a wide range of Revit versions (2020-2025). Code generation must strictly follow Multi-Targeting strategies.

## 1. TECH STACK LOCK (MULTI-TARGETING STRATEGY)
- **Supported Versions:** Revit 2020, 2021, 2022, 2023, 2024, 2025.
- **Target Frameworks (CRITICAL):**
  - The project MUST be configured as **SDK-Style Multi-Targeting**.
  - Strategy: Use `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>`.
  - **.NET 4.8** handles Revit 2020-2024.
  - **.NET 8.0** handles Revit 2025+.
- **Dependencies (NuGet):**
  - DO NOT reference local paths (e.g., `C:\Program Files\...`).
  - MUST use NuGet packages (Recommend: `Nice3point.Revit.Api.RevitApi`) with `Condition` tags to load the correct API version for each framework.
- **Backend:** Supabase (REST API) - Must use libraries compatible with both net48 and net8.0 (e.g., RestSharp).

## 2. SOLUTION ARCHITECTURE
The solution is split to isolate Logic from Revit Versioning conflicts.

### A. Project: `NTC.Core` (The Brain)
- **Target:** `.netstandard2.0` (This ensures compatibility with BOTH .NET 4.8 and .NET 8.0).
- **Role:** Pure Business Logic, Data Models (FamilyItem), API Services.
- **Constraint:** STRICTLY NO references to `RevitAPI.dll`. This allows logic reuse across all Revit versions without recompiling.

### B. Project: `NTC.Revit.App` (The Body)
- **Target:** Multi-target `net48;net8.0-windows`.
- **Role:** UI Views, External Commands, Event Handlers.
- **Coding Convention:**
  - If an API changes between versions (rare for basic load family), use Preprocessor Directives:
    ```csharp
    #if NET8_0_OR_GREATER
      // Code for Revit 2025
    #else
      // Code for Revit 2020-2024
    #endif
    ```

## 3. DATA MAPPING
- **Table:** `families`
- **Columns:** `id`, `name`, `category`, `thumbnail_url`, `file_url`, `revit_version` (integer), `created_at`.