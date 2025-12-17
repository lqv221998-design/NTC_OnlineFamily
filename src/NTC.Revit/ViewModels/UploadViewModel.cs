using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NTC.Core.DTOs;
using NTC.Core.Interfaces;
using NTC.Core.Services;
using NTC.Revit.Utils;
using NTC.Revit.ViewModels.Base;

namespace NTC.Revit.ViewModels
{
    public class UploadViewModel : ViewModelBase
    {
        private readonly ISupabaseService _supabaseService;
        
        private string _filePath;
        private string _fileName;
        private string _category = "Generic Models"; // Default or detect
        private int _detectedVersion;
        private bool _isUploading;
        private string _statusMessage;
        private string _errorMessage;

        public UploadViewModel()
        {
            _supabaseService = SupabaseService.Instance;
            SelectFileCommand = new RelayCommand(SelectFile);
            SubmitCommand = new AsyncRelayCommand(SubmitUploadAsync, OnException, CanSubmit);
        }

        // Properties
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value))
                {
                    AnalyzeFile(value);
                }
            }
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public int DetectedVersion
        {
            get => _detectedVersion;
            set => SetProperty(ref _detectedVersion, value);
        }

        public bool IsUploading
        {
            get => _isUploading;
            set
            {
                SetProperty(ref _isUploading, value);
                // Force re-evaluate CanSubmit
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Commands
        public ICommand SelectFileCommand { get; }
        public ICommand SubmitCommand { get; }

        private void SelectFile(object obj)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Revit Families (*.rfa)|*.rfa",
                Title = "Select Revit Family"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }

        private void AnalyzeFile(string path)
        {
            ErrorMessage = null;
            StatusMessage = "Analyzing file...";
            
            if (!File.Exists(path))
            {
                ErrorMessage = "File does not exist.";
                return;
            }

            FileName = Path.GetFileNameWithoutExtension(path);

            // "Data Governance": Trust code, extract from file header
            var info = RevitFileHelper.GetFamilyInfo(path);
            
            if (info.Version == 0)
            {
                ErrorMessage = "Invalid Revit Family file or unable to read version.";
                DetectedVersion = 0;
            }
            else
            {
                DetectedVersion = info.Version;
                StatusMessage = $"Detected Revit {info.Version}. Ready to upload.";
            }
        }

        private bool CanSubmit(object obj)
        {
            return !IsUploading 
                   && !string.IsNullOrEmpty(FilePath) 
                   && DetectedVersion > 0 
                   && string.IsNullOrEmpty(ErrorMessage);
        }

        private async Task SubmitUploadAsync()
        {
            IsUploading = true;
            StatusMessage = "Uploading...";
            ErrorMessage = null;

            try
            {
                var dto = new FamilyUploadDto
                {
                    Name = FileName,
                    Category = Category,
                    RevitVersion = DetectedVersion,
                    FilePath = FilePath
                };

                bool success = await _supabaseService.UploadFamilyAsync(dto, FilePath);

                if (success)
                {
                    StatusMessage = "Upload Successful!";
                    // Reset or Close window logic could go here
                }
            }
            finally
            {
                IsUploading = false;
            }
        }

        private void OnException(Exception ex)
        {
            IsUploading = false;
            ErrorMessage = $"Error: {ex.Message}";
        }
    }
}
