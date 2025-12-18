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
                // Init Resources strictly
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
            // MD 5.0 Setup & Styles.xaml loading via Pack URI
            var dicts = new string[]
            {
                "pack://application:,,,/NTC.Revit;component/App.xaml"
            };

            foreach (var url in dicts)
            {
                try
                {
                    var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                    this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                }
                catch (Exception ex)
                {
                   System.Diagnostics.Debug.WriteLine($"[NTC] Warning: Failed to load resource '{url}': {ex.Message}");
                }
            }
        }
    }
}
