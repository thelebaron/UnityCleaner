using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using UnityCleaner.Models;

namespace UnityCleaner.ViewModels
{
    public partial class ProjectItemViewModel : ViewModelBase
    {
        private readonly UnityProject _project;

        [ObservableProperty]
        private bool _isExpanded;

        public string ProjectName => _project.ProjectName;
        public string ProjectPath => _project.ProjectPath;
        public ObservableCollection<FileSystemItemViewModel> FileStructure { get; } = new();

        public ProjectItemViewModel(UnityProject project)
        {
            _project = project;
            IsExpanded = false;
        }

        partial void OnIsExpandedChanged(bool value)
        {
            _project.IsExpanded = value;
            
            if (value && FileStructure.Count == 0)
            {
                LoadFileStructure();
            }
        }

        private void LoadFileStructure()
        {
            FileStructure.Clear();
            _project.LoadFileStructure();
            
            foreach (var item in _project.FileStructure)
            {
                FileStructure.Add(new FileSystemItemViewModel(item));
            }
        }
    }

    public partial class FileSystemItemViewModel : ViewModelBase
    {
        private readonly FileSystemItem _item;

        [ObservableProperty]
        private bool _isExpanded;

        public string Name => _item.Name;
        public string Path => _item.Path;
        public bool IsDirectory => _item.IsDirectory;
        public int Level => _item.Level;
        public ObservableCollection<FileSystemItemViewModel> Children { get; } = new();

        public FileSystemItemViewModel(FileSystemItem item)
        {
            _item = item;
            IsExpanded = false;
            
            foreach (var child in item.Children)
            {
                Children.Add(new FileSystemItemViewModel(child));
            }
        }
    }
}
