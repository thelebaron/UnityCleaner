using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityCleaner.Models;
using UnityCleaner.Services;

namespace UnityCleaner.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ProjectScannerService _scannerService;
        private readonly ProjectCleanerService _cleanerService;
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private string _currentDirectory;

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private bool _isCleaning;

        [ObservableProperty]
        private string _statusMessage;

        public ObservableCollection<UnityProject> Projects { get; } = new();
        public ObservableCollection<string> RecentDirectories { get; } = new();
        public ObservableCollection<CleanPattern> CleanPatterns { get; } = new();

        public MainWindowViewModel()
        {
            _scannerService = new ProjectScannerService();
            _cleanerService = new ProjectCleanerService();
            _settingsService = new SettingsService();

            CurrentDirectory = string.Empty;
            StatusMessage = "Ready to scan for Unity projects";

            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load recent directories
            RecentDirectories.Clear();
            foreach (var directory in _settingsService.GetRecentDirectories())
            {
                RecentDirectories.Add(directory);
            }

            // Load clean patterns
            CleanPatterns.Clear();
            foreach (var pattern in _settingsService.GetCleanPatterns())
            {
                CleanPatterns.Add(pattern);
            }
        }

        [RelayCommand]
        private async Task BrowseDirectory()
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder to Scan for Unity Projects",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                var folder = folders[0];
                CurrentDirectory = folder.Path.LocalPath;
                await ScanDirectoryAsync(CurrentDirectory);
            }
        }

        [RelayCommand]
        private async Task ScanDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                StatusMessage = "Invalid directory path";
                return;
            }

            CurrentDirectory = directory;
            await ScanDirectoryAsync(directory);
        }

        private async Task ScanDirectoryAsync(string directory)
        {
            try
            {
                IsScanning = true;
                StatusMessage = "Scanning for Unity projects...";
                Projects.Clear();

                var projects = await _scannerService.ScanDirectoryAsync(directory);

                foreach (var project in projects)
                {
                    Projects.Add(project);
                }

                _settingsService.AddRecentDirectory(directory);
                RecentDirectories.Clear();
                foreach (var recentDir in _settingsService.GetRecentDirectories())
                {
                    RecentDirectories.Add(recentDir);
                }

                StatusMessage = $"Found {Projects.Count} Unity project(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error scanning directory: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }

        [RelayCommand]
        private async Task CleanProjects()
        {
            try
            {
                IsCleaning = true;
                StatusMessage = "Cleaning Unity projects...";

                bool useRecycleBin = _settingsService.GetUseRecycleBin();
                await _cleanerService.CleanProjectsAsync(Projects, CleanPatterns, useRecycleBin);

                StatusMessage = "Cleaning completed successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error cleaning projects: {ex.Message}";
            }
            finally
            {
                IsCleaning = false;
            }
        }

        [RelayCommand]
        private void ToggleProjectExpansion(UnityProject project)
        {
            project.IsExpanded = !project.IsExpanded;

            if (project.IsExpanded && project.FileStructure.Count == 0)
            {
                project.LoadFileStructure();
            }
        }

        [RelayCommand]
        private void ToggleProjectSelection(UnityProject project)
        {
            project.IsSelectedForCleaning = !project.IsSelectedForCleaning;
        }

        [RelayCommand]
        private void OpenInExplorer(string path)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start("explorer.exe", path);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    System.Diagnostics.Process.Start("open", path);
                }
                else if (OperatingSystem.IsLinux())
                {
                    System.Diagnostics.Process.Start("xdg-open", path);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening directory: {ex.Message}";
            }
        }

        [RelayCommand]
        private void AddCleanPattern()
        {
            var pattern = "NewPattern";
            _settingsService.AddCleanPattern(pattern);

            CleanPatterns.Clear();
            foreach (var p in _settingsService.GetCleanPatterns())
            {
                CleanPatterns.Add(p);
            }
        }

        [RelayCommand]
        private void RemoveCleanPattern(CleanPattern pattern)
        {
            if (!pattern.IsDefault)
            {
                _settingsService.RemoveCleanPattern(pattern.Pattern);

                CleanPatterns.Clear();
                foreach (var p in _settingsService.GetCleanPatterns())
                {
                    CleanPatterns.Add(p);
                }
            }
        }

        [RelayCommand]
        private void UpdateCleanPattern(CleanPattern pattern)
        {
            _settingsService.UpdateCleanPattern(pattern);
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            var useDarkTheme = _settingsService.GetUseDarkTheme();
            _settingsService.SetUseDarkTheme(!useDarkTheme);

            // Theme change will be handled by App.axaml.cs
        }

        private Window? GetTopLevel()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }
}