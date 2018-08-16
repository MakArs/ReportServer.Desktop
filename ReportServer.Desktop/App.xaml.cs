using System.Windows;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
                //BootsTrap.Init();
            var shell = UiStarter.Start<IDockWindow>(
                new BootsTrap(),
                new UiShowStartWindowOptions
                {
                    Title = "ReportServer.Desktop",
                    ToolPaneWidth = 100
                });

            shell.ShowView<TaskManagerView>(options: new UiShowOptions { Title = "Task Manager", CanClose = false });
            shell.ShowView<ReportManagerView>(options: new UiShowOptions { Title = "Report Manager", CanClose = false });
            shell.ShowView<TaskEditorView>(options: new UiShowOptions { Title = "Task Editor"});
            shell.ShowView<ReportEditorView>(options: new UiShowOptions { Title = "Report Editor" });
        }
    }
}
