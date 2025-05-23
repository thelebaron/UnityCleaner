﻿﻿﻿﻿﻿using System;
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

        private string _currentDirectory;

        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                if (SetProperty(ref _currentDirectory, value) && !string.IsNullOrEmpty(value) && Directory.Exists(value))
                {
                    // Save the directory whenever it changes to a valid directory
                    _settingsService.AddRecentDirectory(value);
                }
            }
        }

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private bool _isCleaning;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private int _cleaningProgress;

        [ObservableProperty]
        private int _totalProjectsToClean;

        [ObservableProperty]
        private string _currentProjectCleaning;

        public ObservableCollection<UnityProject> Projects { get; } = new();
        public ObservableCollection<string> RecentDirectories { get; } = new();
        public ObservableCollection<CleanPattern> CleanPatterns { get; } = new();

        public MainWindowViewModel()
        {
            _scannerService = new ProjectScannerService();
            _cleanerService = new ProjectCleanerService();
            _settingsService = new SettingsService();

            StatusMessage = "Ready to scan for Unity projects";
            CurrentProjectCleaning = string.Empty;
            CleaningProgress = 0;
            TotalProjectsToClean = 0;

            // Subscribe to progress updates
            _cleanerService.ProgressUpdated += OnCleaningProgressUpdated;

            LoadSettings();
        }

        private void OnCleaningProgressUpdated(int projectsCompleted, int totalProjects, string currentProject)
        {
            CleaningProgress = projectsCompleted;
            TotalProjectsToClean = totalProjects;
            CurrentProjectCleaning = currentProject;

            // Update status message
            if (projectsCompleted < totalProjects)
            {
                StatusMessage = $"Cleaning {projectsCompleted} of {totalProjects} projects: {currentProject}";
            }
            else
            {
                StatusMessage = $"Cleaning completed successfully: {totalProjects} projects processed";
            }
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

            // Load last used directory
            CurrentDirectory = _settingsService.GetLastUsedDirectory();

            // Automatically scan the last used directory if it exists
            if (!string.IsNullOrEmpty(CurrentDirectory) && Directory.Exists(CurrentDirectory))
            {
                // Use Task.Run to avoid blocking the UI thread during initialization
                Task.Run(async () => await ScanDirectoryAsync(CurrentDirectory));
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

                // Refresh the recent directories list
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
                // Reset progress tracking
                CleaningProgress = 0;
                TotalProjectsToClean = Projects.Count(p => p.IsSelectedForCleaning);
                CurrentProjectCleaning = string.Empty;

                // Start cleaning
                IsCleaning = true;
                StatusMessage = "Preparing to clean Unity projects...";

                bool useRecycleBin = _settingsService.GetUseRecycleBin();
                await _cleanerService.CleanProjectsAsync(Projects, CleanPatterns, useRecycleBin);

                // Final status is set by the progress update handler
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

        // Theme toggle removed

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