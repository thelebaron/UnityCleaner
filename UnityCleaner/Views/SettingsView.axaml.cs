using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using UnityCleaner.Models;
using UnityCleaner.Services;
using UnityCleaner.ViewModels;

namespace UnityCleaner.Views
{
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public SettingsView(SettingsService settingsService)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(settingsService);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TogglePatternEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.CommandParameter is CleanPattern pattern && DataContext is SettingsViewModel viewModel)
            {
                viewModel.TogglePatternEnabledCommand.Execute(pattern);
            }
        }

        private void RemoveCleanPattern_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CleanPattern pattern && DataContext is SettingsViewModel viewModel)
            {
                viewModel.RemoveCleanPatternCommand.Execute(pattern);
            }
        }
    }
}
