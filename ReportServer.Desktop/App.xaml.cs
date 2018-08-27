using System.Windows;
using ReportServer.Desktop.ViewModels;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            var shell = UiStarter.Start<IDockWindow>(
                new BootsTrap(),
                new UiShowStartWindowOptions
                {
                    Title = "ReportServer.Desktop",
                    ToolPaneWidth = 100
                });

            (shell as DistinctShell)?.InitCachedService(2);

        }
    }
}
