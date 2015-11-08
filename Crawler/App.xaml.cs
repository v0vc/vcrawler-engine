using System.Windows;
using Crawler.Models;
using Crawler.ViewModels;
using Crawler.Views;

namespace Crawler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            #region autofac

            //var builder = new ContainerBuilder();
            //builder.RegisterType<MainWindow>().SingleInstance().PropertiesAutowired();
            //builder.RegisterType<MainWindowModel>().SingleInstance();
            //builder.RegisterType<MainWindowViewModel>().SingleInstance();
            //using (var container = builder.Build())
            //using (var scope = container.BeginLifetimeScope())
            //{
            //    var mainWindow = scope.Resolve<MainWindow>();
            //    mainWindow.Show();
            //}

            #endregion

            #region with NInject

            //IKernel kernel = new StandardKernel();
            //kernel.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
            //kernel.Bind<MainWindow>().ToSelf().InSingletonScope();
            //kernel.Bind<MainWindowModel>().ToSelf().InSingletonScope();

            //kernel.Bind<ISubscribeFactory>().To<SubscribeFactory>().InSingletonScope();
            //kernel.Bind<IChannelFactory>().To<ChannelFactory>().InSingletonScope();
            //kernel.Bind<IVideoItemFactory>().To<VideoItemFactory>().InSingletonScope();
            //kernel.Bind<IPlaylistFactory>().To<PlaylistFactory>().InSingletonScope();
            //kernel.Bind<ISqLiteDatabase>().To<SqLiteDatabase>().InSingletonScope();
            //kernel.Bind<IYouTubeSite>().To<YouTubeSiteApiV2>().InSingletonScope();
            //kernel.Bind<ITagFactory>().To<TagFactory>().InSingletonScope();
            //kernel.Bind<ICredFactory>().To<CredFactory>().InSingletonScope();
            //kernel.Bind<ISettingFactory>().To<SettingFactory>().InSingletonScope();

            //kernel.Bind<ICommonFactory>().ToFactory();

            //var mainWindow = kernel.Get<MainWindow>();

            //mainWindow.Show();

            #endregion

            var mainWindow = new MainWindow();
            //mainWindow.DataContext = new MainWindowViewModel(new MainWindowModel());
            mainWindow.Show();
        }
    }
}
