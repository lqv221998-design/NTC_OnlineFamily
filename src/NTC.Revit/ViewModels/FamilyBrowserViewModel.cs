using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NTC.Core.Interfaces;
using NTC.Core.Models;
using NTC.Core.Services;
using NTC.Revit.ViewModels.Base;

namespace NTC.Revit.ViewModels
{
    public class FamilyBrowserViewModel : ViewModelBase
    {
        private readonly ISupabaseService _supabaseService;
        private string _searchText;
        private bool _isLoading;
        private string _statusMessage;
        
        // This should theoretically come from the Revit Application context
        private int _currentRevitVersion = 2025; 

        public FamilyBrowserViewModel()
        {
            // Dependency Injection (Manually resolved for now, or via ServiceLocator in real app)
            _supabaseService = SupabaseService.Instance;

            Families = new ObservableCollection<FamilyModel>();

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync, OnException);
            DownloadFamilyCommand = new AsyncRelayCommand<FamilyModel>(DownloadFamilyAsync, OnException);
        }

        // Properties
        public ObservableCollection<FamilyModel> Families { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Optional: Trigger filter logic here if local filtering is desired
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Commands
        public ICommand LoadDataCommand { get; }
        public ICommand DownloadFamilyCommand { get; }

        // Logic
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading families...";
            Families.Clear();

            try
            {
                var result = await _supabaseService.GetApprovedFamiliesAsync(_currentRevitVersion);

                // Simple Client-side Search Filter (in production, use API search)
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    result = result.Where(f => f.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                foreach (var family in result)
                {
                    Families.Add(family);
                }

                StatusMessage = $"Found {Families.Count} families for Revit {_currentRevitVersion}.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DownloadFamilyAsync(FamilyModel family)
        {
            if (family == null) return;

            IsLoading = true;
            StatusMessage = $"Downloading {family.Name}...";

            try
            {
                // TODO: Implement actual file download logic in SupabaseService or here.
                // For now we simulate a delay. 
                // In reality: await _supabaseService.DownloadFileAsync(family.Url, localPath);
                
                await Task.Delay(1000); // Simulation

                StatusMessage = $"Ready to insert: {family.Name}";
                
                // Trigger Revit External Event here to LoadFamily
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnException(Exception ex)
        {
            IsLoading = false;
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
