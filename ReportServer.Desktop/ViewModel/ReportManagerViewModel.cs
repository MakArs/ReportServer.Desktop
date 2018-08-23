using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class ReportManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly DistinctShell shell;

        public ReactiveList<DesktopReport> Reports { get; set; }
        [Reactive] public DesktopReport SelectedReport { get; set; }

        public ReactiveCommand<DesktopReport, Unit> EditReportCommand { get; set; }

        public ReportManagerViewModel(ICachedService cachedService, IShell shell)
        {
            this.cachedService = cachedService;
            this.shell = shell as DistinctShell;

            EditReportCommand = ReactiveCommand.Create<DesktopReport>(report =>
            {
                if (report == null) return;
                var name = $"Report \"{report.Name}\" editor";

                var fullName = $"Report {report.Id} editor";

                this.shell.ShowDistinctView<ReportEditorView>(fullName,
                    new ReportEditorRequest {Report = report, FullId = fullName},
                    new UiShowOptions {Title = name});
            });
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Reports = cachedService.Reports;
        }
    }
}