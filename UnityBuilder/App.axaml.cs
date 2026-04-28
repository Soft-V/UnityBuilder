using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Hypocrite.Container;
using Hypocrite.Container.Interfaces;
using System.Linq;
using UnityBuilder.Services.ServiceCollectionExtensions;
using UnityBuilder.ViewModels;
using UnityBuilder.Views;

namespace UnityBuilder
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public ILightContainer Container { get; } = new LightContainer();
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //UnityBuilder.Properties.Settings.Default.ParametersJson = string.Empty;
            //UnityBuilder.Properties.Settings.Default.Save();
            //return;
            Container.RegisterInstance<ILightContainer>(Container);
            Container.AddCommonServices();
            Container.AddViewModels();
            Container.AddViews();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Container.Resolve<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}