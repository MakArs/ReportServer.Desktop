using System.Collections.ObjectModel;
using PropertyChanged;
using ReportServer.Desktop.Interfaces;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace ReportServer.Desktop.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class Core : ICore
    {
        private readonly IReportService _reportService;

        public ObservableCollection<ApiTaskCompact> TaskCompacts { get; set; }
        public ObservableCollection<ApiInstanceCompact> InstanceCompacts { get; set; }

        public readonly ObservableCollection<ApiInstanceCompact> _selectedTaskInstanceCompacts;

        private ApiInstance SelectedInstance { get; set; }
        private ApiTask SelectedTask { get; set; }

        public ReactiveCommand StartCommand { get; }
        public ReactiveCommand LoadTaskCompactsCommand { get; }
        public ReactiveCommand LoadInstanceCompactsCommand { get; }
        public ReactiveCommand LoadSelectedTaskByIdCommand { get; }
        public ReactiveCommand LoadSelectedInstanceByIdCommand { get; }
        public ReactiveCommand LoadInstanceCompactsByTaskIdCommand { get; }

        public Core(IReportService reportService)
        {
            _reportService = reportService;

            TaskCompacts = new ObservableCollection<ApiTaskCompact>();
            InstanceCompacts = new ObservableCollection<ApiInstanceCompact>();

            StartCommand = ReactiveCommand.Create(OnStart);
            LoadTaskCompactsCommand = ReactiveCommand.Create(LoadTaskCompacts);
            LoadInstanceCompactsCommand = ReactiveCommand.Create(LoadInstanceCompacts);
            LoadSelectedTaskByIdCommand = ReactiveCommand.Create((int id)
                => LoadSelectedTaskById(id));
            LoadSelectedInstanceByIdCommand = ReactiveCommand.Create((int id)
                => LoadSelectedInstanceById(id));
            LoadInstanceCompactsByTaskIdCommand = ReactiveCommand.Create((int taskId)
                => LoadInstanceCompactsByTaskId(taskId));

            OnStart();
        }

        public void LoadTaskCompacts()
        {
            var taskList = _reportService.GetAllTaskCompacts();
            TaskCompacts.Clear();
            foreach (var task in taskList)
                TaskCompacts.Add(task);
        }

        public void LoadInstanceCompacts()
        {
            var instanceList = _reportService.GetInstanceCompacts();
            InstanceCompacts.Clear();
            foreach (var instance in instanceList)
                InstanceCompacts.Add(instance);
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
            _selectedTaskInstanceCompacts.Clear();
            foreach (var instance in instanceList)
                _selectedTaskInstanceCompacts.Add(instance);
        }

        public void OnStart()
        {
            LoadTaskCompacts();
            LoadInstanceCompacts();
        }
    }
}
