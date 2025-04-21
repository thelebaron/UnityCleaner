using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UnityCleaner.Models;

namespace UnityCleaner.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private Settings _settings;

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "UnityCleaner");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            _settings = LoadSettings();
        }

        private Settings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return CreateDefaultSettings();
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json);

                if (settings == null)
                {
                    return CreateDefaultSettings();
                }

                // Ensure default patterns exist
                EnsureDefaultPatterns(settings);

                return settings;
            }
            catch (Exception)
            {
                return CreateDefaultSettings();
            }
        }

        private void EnsureDefaultPatterns(Settings settings)
        {
            var defaultPatterns = new[] { "Library", "Logs" };

            foreach (var pattern in defaultPatterns)
            {
                if (!settings.CleanPatterns.Any(p => p.Pattern == pattern))
                {
                    settings.CleanPatterns.Add(new CleanPattern(pattern, true, true));
                }
            }
        }

        private Settings CreateDefaultSettings()
        {
            return new Settings
            {
                RecentDirectories = new List<string>(),
                CleanPatterns = new List<CleanPattern>
                {
                    new CleanPattern("Library", true, true),
                    new CleanPattern("Logs", true, true)
                },
                UseDarkTheme = true
            };
        }

        public async Task SaveSettingsAsync()
        {
            await Task.Run(() =>
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_settingsFilePath, json);
            });
        }

        public List<string> GetRecentDirectories()
        {
            return _settings.RecentDirectories;
        }

        public void AddRecentDirectory(string directory)
        {
            if (!_settings.RecentDirectories.Contains(directory))
            {
                _settings.RecentDirectories.Insert(0, directory);

                // Keep only the last 10 directories
                if (_settings.RecentDirectories.Count > 10)
                {
                    _settings.RecentDirectories = _settings.RecentDirectories.Take(10).ToList();
                }

                SaveSettingsAsync().ConfigureAwait(false);
            }
        }

        public List<CleanPattern> GetCleanPatterns()
        {
            return _settings.CleanPatterns;
        }

        public void AddCleanPattern(string pattern)
        {
            if (!_settings.CleanPatterns.Any(p => p.Pattern == pattern))
            {
                _settings.CleanPatterns.Add(new CleanPattern(pattern));
                SaveSettingsAsync().ConfigureAwait(false);
            }
        }

        public void RemoveCleanPattern(string pattern)
        {
            var patternToRemove = _settings.CleanPatterns.FirstOrDefault(p => p.Pattern == pattern && !p.IsDefault);

            if (patternToRemove != null)
            {
                _settings.CleanPatterns.Remove(patternToRemove);
                SaveSettingsAsync().ConfigureAwait(false);
            }
        }

        public void UpdateCleanPattern(CleanPattern pattern)
        {
            var existingPattern = _settings.CleanPatterns.FirstOrDefault(p => p.Pattern == pattern.Pattern);

            if (existingPattern != null)
            {
                existingPattern.IsEnabled = pattern.IsEnabled;
                SaveSettingsAsync().ConfigureAwait(false);
            }
        }

        public bool GetUseDarkTheme()
        {
            return _settings.UseDarkTheme;
        }

        public void SetUseDarkTheme(bool useDarkTheme)
        {
            _settings.UseDarkTheme = useDarkTheme;
            SaveSettingsAsync().ConfigureAwait(false);
        }

        public bool GetUseRecycleBin()
        {
            return _settings.UseRecycleBin;
        }

        public void SetUseRecycleBin(bool useRecycleBin)
        {
            _settings.UseRecycleBin = useRecycleBin;
            SaveSettingsAsync().ConfigureAwait(false);
        }
    }

    public class Settings
    {
        public List<string> RecentDirectories { get; set; } = new List<string>();
        public List<CleanPattern> CleanPatterns { get; set; } = new List<CleanPattern>();
        public bool UseDarkTheme { get; set; } = true;
        public bool UseRecycleBin { get; set; } = true;
    }
}
