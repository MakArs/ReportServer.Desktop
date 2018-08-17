using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IReportService reportService;
        private readonly IMapper mapper;

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopReport> Reports { get; set; }

        [Reactive] public DesktopReport SelectedReport { get; set; }
        public int Id { get; set; }
        public int? TelegramChannelId { get; set; }
        [Reactive] public int ReportId { get; set; }
        [Reactive] public int? ScheduleId { get; set; }
        [Reactive] public int? RecepientGroupId { get; set; }
        [Reactive] public int TryCount { get; set; }
        [Reactive] public bool HasHtmlBody { get; set; }
        [Reactive] public bool HasJsonAttachment { get; set; }
        [Reactive] public bool HasXlsxAttachment { get; set; }
        [Reactive] public bool IsDirty { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }

        public TaskEditorViewModel(IReportService reportService, IMapper mapper)
        {
            this.reportService = reportService;
            this.mapper = mapper;

            var cansave = this.WhenAnyValue(tvm => tvm.IsDirty, isd => isd == true);

            SaveChangesCommand = ReactiveCommand.Create(() =>
            {
                var editedTask = new ApiTask();
                mapper.Map(this, editedTask);
                reportService.UpdateTask(editedTask);
                Close();
            }, cansave);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Schedules = reportService.Schedules;
            RecepientGroups = reportService.RecepientGroups;
            Reports = reportService.Reports;

            if (viewRequest is TaskEditorRequest request)
            {
                mapper.Map(request.Task, this);
            }

            SelectedReport = Reports.First(rep => rep.Id == ReportId);
            IsDirty = false;

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                IsDirty = true;
            }

            PropertyChanged += Changed;
        }
    }
}
