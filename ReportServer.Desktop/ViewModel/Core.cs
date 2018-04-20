using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore,IDataErrorInfo
    {
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public ReactiveList<ViewModelTaskCompact> TaskCompacts { get; set; }
        public ReactiveList<ViewModelInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<string> ViewTemplates { get; set; }
        public ReactiveList<string> QueryTemplates { get; set; }

        [Reactive] public ViewModelTaskCompact SelectedTaskCompact { get; set; }
        [Reactive] public ViewModelInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public ViewModelTask SelectedTask { get; set; }
        [Reactive] public ViewModelInstance SelectedInstance { get; set; }

        public ReactiveCommand RefreshTasksCommand { get; set; }
        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand OpenCurrentTaskView { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }
        public ReactiveCommand SaveTaskCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }

        [Reactive] public string TimeOut { get; set; }

        public Core(IReportService reportService, IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;

            TaskCompacts = new ReactiveList<ViewModelTaskCompact>(); //{ChangeTrackingEnabled = true};
            SelectedTaskInstanceCompacts = new ReactiveList<ViewModelInstanceCompact>();
            Schedules = new ReactiveList<ApiSchedule>();
            RecepientGroups = new ReactiveList<ApiRecepientGroup>();
            RefreshTasksCommand = ReactiveCommand.Create(LoadTaskCompacts);
            ViewTemplates = new ReactiveList<string> { "weeklyreport_ve", "dailyreport_ve" };
            QueryTemplates = new ReactiveList<string> { "weeklyreport_de", "dailyreport_de" };

            IObservable<bool> canOpenInstancePage = this
                .WhenAnyValue(t => t.SelectedInstance,
                    si => !string.IsNullOrEmpty(si?.ViewData));
            OpenPage = ReactiveCommand.CreateFromObservable<string, Unit>(OpenPageInBrowser, canOpenInstancePage);

            IObservable<bool> canOpenCurrentTaskView = this
                .WhenAnyValue(t => t.SelectedTask,
                    st => !string.IsNullOrEmpty(st?.ViewTemplate));
            OpenCurrentTaskView =
                ReactiveCommand.CreateFromObservable<int, Unit>(GetHtmlPageByTaskId, canOpenCurrentTaskView);

            IObservable<bool> canDelete = this
                .WhenAnyValue(t => t.SelectedTask, t => t.SelectedInstance, (st, si) =>
                    (st != null && st.Id > 0) || si != null);
            DeleteCommand = ReactiveCommand.Create(DeleteEntity, canDelete);

            IObservable<bool> canSaveTask = this
                .WhenAnyValue(t => t.SelectedTask.ViewTemplate, st =>
                    !string.IsNullOrWhiteSpace(st));
            SaveTaskCommand = ReactiveCommand.Create(SaveTask, canSaveTask);

            CreateTaskCommand = ReactiveCommand.Create(CreateTask);

            this.WhenAnyObservable(s =>
                    s.TaskCompacts.Changed) //ObservableForProperty ignores initial nulls,whenanyvalue not?
                .Subscribe(x =>
                {
                    SelectedTaskInstanceCompacts.Clear();
                    SelectedTaskCompact = null;
                });

            this.ObservableForProperty(s =>
                    s.SelectedTaskCompact)
                .Where(x => x.Value != null)
                .Subscribe(x =>
                {
                    LoadInstanceCompactsByTaskId(x.Value.Id);
                    LoadSelectedTaskById(x.Value.Id);
                });

            this.ObservableForProperty(s => s.SelectedTaskCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ => { SelectedTask = null; });

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value != null)
                .Subscribe(x => { LoadSelectedInstanceById(x.Value.Id); });

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ => SelectedInstance = null);

            OnStart();
        }

        public void LoadTaskCompacts()
        {
            var taskList = _reportService.GetAllTaskCompacts();
            TaskCompacts.Clear();

            foreach (var task in taskList)
            {
                var vtask = _mapper.Map<ViewModelTaskCompact>(task);

                vtask.Schedule = Schedules
                    .FirstOrDefault(s => s.Id == task.ScheduleId)?.Name;

                vtask.RecepientGroup = RecepientGroups
                    .FirstOrDefault(r => r.Id == task.RecepientGroupId)?.Name;

                TaskCompacts.Add(vtask);
            }
        }

        public void LoadSchedules()
        {
            var scheduleList = _reportService.GetSchedules();
            Schedules.Clear();

            foreach (var schedule in scheduleList)
                Schedules.Add(schedule);
        }

        public void LoadRecepientGroups()
        {
            var recepientGroupList = _reportService.GetRecepientGroups();
            RecepientGroups.Clear();

            foreach (var group in recepientGroupList)
                RecepientGroups.Add(group);
        }

        public void LoadSelectedTaskById(int id)
        {
            var apitask = _reportService.GetTaskById(id);
            var selTask = _mapper.Map<ViewModelTask>(apitask);

            selTask.Schedule = Schedules
                .FirstOrDefault(s => s.Id == apitask.ScheduleId)?.Name;

            selTask.RecepientGroup = RecepientGroups
                .FirstOrDefault(r => r.Id == apitask.RecepientGroupId)?.Name;

            SelectedTask = selTask;
        }

        public void SaveTask() // todo:validation for numerical fields
        {
            var result = MessageBox.Show(
                SelectedTask.Id > 0
                    ? "Вы действительно хотите изменить эту задачу?"
                    : "Вы действительно хотите создать эту задачу?", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (SelectedTask.Id > 0)

                {
                    var apiTask = _mapper.Map<ApiTask>(SelectedTask);
                    apiTask.RecepientGroupId = RecepientGroups
                        .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                    apiTask.ScheduleId = Schedules
                        .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                    _reportService.UpdateTask(apiTask);
                    //LoadTaskCompacts(); // why it breaks reactive while other methods no and why not breaks when use method later?
                }

                if (SelectedTask.Id == 0)
                {
                    var apiTask = _mapper.Map<ApiTask>(SelectedTask);
                    apiTask.RecepientGroupId = RecepientGroups
                        .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                    apiTask.ScheduleId = Schedules
                        .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                    _reportService.CreateTask(apiTask);
                }

                LoadTaskCompacts();
            }
        }

        public void CreateTask()
        {
            SelectedTaskCompact = null;
            SelectedTaskInstanceCompacts.Clear();
            SelectedInstance = null;
            SelectedTask = null;
            SelectedTask = new ViewModelTask()
            {
                Id = 0,
                TryCount = 1,
                RecepientGroup = RecepientGroups.First().Name,
                ViewTemplate = "dailyreport_ve",
                Query = "dailyreport_de",
                Schedule = Schedules.First().Name,
                QueryTimeOut = 60,
                TaskType = TaskType.Common
            };
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = _mapper.Map<ViewModelInstance>(_reportService.GetInstanceById(id));
        }

        public void DeleteEntity()
        {
            var result = MessageBox.Show("Вы действительно хотите удалить данную запись?", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (SelectedInstance != null)
                {
                    _reportService.DeleteInstance(SelectedInstance.Id);
                    LoadInstanceCompactsByTaskId(SelectedTask.Id);
                }
                else
                {
                    if (SelectedTask != null && SelectedTask.Id > 0)
                    {
                        _reportService.DeleteTask(SelectedTask.Id);
                        LoadTaskCompacts();
                    }
                }
            }
        }

        public IObservable<Unit> OpenPageInBrowser(string htmlPage)
        {
            return Observable.Start(() =>
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\testreport.html";
                using (FileStream fstr = new FileStream(path, FileMode.Create))
                {
                    byte[] bytePage = System.Text.Encoding.UTF8.GetBytes(htmlPage);
                    fstr.Write(bytePage, 0, bytePage.Length);
                }

                System.Diagnostics.Process.Start(path);
            });
        }

        public IObservable<Unit> GetHtmlPageByTaskId(int taskId)
        {
            return Observable.Start(() =>
            {
                var str = _reportService.GetCurrentTaskViewById(taskId);
                OpenPageInBrowser(str);
            });
        }

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = _reportService.GetInstanceCompactsByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(_mapper.Map<ViewModelInstanceCompact>(instance));
        }



        public void OnStart()
        {
            LoadSchedules();
            LoadRecepientGroups();
            LoadTaskCompacts();
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "TimeOut":
                        int t;
                        if (!Int32.TryParse(SelectedTask.QueryTimeOut.ToString(), out t))
                            return "Enter int value for timeout";
                        break;
                }

                return string.Empty;
            }
        }

        public string Error => this["TimeOut"];
    }
}

