using System.Windows;
using Autofac;
using MahApps.Metro.Controls.Dialogs;
using ReportServer.Desktop.ViewModels.General;
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
                    Title = "ReportServer.Desktop"
                });

            // register IDockWindow in dialog coordinator with shell context
            DialogParticipation.SetRegister(shell.Container.Resolve<IDockWindow>() as Window, shell);

            (shell as CachedServiceShell)?.InitCachedServiceAsync(3);
        }
    }
}
