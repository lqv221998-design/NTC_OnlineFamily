using System;
using System.Windows;

namespace NTC.Revit.Views
{
    public partial class FamilyBrowserWindow : Window
    {
        public FamilyBrowserWindow()
        {
            InitializeMaterialDesign(); // Pre-load resources
            InitializeComponent();
        }

        private void InitializeMaterialDesign()
        {
            // MD 5.0 Setup
            var dicts = new string[]
            {
                "pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.DeepPurple.xaml",
                "pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.Lime.xaml",
                // "MaterialDesignTheme.Light.xaml" is REMOVED in 5.0. Do not load it.
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"
            };

            foreach (var url in dicts)
            {
                try
                {
                    this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(url) });
                }
                catch (Exception ex)
                {
                   MessageBox.Show($"Warning: Failed to load resource '{url}':\n{ex.Message}");
                }
            }

            /* 
            // Explicitly set Light Theme to ensure Brushes are populated
            try
            {
                var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
                var theme = MaterialDesignThemes.Wpf.Theme.Create(
                    MaterialDesignThemes.Wpf.BaseTheme.Light, 
                    System.Windows.Media.Colors.DeepPurple, 
                    System.Windows.Media.Colors.Lime);
                paletteHelper.SetTheme(theme);
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Warning: Failed to set initial theme: {ex.Message}");
            }
            */
        }
    }
}
