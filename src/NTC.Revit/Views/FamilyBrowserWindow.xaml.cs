using System;
using System.Windows;

namespace NTC.Revit.Views
{
    public partial class FamilyBrowserWindow : Window
    {
        public FamilyBrowserWindow()
        {
            try
            {
                InitializeResources();
                InitializeComponent();
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"UI Crash: {ex.ToString()}", "NTC Failure Scope", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeResources()
        {
             // Centralized loading via RevitResources.xaml
             var url = "pack://application:,,,/NTC.Revit;component/RevitResources.xaml";
             try
             {
                 this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(url, UriKind.RelativeOrAbsolute) });
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"[NTC] Warning: Failed to load resource '{url}': {ex.Message}");
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
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
