using System.Collections.ObjectModel;
using Newtonsoft.Json;
using PropertyChanged;
using ReactiveUI;
using ReactiveUI.Legacy;
using ReportServer.Desktop.Interfaces;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace ReportServer.Desktop.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class Core : ICore
    {
        private ObservableCollection<ApiTaskCompact> _taskCompacts;
        private ObservableCollection<ApiInstanceCompact> _instanceCompacts;
        private readonly IReportService _reportService;
        private readonly ObservableCollection<ApiInstanceCompact> _selectedTaskInstanceCompacts;

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
            StartCommand = ReactiveCommand.Create(OnStart);
            LoadTaskCompactsCommand = ReactiveCommand.Create(LoadTaskCompacts);
            LoadInstanceCompactsCommand = ReactiveCommand.Create(LoadInstanceCompacts);
            LoadSelectedTaskByIdCommand = ReactiveCommand.Create((int id)
                => LoadSelectedTaskById(id));
            LoadSelectedInstanceByIdCommand = ReactiveCommand.Create((int id)
                => LoadSelectedInstanceById(id));
            LoadInstanceCompactsByTaskIdCommand = ReactiveCommand.Create((int taskId)
                => LoadInstanceCompactsByTaskId(taskId));
        }

        public async void LoadTaskCompacts()
        {
            var taskList =  _reportService.LoadAllTaskCompacts();
            lock (this)
            {
                _taskCompacts.Clear();

                //foreach (var task in taskList)
                //    _taskCompacts.Add(json task);
            }
        }

        public void LoadInstanceCompacts()
        {
            var instanceList = _reportService.LoadInstanceCompacts();
            lock (this)
            {
                _instanceCompacts.Clear();
                foreach (var instance in instanceList)
                    _instanceCompacts.Add(instance);
            }
        }

        public void LoadSelectedTaskById(int id)
        {
            SelectedTask = _reportService.LoadTaskById(id);
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = _reportService.LoadInstanceById(id);
        }

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = _reportService.LoadInstanceCompactsByTaskId(taskId);
            lock (this)
            {
                _selectedTaskInstanceCompacts.Clear();
                foreach (var instance in instanceList)
                    _selectedTaskInstanceCompacts.Add(instance);
            }
        }

        public void OnStart()
        {
            _taskCompacts = new ObservableCollection<ApiTaskCompact>();
            _instanceCompacts = new ObservableCollection<ApiInstanceCompact>();
            LoadTaskCompacts();
            LoadInstanceCompacts();
        }
    }
}
