using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore
    {
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public ReactiveList<ViewModelTaskCompact> TaskCompacts { get; set; }
        public ReactiveList<ApiInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        private readonly List<ApiSchedule> _schedules;
        private readonly List<ApiRecepientGroup> _recepientGroups;

        [Reactive] public ViewModelTaskCompact SelectedTaskCompact { get; set; } 
        [Reactive] public ApiInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public ViewModelTask SelectedTask { get; set; }
        [Reactive] public ApiInstance SelectedInstance { get; set; }


        public ReactiveCommand RefreshTasksCommand { get; }

        public Core(IReportService reportService,IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;

            TaskCompacts = new ReactiveList<ViewModelTaskCompact>();
            SelectedTaskInstanceCompacts = new ReactiveList<ApiInstanceCompact>();
            _schedules=new List<ApiSchedule>();
            _recepientGroups=new List<ApiRecepientGroup>();

            RefreshTasksCommand = ReactiveCommand.Create(LoadTaskCompacts);
            
            this.ObservableForProperty(s => s.SelectedTaskCompact) //ObservableForProperty ignores initial nulls,whenanyvalue not?
                .Where(x => x.Value != null)
                .Subscribe(_ =>
                {
                    LoadInstanceCompactsByTaskId(_.Value.Id);
                    LoadSelectedTaskById(_.Value.Id);
                });
            this.ObservableForProperty(s => s.SelectedTaskCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ => SelectedTask = null);

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value != null)
                .Subscribe(_ =>
                {
                    LoadSelectedInstanceById(_.Value.Id);
                });
            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ =>  SelectedInstance = null);
            OnStart();
        }

        public void LoadTaskCompacts()
        {
            var taskList = _reportService.GetAllTaskCompacts();
            TaskCompacts.Clear();

            foreach (var task in taskList)
            {
                var vtask = _mapper.Map<ViewModelTaskCompact>(task);

                vtask.Schedule = _schedules
                    .FirstOrDefault(s => s.Id == task.ScheduleId)?.Name;

                vtask.RecepientGroup = _recepientGroups
                    .FirstOrDefault(r=>r.Id==task.RecepientGroupId)?.Name;

                TaskCompacts.Add(vtask);
            }
        }
        
        public void LoadSchedules()
        {
            var scheduleList = _reportService.GetSchedules();
            _schedules.Clear();

            foreach (var schedule in scheduleList)
                _schedules.Add(schedule);
        }

        public void LoadRecepientGroups()
        {
            var recepientGroupList = _reportService.GetRecepientGroups();
            _recepientGroups.Clear();

            foreach (var group in recepientGroupList)
            _recepientGroups.Add(group);
        }

        public void LoadSelectedTaskById(int id)
        {
            var apitask = _reportService.GetTaskById(id);
            var selTask = _mapper.Map<ViewModelTask>(apitask);

            selTask.Schedule = _schedules
                .FirstOrDefault(s => s.Id == apitask.ScheduleId)?.Schedule;
            selTask.RecepientAddresses= _recepientGroups
                .FirstOrDefault(r => r.Id == apitask.RecepientGroupId)?.Addresses;
            SelectedTask = selTask;
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = _reportService.GetInstanceById(id);
        }

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = _reportService.GetInstanceCompactsByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(instance);
        }

        public void OnStart()
        {
            LoadSchedules();
            LoadRecepientGroups();
            LoadTaskCompacts();
        }
    }
}
