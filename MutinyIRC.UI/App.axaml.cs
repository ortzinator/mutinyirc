namespace MutinyIRC.UI;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using Common;
using ViewModels;
using Views;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            CompositionRoot.Wire(new Bindings());
            var viewModel = CompositionRoot.Resolve<MainViewModel>();
            viewModel.Start();
            var window = new MainWindow { DataContext = viewModel };
            bool closing = false;
            viewModel.RequestClose += (_, _) => window.Close();
            window.Closing += (_, _) =>
            {
                if (closing) return;
                closing = true;
                viewModel.Close();
                IrcSettingsManager.Instance.Save();
                RandomMessages.Instance.Save();
            };
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
