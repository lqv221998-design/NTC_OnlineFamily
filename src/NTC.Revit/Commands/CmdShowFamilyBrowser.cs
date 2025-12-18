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
                // DEBUG: Confirm Command Starts
                // TaskDialog.Show("Debug", "Command Started"); // Uncomment if needed

                // FORCE LOAD MaterialDesign Assemblies via AssemblyResolve
                // This is the most robust way to handle 3rd party DLLs in Revit
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                // Force type loading to trigger resolve if needed
                var dummy1 = typeof(MaterialDesignThemes.Wpf.PaletteHelper);
                // MaterialDesignColors 3.x+ uses enums or other types. 'PrimaryColor' is a stable enum.
                var dummy2 = typeof(MaterialDesignColors.PrimaryColor);
                // Also load Microsoft.Xaml.Behaviors (Vital dependency for MD 5.0)
                var dummy3 = typeof(Microsoft.Xaml.Behaviors.Interaction);

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
                // SHOW ERROR DIALOG (Vital for debugging silent failures)
                TaskDialog.Show("Error", $"Command Failed:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                message = ex.Message;
                return Result.Failed;
            }
            finally
            {
                 // Cleanup subscription to avoid memory leaks or side effects
                 AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string folderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string assemblyName = new System.Reflection.AssemblyName(args.Name).Name;
                string assemblyPath = System.IO.Path.Combine(folderPath, assemblyName + ".dll");

                if (System.IO.File.Exists(assemblyPath))
                {
                    return System.Reflection.Assembly.LoadFrom(assemblyPath);
                }
            }
            catch { /* Ignored */ }
            return null;
        }
    }
}
