using System;
using System.Linq;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskCompactsViewModel : ReactiveObject
    {
        private readonly IReportService reportService;
        private readonly IMapper mapper;

        public ReactiveList<DesktopTask> TaskCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get;  }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get;  }
        public ReactiveList<ApiReport> Reports { get;  }

        [Reactive] public DesktopTask SelectedTaskCompact { get; set; }

        public TaskCompactsViewModel(IReportService reportService, IMapper mapper, ReactiveList<ApiSchedule> schedules, ReactiveList<ApiRecepientGroup> recepientGroups, ReactiveList<ApiReport> reports)
        {
            this.reportService = reportService;
            this.mapper = mapper;
            Schedules = schedules;
            RecepientGroups = recepientGroups;
            Reports = reports;

            this.WhenAnyObservable(s =>
                    s.TaskCompacts.Changed) 
                .Subscribe(x =>SelectedTaskCompact = null);
        }

        public void LoadTaskCompacts()
        {
            var taskList = reportService.GetAllTasks();
            TaskCompacts.Clear();

            foreach (var task in taskList)
            {
                var vtask = mapper.Map<DesktopTask>(task);

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
