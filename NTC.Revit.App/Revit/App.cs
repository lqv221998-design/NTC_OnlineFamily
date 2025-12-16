using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace NTC.Revit.App.Revit
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // TODO: Add your startup code here (creating Ribbon panels, buttons, etc.)
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // TODO: Add your shutdown code here
            return Result.Succeeded;
        }
    }
}
