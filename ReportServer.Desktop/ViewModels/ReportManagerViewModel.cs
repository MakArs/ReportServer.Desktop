using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class ReportManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public DistinctShell Shell { get; }

        public ReactiveList<DesktopReport> Reports { get; set; }
        [Reactive] public DesktopReport SelectedReport { get; set; }

        public ReactiveCommand<DesktopReport, Unit> EditReportCommand { get; set; }

        public ReportManagerViewModel(ICachedService cachedService, IShell shell)
        {
            this.cachedService = cachedService;
            this.Shell = shell as DistinctShell;

            EditReportCommand = ReactiveCommand.Create<DesktopReport>(report =>
            {
                if (report == null) return;

                var fullName = $"Report {report.Id} editor";

                this.Shell.ShowDistinctView<ReportEditorView>(fullName,
                    new ReportEditorRequest {Report = report, FullId = fullName},
                    new UiShowOptions {Title = fullName});
            });
        }

        public void Initialize(ViewRequest viewRequest)
        {
            //Reports = cachedService.oper;
        }
    }
}