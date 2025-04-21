using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UnityCleaner.Models
{
    public class UnityProject : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelectedForCleaning;
        private List<FileSystemItem> _fileStructure;

        public string ProjectPath { get; }
        public string ProjectName { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelectedForCleaning
        {
            get => _isSelectedForCleaning;
            set
            {
                if (_isSelectedForCleaning != value)
                {
                    _isSelectedForCleaning = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<FileSystemItem> FileStructure
        {
            get => _fileStructure;
            private set
            {
                if (_fileStructure != value)
                {
                    _fileStructure = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UnityProject(string projectPath)
        {
            ProjectPath = projectPath;
            ProjectName = Path.GetFileName(projectPath);
            _isExpanded = false;
            _isSelectedForCleaning = true; // Default to selected for cleaning
            _fileStructure = new List<FileSystemItem>();
        }

        public void LoadFileStructure()
        {
            FileStructure.Clear();
            var rootDir = new DirectoryInfo(ProjectPath);
            FileStructure = GetFileSystemItems(rootDir, 0);
        }

        private List<FileSystemItem> GetFileSystemItems(DirectoryInfo directory, int level, int maxDepth = 2)
        {
            if (level > maxDepth)
                return new List<FileSystemItem>();

            var items = new List<FileSystemItem>();

            try
            {
                foreach (var dir in directory.GetDirectories().OrderBy(d => d.Name))
                {
                    var item = new FileSystemItem
                    {
                        Name = dir.Name,
                        Path = dir.FullName,
                        IsDirectory = true,
                        Level = level
                    };

                    if (level < maxDepth)
                    {
                        item.Children = GetFileSystemItems(dir, level + 1, maxDepth);
                    }

                    items.Add(item);
                }

                if (level == maxDepth)
                    return items;

                foreach (var file in directory.GetFiles().OrderBy(f => f.Name))
                {
                    items.Add(new FileSystemItem
                    {
                        Name = file.Name,
                        Path = file.FullName,
                        IsDirectory = false,
                        Level = level
                    });
                }
            }
            catch (Exception)
            {
                // Handle access denied or other exceptions
            }

            return items;
        }

        public bool IsValidUnityProject()
        {
            // Check if the directory contains the required Unity project folders
            string[] requiredFolders = { "Assets", "Library", "Packages", "ProjectSettings" };

            foreach (var folder in requiredFolders)
            {
                if (!Directory.Exists(Path.Combine(ProjectPath, folder)))
                {
                    return false;
                }
            }

            return true;
        }

        public void Clean(IEnumerable<string> patterns, bool useRecycleBin = false)
        {
            // Default folders to clean
            var foldersToClean = new List<string> { "Library", "Logs" };

            // Add custom patterns
            foldersToClean.AddRange(patterns);

            foreach (var pattern in foldersToClean)
            {
                var path = Path.Combine(ProjectPath, pattern);
                if (Directory.Exists(path))
                {
                    try
                    {
                        if (useRecycleBin)
                        {
                            // Use recycle bin (or equivalent) if available
                            if (OperatingSystem.IsWindows())
                            {
                                // Windows: Use Shell32 to delete to recycle bin
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                                    path,
                                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                            }
                            else if (OperatingSystem.IsMacOS())
                            {
                                // macOS: Use 'trash' command if available, otherwise fallback to regular delete
                                try
                                {
                                    var process = new System.Diagnostics.Process
                                    {
                                        StartInfo = new System.Diagnostics.ProcessStartInfo
                                        {
                                            FileName = "trash",
                                            Arguments = $"\"{path}\"",
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        }
                                    };
                                    process.Start();
                                    process.WaitForExit();

                                    if (process.ExitCode != 0)
                                    {
                                        // Fallback to regular delete if trash command fails
                                        Directory.Delete(path, true);
                                    }
                                }
                                catch
                                {
                                    // Fallback to regular delete if trash command is not available
                                    Directory.Delete(path, true);
                                }
                            }
                            else if (OperatingSystem.IsLinux())
                            {
                                // Linux: Try to use 'gio trash' command, otherwise fallback to regular delete
                                try
                                {
                                    var process = new System.Diagnostics.Process
                                    {
                                        StartInfo = new System.Diagnostics.ProcessStartInfo
                                        {
                                            FileName = "gio",
                                            Arguments = $"trash \"{path}\"",
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        }
                                    };
                                    process.Start();
                                    process.WaitForExit();

                                    if (process.ExitCode != 0)
                                    {
                                        // Fallback to regular delete if gio trash command fails
                                        Directory.Delete(path, true);
                                    }
                                }
                                catch
                                {
                                    // Fallback to regular delete if gio command is not available
                                    Directory.Delete(path, true);
                                }
                            }
                            else
                            {
                                // Fallback for other platforms
                                Directory.Delete(path, true);
                            }
                        }
                        else
                        {
                            // Regular delete
                            Directory.Delete(path, true);
                        }
                    }
                    catch (Exception)
                    {
                        // Handle exceptions (access denied, etc.)
                    }
                }
            }
        }
    }



    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private List<FileSystemItem> _children = [];

        public required string Name { get; set; }
        public required string Path { get; set; }
        public bool IsDirectory { get; set; }
        public int Level { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<FileSystemItem> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
