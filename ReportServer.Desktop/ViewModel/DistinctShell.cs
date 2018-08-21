using System.Linq;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;

namespace ReportServer.Desktop.ViewModel
{

    public class DistinctShell : Shell
    {
        private readonly ICachedService cachedService;

        public ReactiveCommand SaveCommand { get; set; }
        public ReactiveCommand RefreshCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }
        public ReactiveCommand CreateReportCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public DistinctShell(ICachedService cachedService)
        {
            this.cachedService = cachedService;

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