---
description: Tạo trọn bộ Model - View - ViewModel cho một màn hình
---

# MISSION: GENERATE MVVM COMPONENT
The user wants a new UI Component (e.g., Login, FamilyBrowser).

1. **Model:** Define the Data Model in `NTC.Core`.
2. **ViewModel:** Create a ViewModel in `NTC.Core` implementing `INotifyPropertyChanged` (or using a BaseViewModel).
   - Must handle commands (ICommand) for button clicks.
   - Must use ObservableCollection for lists.
3. **View:** Create the WPF XAML in `NTC.Revit.App`.
   - Use `MaterialDesignInXaml` controls.
   - Set `d:DataContext` for design-time data.
   - **Crucial:** Explain where to initialize this View and bind the DataContext in the Revit logic.