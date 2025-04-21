using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityCleaner.Models;
using UnityCleaner.Services;

namespace UnityCleaner.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private bool _useDarkTheme;

        [ObservableProperty]
        private bool _useRecycleBin;

        [ObservableProperty]
        private string _newPatternText;

        public ObservableCollection<CleanPattern> CleanPatterns { get; } = new();

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            UseDarkTheme = _settingsService.GetUseDarkTheme();
            UseRecycleBin = _settingsService.GetUseRecycleBin();
            NewPatternText = string.Empty;

            LoadCleanPatterns();
        }

        private void LoadCleanPatterns()
        {
            CleanPatterns.Clear();
            foreach (var pattern in _settingsService.GetCleanPatterns())
            {
                CleanPatterns.Add(pattern);
            }
        }

        [RelayCommand]
        private void AddCleanPattern()
        {
            if (string.IsNullOrWhiteSpace(NewPatternText))
                return;

            if (!CleanPatterns.Any(p => p.Pattern == NewPatternText))
            {
                _settingsService.AddCleanPattern(NewPatternText);
                LoadCleanPatterns();
                NewPatternText = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveCleanPattern(CleanPattern pattern)
        {
            if (!pattern.IsDefault)
            {
                _settingsService.RemoveCleanPattern(pattern.Pattern);
                LoadCleanPatterns();
            }
        }

        [RelayCommand]
        private void TogglePatternEnabled(CleanPattern pattern)
        {
            pattern.IsEnabled = !pattern.IsEnabled;
            _settingsService.UpdateCleanPattern(pattern);
        }

        partial void OnUseDarkThemeChanged(bool value)
        {
            _settingsService.SetUseDarkTheme(value);
        }

        partial void OnUseRecycleBinChanged(bool value)
        {
            _settingsService.SetUseRecycleBin(value);
        }
    }
}
