using System;
using System.Windows;
using NLog;

namespace MSsqlTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionOccured; ;
        }

        private void OnUnhandledExceptionOccured(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e);
        }
    }
}
