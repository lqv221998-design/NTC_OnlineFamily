using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NTC.Revit.ViewModels;
using NTC.Revit.Views;

namespace NTC.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdShowFamilyBrowser : IExternalCommand
    {
        // Singleton window reference per Revit session
        private static FamilyBrowserWindow _window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (_window == null || !_window.IsLoaded)
                {
                    // 1. Initialize ViewModel
                    var viewModel = new FamilyBrowserViewModel();

                    // 2. Initialize Window
                    _window = new FamilyBrowserWindow
                    {
                        DataContext = viewModel
                    };

                    // 3. Set Owner Logic (Crucial for Revit Add-ins)
                    // Keeps the window on top of Revit
                    var helper = new WindowInteropHelper(_window);
                    helper.Owner = Process.GetCurrentProcess().MainWindowHandle;

                    // 4. Cleanup on close
                    _window.Closed += (s, e) => _window = null;

                    _window.Show();
                }
                else
                {
                    _window.Activate();
                    if (_window.WindowState == System.Windows.WindowState.Minimized)
                    {
                        _window.WindowState = System.Windows.WindowState.Normal;
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
