using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace NTC.Revit.App.Revit
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Register Assembly Resolver to load DLLs (Supabase, Newtonsoft) from the same folder
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // TODO: Add your startup code here (creating Ribbon panels, buttons, etc.)
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Unregister Assembly Resolver
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

            // TODO: Add your shutdown code here
            return Result.Succeeded;
        }

        /// <summary>
        /// Handlers resolving missing assemblies. 
        /// Revit might not find DLLs in the addin folder automatically if they are not in the main Revit bin.
        /// </summary>
        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                // Get the folder where this executing assembly (NTC.Revit.dll) is located
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Construct the path to the requested assembly
                string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                string fullPath = Path.Combine(assemblyPath, assemblyName);

                if (File.Exists(fullPath))
                {
                    return Assembly.LoadFrom(fullPath);
                }
            }
            catch (Exception)
            {
                // Log error if needed, but usually silent fail here matches default behavior
            }

            return null;
        }
    }
}