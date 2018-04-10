using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore
    {
        private readonly IReportService _reportService;

        public ObservableCollection<ApiTaskCompact> TaskCompacts { get; set; }
        public ObservableCollection<ApiInstanceCompact> SelectedTaskInstanceCompacts { get; set; }

        [Reactive] public ApiTaskCompact SelectedTaskCompact { get; set; } //todo:check for null
        [Reactive] public ApiInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public ApiTask SelectedTask { get; set; }
        [Reactive] public ApiInstance SelectedInstance { get; set; }

        public ReactiveCommand RefreshTasksCommand { get; }

        public Core(IReportService reportService)
        {
            _reportService = reportService;

            TaskCompacts = new ObservableCollection<ApiTaskCompact>();
            SelectedTaskInstanceCompacts = new ObservableCollection<ApiInstanceCompact>();

            RefreshTasksCommand = ReactiveCommand.Create(LoadTaskCompacts);
            
            this.ObservableForProperty(s => s.SelectedTaskCompact)
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
                TaskCompacts.Add(task);
        }
        
        public void LoadSelectedTaskById(int id)
        {
            SelectedTask = _reportService.GetTaskById(id);
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
            LoadTaskCompacts();
        }
    }
}
