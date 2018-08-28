using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop.ViewModels
{

    public class DistinctShell : Shell
    {
        private readonly ICachedService cachedService;
        private readonly IDialogCoordinator dialogCoordinator;

        public ReactiveCommand SaveCommand { get; set; }
        public ReactiveCommand RefreshCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }
        public ReactiveCommand CreateReportCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public DistinctShell(ICachedService cachedService, IDialogCoordinator dialogCoordinator)
        {
            this.cachedService = cachedService;
            this.dialogCoordinator = dialogCoordinator;

            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (DocumentPane.Children.First(child => child.IsActive).Content is
                        IView v && v.ViewModel is ISaveableViewModel vm)
                    await vm.Save();
            });

            RefreshCommand = ReactiveCommand.Create(this.cachedService.RefreshData);

            CreateTaskCommand = ReactiveCommand.Create(() =>
                ShowDistinctView<TaskEditorView>("Creating new Task",
                    new TaskEditorRequest {Task = new DesktopFullTask {Id = 0}},
                    new UiShowOptions {Title = "Creating new Task"}));

            CreateReportCommand = ReactiveCommand.Create(() =>
                ShowDistinctView<ReportEditorView>("Creating new report",
                    new ReportEditorRequest
                    {
                        Report = new DesktopReport {Id = 0},
                        FullId = "Creating new report"
                    },
                    new UiShowOptions {Title = "Creating new report"}));
           
            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (DocumentPane.Children.First(child => child.IsActive).Content is
                        IView v && v.ViewModel is IDeleteableViewModel vm)
                    await vm.Delete();
            });
        }

        public async Task InitCachedService(int tries)
        {
            while (tries-- > 0)
            {
                var serviceUri = await dialogCoordinator.ShowInputAsync(this, "Login",
                    "Enter working Report service instance url",
                    new MetroDialogSettings
                    {
                        DefaultText = "http://localhost:12345/",
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });

                if (!cachedService.Init(serviceUri))
                    continue;

                ShowView<TaskManagerView>(
                    options: new UiShowOptions {Title = "Task Manager", CanClose = false});

                ShowView<ReportManagerView>(
                    options: new UiShowOptions {Title = "Report Manager", CanClose = false});

                ShowView<RecepientManagerView>(
                    options: new UiShowOptions { Title = "Recepient Manager", CanClose = false });

                ShowView<CronEditorView>(
                    options: new UiShowOptions { Title = "CronEditor", CanClose = false });
                return;
            }

            Application.Current.MainWindow.Close();
        }

        public void ShowDistinctView<TView>(string value,
                                            ViewRequest viewRequest = null,
                                            UiShowOptions options = null) where TView : class, IView
        {
            var child = DocumentPane.Children
                .FirstOrDefault(ch =>
                    ch.Content is TView view && view.ViewModel.FullTitle == value);

            if (child != null)
                child.IsActive = true;

            else
                ShowView<TView>(viewRequest, options);
        }
    }
}