using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class ReportManagerViewModel : ReactiveObject, IViewModel, IInitializableViewModel
    {
        private readonly IReportService reportService;
        private readonly IDistinctShell shell;

        public string Title { get; set; }
        public string FullTitle { get; set; }

        public ReactiveList<DesktopReport> Reports { get; set; }
        [Reactive] public DesktopReport SelectedReport { get; set; }

        public ReportManagerViewModel(IReportService reportService, IShell shell)
        {
            this.reportService = reportService;
            this.shell = shell as IDistinctShell;

            this.WhenAnyValue(t => t.SelectedReport)
                .Where(v => v != null)
                .Subscribe(_=>reportService.RefreshReports());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Reports = reportService.Reports;
        }

    }
}
