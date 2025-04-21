using Avalonia.Controls;
using Avalonia.Interactivity;
using UnityCleaner.Models;
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
            viewModel.ToggleProjectExpansionCommand.Execute(project);
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
}