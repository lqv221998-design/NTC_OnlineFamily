using System;
using Autodesk.Revit.DB;

namespace NTC.Revit.Utils
{
    public static class RevitFileHelper
    {
        public static (int Version, bool IsLater) GetFamilyInfo(string path)
        {
            if (string.IsNullOrEmpty(path)) return (0, false);

            try
            {
                // Autodesk.Revit.DB.BasicFileInfo is available in Revit API
                // Works for 2020+
                var info = BasicFileInfo.Extract(path);
                
                // Format is usually "2021", "2024", etc.
                if (int.TryParse(info.Format, out int version))
                {
                    return (version, info.IsSavedInLaterVersion);
                }
                
                return (0, info.IsSavedInLaterVersion);
            }
            catch
            {
                // Not a valid Revit file or other error
                return (0, false);
            }
        }
    }
}
