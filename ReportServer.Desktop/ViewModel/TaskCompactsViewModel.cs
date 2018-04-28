using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskCompactsViewModel : ReactiveObject
    {
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public ReactiveList<ViewModelTask> TaskCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<ApiReport> Reports { get; set; }

        [Reactive] public ViewModelTask SelectedTaskCompact { get; set; }

        public TaskCompactsViewModel(IReportService reportService, IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;


            this.WhenAnyObservable(s =>
                    s.TaskCompacts.Changed) 
                .Subscribe(x =>SelectedTaskCompact = null);
        }

        public void LoadTaskCompacts()
        {
            var taskList = _reportService.GetAllTasks();
            TaskCompacts.Clear();

            foreach (var task in taskList)
            {
                var vtask = _mapper.Map<ViewModelTask>(task);

                vtask.Schedule = Schedules
                    .FirstOrDefault(s => s.Id == task.ScheduleId)?.Name;

                vtask.RecepientGroup = RecepientGroups
                    .FirstOrDefault(r => r.Id == task.RecepientGroupId)?.Name;

                vtask.ReportName = Reports.FirstOrDefault(r => r.Id == task.ReportId)?.Name;

                TaskCompacts.Add(vtask);
            }
        }
    }
}
