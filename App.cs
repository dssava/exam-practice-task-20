using Avalonia;
using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace BakeryTracker
{
    public sealed class App : Application
    {
        public override void Initialize()
        {
            // Задаємо світлу тему та підключаємо стилі Fluent.
            RequestedThemeVariant = ThemeVariant.Light;
            Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Themes.Fluent/"))
            {
                Source = new Uri("avares://Avalonia.Themes.Fluent/FluentTheme.xaml")
            });
            Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/"))
            {
                Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
            });
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
