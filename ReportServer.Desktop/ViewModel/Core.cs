using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Views;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore
    {
        private readonly IReportService _reportService; //
        private readonly IMapper _mapper; //
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;

        public ReactiveList<ViewModelTask> TaskCompacts { get; set; } //
        public ReactiveList<ViewModelInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; } //
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; } //
        public ReactiveList<ViewModelReport> Reports { get; set; } //
        public ReactiveList<string> ViewTemplates { get; set; }
        public ReactiveList<string> QueryTemplates { get; set; }

        [Reactive] public object SelectTab { get; set; }
        [Reactive] public ViewModelTask SelectedTaskCompact { get; set; } //
        [Reactive] public ViewModelInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public ViewModelFullTask SelectedTask { get; set; }
        [Reactive] public ViewModelInstance SelectedInstance { get; set; }
        [Reactive] public ViewModelReport SelectedReport { get; set; }

        public ReactiveCommand RefreshTasksCommand { get; set; }
        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand OpenCurrentTaskView { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }
        public ReactiveCommand SaveEntityCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }
        public ReactiveCommand OpenViewTemplateWindowCommand { get; set; }
        public ReactiveCommand OpenQueryTemplateWindowCommand { get; set; }
        public ReactiveCommand CreateReportCommand { get; set; }

        public Core(IReportService reportService, IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;

            TaskCompacts = new ReactiveList<ViewModelTask>(); //  //{ChangeTrackingEnabled = true};
            SelectedTaskInstanceCompacts = new ReactiveList<ViewModelInstanceCompact>();
            Schedules = new ReactiveList<ApiSchedule>();
            RecepientGroups = new ReactiveList<ApiRecepientGroup>();
            Reports = new ReactiveList<ViewModelReport>();
            RefreshTasksCommand = ReactiveCommand.Create(LoadTaskCompacts);
            ViewTemplates = new ReactiveList<string> {"weeklyreport_ve", "dailyreport_ve"};
            QueryTemplates = new ReactiveList<string> {"weeklyreport_de", "dailyreport_de"};

            IObservable<bool> canOpenInstancePage = this
                .WhenAnyValue(t => t.SelectedInstance,
                    si => !string.IsNullOrEmpty(si?.ViewData));
            OpenPage = ReactiveCommand.Create<string>
                (OpenPageInBrowser, canOpenInstancePage);

            IObservable<bool> canOpenCurrentTaskView = this
                .WhenAnyValue(t => t.SelectedTask,
                    st => st?.Id > 0); //why don't work while create task?...
            OpenCurrentTaskView =
                ReactiveCommand.CreateFromObservable<int, Unit>
                    (GetHtmlPageByTaskId, canOpenCurrentTaskView);

            IObservable<bool> canDelete = this
                .WhenAnyValue(t => t.SelectTab,
                    t => t.SelectedTask,
                    t => t.SelectedReport,
                    s => s.SelectedInstanceCompact,
                    (stb, stc, srp, sic) =>
                        stb != null &&
                        ((stb.GetType() == typeof(TaskListView) && stc != null) ||
                         //(stb.GetType() == typeof(ReportListView)&& srp != null) ||
                         (stb.GetType() == typeof(SelectedTaskInstancesView) && sic != null)
                        ));
            DeleteCommand = ReactiveCommand.Create(DeleteEntity, canDelete);

            IObservable<bool> canSave = this
                .WhenAnyValue(t => t.SelectTab, 
                    t => t.SelectedTask,
                    t => t.SelectedReport,
                    (stb,st, sr) =>
                        (stb?.GetType() == typeof(SelectedTaskFullView) && 
                         st?.ReportId > 0) ||
                        (stb?.GetType() == typeof(SelectedReportFullView)&&
                         !string.IsNullOrEmpty(sr?.Name) &&
                         !string.IsNullOrEmpty(sr.Query) &&
                         !string.IsNullOrEmpty(sr.ViewTemplate) &&
                         sr.QueryTimeOut > 0)
                );
            SaveEntityCommand = ReactiveCommand.Create(SaveEntity, canSave);

            CreateTaskCommand = ReactiveCommand.Create(CreateTask);
            CreateReportCommand = ReactiveCommand.Create(CreateReport);

            IObservable<bool> canOpenReportModal = this
                .WhenAnyValue(t => t.SelectedReport.ReportType, sr =>
                    sr == ReportType.Common);
            OpenViewTemplateWindowCommand =
                ReactiveCommand.CreateFromTask(async () => SelectedReport.ViewTemplate = await DataRedacting(),
                    canOpenReportModal);
            OpenQueryTemplateWindowCommand =
                ReactiveCommand.CreateFromTask(async () => SelectedReport.Query = await DataRedacting(),
                    canOpenReportModal);

            this.WhenAnyObservable(s => //
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

            this.ObservableForProperty(s => s.SelectedTask.ReportId)
                .Where(rId => rId.Value != 0)
                .Subscribe(rId =>
                {
                    var rep = Reports.First(r => r.Id == rId.Value);
                    SelectedTask.ConnectionString = rep.ConnectionString;
                    SelectedTask.Query = rep.Query;
                    SelectedTask.QueryTimeOut = rep.QueryTimeOut;
                    SelectedTask.ViewTemplate = rep.ViewTemplate;
                    SelectedTask.ReportType = rep.ReportType;
                });

            OnStart();
        }

        public void LoadTaskCompacts() //
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

        public void LoadReports()
        {
            var reportsList = _reportService.GetReports();
            Reports.Clear();

            foreach (var rep in reportsList)
                Reports.Add(_mapper.Map<ViewModelReport>(rep));
        }

        public void LoadSelectedTaskById(int id)
        {
            var apitask = _reportService.GetFullTaskById(id);
            var selTask = _mapper.Map<ViewModelFullTask>(apitask);

            selTask.Schedule = Schedules
                .FirstOrDefault(s => s.Id == apitask.ScheduleId)?.Name;

            selTask.RecepientGroup = RecepientGroups
                .FirstOrDefault(r => r.Id == apitask.RecepientGroupId)?.Name;

            SelectedTask = selTask;
        }

        public async Task<string> DataRedacting()
        {
            var data = _dialogCoordinator.ShowInputAsync(this, "Hello", "Enter data for this field");
            var result = await data;
            return result;
        }

        public async Task SaveEntity()
        {
            switch (SelectTab)
            {
                case SelectedTaskFullView _:
                {
                    var ts = _dialogCoordinator.ShowMessageAsync(this, "Warning",
                        SelectedTask.Id > 0
                            ? "Вы действительно хотите изменить эту задачу?"
                            : "Вы действительно хотите создать эту задачу?"
                        , MessageDialogStyle.AffirmativeAndNegative); //.ConfigureAwait(continueOnCapturedContext: false);

                    var result = await ts;

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (SelectedTask.Id > 0)

                        {
                            var apiTask = _mapper.Map<ApiFullTask>(SelectedTask);
                            apiTask.RecepientGroupId = RecepientGroups
                                .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                            apiTask.ScheduleId = Schedules
                                .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                            _reportService.UpdateTask(apiTask);
                            //LoadTaskCompacts(); // why it breaks reactive while other methods no and why not breaks when use method later?
                        }

                        if (SelectedTask.Id == 0)
                        {
                            var apiTask = _mapper.Map<ApiFullTask>(SelectedTask);
                            apiTask.RecepientGroupId = RecepientGroups
                                .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                            apiTask.ScheduleId = Schedules
                                .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                            _reportService.CreateTask(apiTask);
                        }
                        OnStart();
                        //LoadTaskCompacts();
                        //SelectedTask = null;
                    }
                    break;
                    }//case

                case SelectedReportFullView _:
                {
                    var ts = _dialogCoordinator.ShowMessageAsync(this, "Warning",
                        SelectedReport.Id > 0
                            ? "Вы действительно хотите изменить этот отчёт?"
                            : "Вы действительно хотите создать этот отчёт?"
                        , MessageDialogStyle.AffirmativeAndNegative);
                    var result = await ts;

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (SelectedReport.Id > 0)

                        {
                            var apiRep = _mapper.Map<ApiReport>(SelectedReport);
                           _reportService.UpdateReport(apiRep);
                        }

                        if (SelectedReport.Id == 0)
                        {
                            var apiRep = _mapper.Map<ApiReport>(SelectedReport);
                            _reportService.CreateReport(apiRep);
                        }
                        OnStart();
                    }
                        break;
                } //case
            }
        }

        public void CreateTask()
        {
            SelectedTaskCompact = null;
            SelectedTaskInstanceCompacts.Clear();
            SelectedInstance = null;
            SelectedTask = null;
            SelectedTask = new ViewModelFullTask()
            {
                Id = 0,
                TryCount = 1,
                Schedule = Schedules.First().Name,
                RecepientGroup = RecepientGroups.First().Name
            };
            SelectedTask.ReportId = Reports.First().Id;
            SelectedTask = SelectedTask;
        }

        public void CreateReport()
        {
            SelectedReport = null;
            SelectedReport = new ViewModelReport()
            {
                Id = 0,
                QueryTimeOut = 60
            };
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = _mapper.Map<ViewModelInstance>(_reportService.GetFullInstanceById(id));
        }

        public async Task DeleteEntity()
        {
            var ts = _dialogCoordinator.ShowMessageAsync(this, "Warning",
                "Вы действительно хотите удалить данную запись?"
                , MessageDialogStyle.AffirmativeAndNegative);
            var result = await ts;

            if (result == MessageDialogResult.Affirmative)
            {
                switch (SelectTab)
                {
                    case TaskListView _:
                    {
                        _reportService.DeleteTask(SelectedTask.Id);
                        LoadTaskCompacts();
                        break;
                    }
                    //case ReportListView _:
                    //{
                    //    break;
                    //}
                    case SelectedTaskInstancesView _:
                    {
                        _reportService.DeleteInstance(SelectedInstance.Id);
                        LoadInstanceCompactsByTaskId(SelectedTask.Id);
                        break;
                    }
                }
            }
        }

        public void OpenPageInBrowser(string htmlPage)
        {

            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\testreport.html";
            using (FileStream fstr = new FileStream(path, FileMode.Create))
            {
                byte[] bytePage = System.Text.Encoding.UTF8.GetBytes(htmlPage);
                fstr.Write(bytePage, 0, bytePage.Length);
            }

            System.Diagnostics.Process.Start(path);
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
            var instanceList = _reportService.GetInstancesByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(_mapper.Map<ViewModelInstanceCompact>(instance));
        }



        public void OnStart()
        {
            LoadSchedules();
            LoadRecepientGroups();
            LoadReports();
            LoadTaskCompacts();
            SelectTab = null;
        }
    }
}

