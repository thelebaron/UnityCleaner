using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UnityCleaner.Models;
using static UnityCleaner.Models.UnityProject;
using UnityCleaner.Services;
using UnityCleaner.ViewModels;

namespace UnityCleaner.Views;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsView(_settingsService);
        settingsWindow.ShowDialog(this);
    }

    private void ToggleProjectExpansion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is UnityProject project && DataContext is MainWindowViewModel viewModel)
        {
            // Debug logging
            Console.WriteLine($"Toggling expansion for project: {project.ProjectName}, current state: {project.IsExpanded}");

            viewModel.ToggleProjectExpansionCommand.Execute(project);

            // Debug logging after toggle
            Console.WriteLine($"New expansion state: {project.IsExpanded}");
        }
    }

    private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && DataContext is MainWindowViewModel viewModel)
        {
            var path = menuItem.CommandParameter as string;
            if (!string.IsNullOrEmpty(path))
            {
                viewModel.OpenInExplorerCommand.Execute(path);
            }
        }
    }

    private void ToggleProjectSelection_Click(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is UnityProject project && DataContext is MainWindowViewModel viewModel)
        {
            // Debug logging
            Console.WriteLine($"Toggling selection for project: {project.ProjectName}, current state: {project.IsSelectedForCleaning}");

            viewModel.ToggleProjectSelectionCommand.Execute(project);

            // Debug logging after toggle
            Console.WriteLine($"New selection state: {project.IsSelectedForCleaning}");
        }
    }

    private void ToggleFileItemExpansion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FileSystemItem fileItem)
        {
            // Debug logging
            Console.WriteLine($"Toggling file item expansion: {fileItem.Name}, current state: {fileItem.IsExpanded}");

            fileItem.IsExpanded = !fileItem.IsExpanded;

            // Debug logging after toggle
            Console.WriteLine($"New file item expansion state: {fileItem.IsExpanded}");
        }
    }
}