using System;
using System.Windows;

namespace NTC.Revit.Views
{
    public partial class UploadWindow : Window
    {
        public UploadWindow()
        {
            InitializeMaterialDesign();
            InitializeComponent();
        }

        private void InitializeMaterialDesign()
        {
            // MD 5.0 Setup
            var dicts = new string[]
            {
                "pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.DeepPurple.xaml",
                "pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.Lime.xaml",
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"
            };

            foreach (var url in dicts)
            {
                try
                {
                    var uri = new Uri(url);
                    // Check if already merged to verify avoiding duplicates? No, just add.
                    this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                }
                catch (Exception ex)
                {
                   // Log to Debug output instead of showing intrusive MessageBox
                   System.Diagnostics.Debug.WriteLine($"[NTC] Warning: Failed to load resource '{url}': {ex.Message}");
                }
            }
        }
    }
}
