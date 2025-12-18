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
    public class CmdShowUploadWindow : IExternalCommand
    {
        private static UploadWindow _window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Robust dependency loading
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var dummy1 = typeof(MaterialDesignThemes.Wpf.PaletteHelper);
                var dummy2 = typeof(MaterialDesignColors.PrimaryColor);
                var dummy3 = typeof(Microsoft.Xaml.Behaviors.Interaction);

                if (_window == null || !_window.IsLoaded)
                {
                    _window = new UploadWindow
                    {
                        DataContext = new UploadViewModel()
                    };

                    var helper = new WindowInteropHelper(_window);
                    helper.Owner = Process.GetCurrentProcess().MainWindowHandle;
                    _window.Closed += (s, e) => _window = null;
                    _window.Show();
                }
                else
                {
                    _window.Activate();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to open Upload Window:\n{ex.Message}");
                message = ex.Message;
                return Result.Failed;
            }
            finally
            {
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
                    return System.Reflection.Assembly.LoadFrom(assemblyPath);
            }
            catch { }
            return null;
        }
    }
}
