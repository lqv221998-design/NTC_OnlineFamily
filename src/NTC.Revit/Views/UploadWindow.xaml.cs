using System;
using System.Windows;

namespace NTC.Revit.Views
{
    public partial class UploadWindow : Window
    {
        public UploadWindow()
        {
            try 
            {
                // Init Resources strictly using the Fail-Safe App.xaml
                InitializeResources();
                InitializeComponent();
            }
            catch (Exception ex)
            {
                // Capture and display XAML parsing errors
                MessageBox.Show($"UI Crash: {ex.ToString()}", "NTC Failure Scope", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeResources()
        {
            // Centralized loading via RevitResources.xaml
            var url = "pack://application:,,,/NTC.Revit;component/RevitResources.xaml";

            try
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
            }
            catch (Exception ex)
            {
               // Log failure but don't crash main thread here if possible, though needed for styles
               System.Diagnostics.Debug.WriteLine($"[NTC] Warning: Failed to load resource '{url}': {ex.Message}");
            }
        }
    }
}