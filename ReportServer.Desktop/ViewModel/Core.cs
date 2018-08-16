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
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using ReportServer.Desktop.Views.WpfResources;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore
    {
        private readonly IReportService     reportService; 
        private readonly IMapper            mapper;        
        private readonly IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;

        public ReactiveList<DesktopTask>            TaskCompacts                 { get; set; } //
        public ReactiveList<DesktopInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule>              Schedules                    { get; set; } //
        public ReactiveList<ApiRecepientGroup>        RecepientGroups              { get; set; } //
        public ReactiveList<DesktopReport>          Reports                      { get; set; } //
        public ReactiveList<string>                   ViewTemplates                { get; set; }
        public ReactiveList<string>                   QueryTemplates               { get; set; }

        [Reactive] public object SelectTab { get; set; }
        [Reactive] public DesktopTask SelectedTaskCompact { get; set; } //
        [Reactive] public DesktopInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public DesktopFullTask SelectedTask { get; set; }
        [Reactive] public DesktopInstance SelectedInstance { get; set; }
        [Reactive] public DesktopReport SelectedReport { get; set; }
        [Reactive] public DesktopReport RedactedReport { get; set; }

        // [Reactive] public bool CanSaveTask { get; set; }
        // [Reactive] public bool CanSaveReport { get; set; }
        public ReactiveCommand RefreshTasksCommand            { get; set; }
        public ReactiveCommand OpenPage                       { get; set; }
        public ReactiveCommand OpenCurrentTaskView            { get; set; }
        public ReactiveCommand DeleteCommand                  { get; set; }
        public ReactiveCommand SaveEntityCommand              { get; set; }
        public ReactiveCommand CreateTaskCommand              { get; set; }
        public ReactiveCommand OpenViewTemplateWindowCommand  { get; set; }
        public ReactiveCommand OpenQueryTemplateWindowCommand { get; set; }
        public ReactiveCommand CreateReportCommand            { get; set; }
        
        public Core(IReportService reportService, IMapper mapper)
        { 
            this.reportService = reportService;
            this.mapper        = mapper;

            TaskCompacts                 = new ReactiveList<DesktopTask>(); //  //{ChangeTrackingEnabled = true};
            SelectedTaskInstanceCompacts = new ReactiveList<DesktopInstanceCompact>();
            Schedules                    = new ReactiveList<ApiSchedule>();
            RecepientGroups              = new ReactiveList<ApiRecepientGroup>();
            Reports                      = new ReactiveList<DesktopReport>() {ChangeTrackingEnabled = false};
            RefreshTasksCommand          = ReactiveCommand.Create(LoadTaskCompacts);
            ViewTemplates =
                new ReactiveList<string>
                {
                    "weeklyreport_ve",
                    "dailyreport_ve"
                }; //todo: auto adding named registrations names when using private bootstrapper at reportservice
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
                    t => t.RedactedReport,
                    (stb, st, sr) =>
                        (stb?.GetType() == typeof(TaskEditorView) &&
                         st?.ReportId   > 0) ||
                        (stb?.GetType() == typeof(ReportEditorView) &&
                         !string.IsNullOrEmpty(sr?.Name)                  &&
                         !string.IsNullOrEmpty(sr.Query)                  &&
                         !string.IsNullOrEmpty(sr.ViewTemplate)           &&
                         sr.QueryTimeOut > 0)
                );



            //this.ObservableForProperty(t => t.SelectTab)
            //    .Where(stb =>
            //       stb.Value?.GetType() == typeof(SelectedTaskFullView) || stb.Value?.GetType() == typeof(SelectedReportFullView))
            //    .Subscribe(_ =>
            //    {
            //        if(SelectTab.GetType() == typeof(SelectedTaskFullView))
            //        {
            //            if (SelectedTask?.ReportId > 0) canSaveTask = true;
            //        }
            //        else
            //        {
            //            if (!string.IsNullOrEmpty(SelectedReport?.Name) &&
            //            !string.IsNullOrEmpty(SelectedReport.Query) &&
            //            !string.IsNullOrEmpty(SelectedReport.ViewTemplate) &&
            //            SelectedReport.QueryTimeOut > 0) canSaveReport = true;
            //        }
            //    });

            //IObservable<bool> canSave = this.WhenAnyValue(t => t.canSaveReport, t => t.canSaveTask,
            //    (csr, cst) => csr || cst);

            //IObservable<bool> canSaveTask = this.WhenAnyValue(t => t.SelectTab,
            //    t => t.SelectedTask.ReportId,
            //    (stb, r) =>
            //        stb?.GetType() == typeof(SelectedTaskFullView) &&
            //        r > 0);
            //IObservable<bool> canSaveReport = this.WhenAnyValue(t => t.SelectTab,
            //    t => t.RedactedReport.Name,
            //    t => t.RedactedReport.Query,
            //    t => t.RedactedReport.ViewTemplate,
            //    (stb, n, q, v) =>
            //        stb?.GetType() == typeof(SelectedReportFullView) &&
            //        !string.IsNullOrEmpty(n) &&
            //        !string.IsNullOrEmpty(q) &&
            //        !string.IsNullOrEmpty(v));

            //IObservable<bool> canSave=canSaveReport.Merge(canSaveTask); //works on last observable, not on any


            SaveEntityCommand = ReactiveCommand.Create(SaveEntity, canSave);

            CreateTaskCommand   = ReactiveCommand.Create(CreateTask);
            CreateReportCommand = ReactiveCommand.Create(CreateReport);

            IObservable<bool> canOpenReportModal = this
                .WhenAnyValue(t => t.RedactedReport.ReportType, sr =>
                    sr == ReportType.Common); 
            OpenViewTemplateWindowCommand =
                ReactiveCommand.CreateFromTask(async () => RedactedReport.ViewTemplate = await DataRedacting(),
                    canOpenReportModal);
            OpenQueryTemplateWindowCommand =
                ReactiveCommand.CreateFromTask(async () => RedactedReport.Query = await DataRedacting(),
                    canOpenReportModal);


            this.WhenAnyObservable(s =>     //
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

            this.ObservableForProperty(s => s.SelectedReport)
                .Where(x => x.Value != null)
                .Subscribe(x =>
                {
                    RedactedReport = x.Value.CreateClone();
                    RedactedReport = x.Value.CreateClone();
                });

            this.ObservableForProperty(s => s.RedactedReport.ReportType)
                .Subscribe(_ =>
                {
                    RedactedReport.Query        = null;
                    RedactedReport.ViewTemplate = null;
                });

            this.ObservableForProperty(s => s.SelectedTaskCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ => { SelectedTask = null; });

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value != null)
                .Subscribe(x => LoadSelectedInstanceById(x.Value.Id));

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Where(x => x.Value == null)
                .Subscribe(_ => SelectedInstance = null);

            this.ObservableForProperty(s => s.SelectedTask.ReportId)
                .Where(rId => rId.Value != 0)
                .Subscribe(rId =>
                {
                    var rep = Reports.First(r => r.Id == rId.Value);
                    SelectedTask.ConnectionString = rep.ConnectionString;
                    SelectedTask.Query            = rep.Query;
                    SelectedTask.QueryTimeOut     = rep.QueryTimeOut;
                    SelectedTask.ViewTemplate     = rep.ViewTemplate;
                    SelectedTask.ReportType       = rep.ReportType;
                });

            OnStart();
        }

        public void LoadTaskCompacts() //
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

        public void LoadSchedules()
        {
            var scheduleList = reportService.GetSchedules();
            Schedules.Clear();

            foreach (var schedule in scheduleList)
                Schedules.Add(schedule);
        }

        public void LoadRecepientGroups()
        {
            var recepientGroupList = reportService.GetRecepientGroups();
            RecepientGroups.Clear();

            foreach (var group in recepientGroupList)
                RecepientGroups.Add(group);
        }

        public void LoadReports()
        {
            var reportsList = reportService.GetReports();
            Reports.Clear();

            foreach (var rep in reportsList)
                Reports.Add(mapper.Map<DesktopReport>(rep));
        }

        public void LoadSelectedTaskById(int id)
        {
            var apitask = reportService.GetFullTaskById(id);
            var selTask = mapper.Map<DesktopFullTask>(apitask);

            selTask.Schedule = Schedules
                .FirstOrDefault(s => s.Id == apitask.ScheduleId)?.Name;

            selTask.RecepientGroup = RecepientGroups
                .FirstOrDefault(r => r.Id == apitask.RecepientGroupId)?.Name;

            SelectedTask = selTask;
        }

        public async Task<string> DataRedacting()
        {
            var data   = dialogCoordinator.ShowInputAsync(this, "Hello", "Enter data for this field");
            var result = await data;
            return result;
        }

        public async Task SaveEntity()
        {
            switch (SelectTab)
            {
                case TaskEditorView _:
                {
                    var ts = dialogCoordinator.ShowMessageAsync(this, "Warning",
                        SelectedTask.Id > 0
                            ? "Вы действительно хотите изменить эту задачу?"
                            : "Вы действительно хотите создать эту задачу?"
                        , MessageDialogStyle.AffirmativeAndNegative); //.ConfigureAwait(continueOnCapturedContext: false);

                    var result = await ts;

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (SelectedTask.Id > 0)

                        {
                            var apiTask = mapper.Map<ApiFullTask>(SelectedTask);
                            apiTask.RecepientGroupId = RecepientGroups
                                .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                            apiTask.ScheduleId = Schedules
                                .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                            reportService.UpdateTask(apiTask);
                            //LoadTaskCompacts(); // why it breaks reactive while other methods no and why not breaks when use method later?
                        }

                        if (SelectedTask.Id == 0)
                        {
                            var apiTask = mapper.Map<ApiFullTask>(SelectedTask);
                            apiTask.RecepientGroupId = RecepientGroups
                                .FirstOrDefault(r => r.Name == SelectedTask.RecepientGroup)?.Id;
                            apiTask.ScheduleId = Schedules
                                .FirstOrDefault(s => s.Name == SelectedTask.Schedule)?.Id;

                            reportService.CreateTask(apiTask);
                        }

                        OnStart();
                        //LoadTaskCompacts();
                        //SelectedTask = null;
                    }

                    break;
                } //case

                case ReportEditorView _:
                {
                    var ts = dialogCoordinator.ShowMessageAsync(this, "Warning",
                        RedactedReport.Id > 0
                            ? "Вы действительно хотите изменить этот отчёт?"
                            : "Вы действительно хотите создать этот отчёт?"
                        , MessageDialogStyle.AffirmativeAndNegative);
                    var result = await ts;

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (RedactedReport.Id > 0)

                        {
                            var apiRep = mapper.Map<ApiReport>(RedactedReport);
                            reportService.UpdateReport(apiRep);
                        }

                        if (RedactedReport.Id == 0)
                        {
                            var apiRep = mapper.Map<ApiReport>(RedactedReport);
                            reportService.CreateReport(apiRep);
                        }

                        OnStart();
                    }

                    break;
                } //case
            }
        }

        public void CreateTask()
        {
            //var saveDialog = new SaveFileDialog()
            //{
            //    Filter = "SQLite DataBase | *.db",
            //    Title = "Создание базы"
            //};
            //var t = "";
            //saveDialog.ShowDialog();
            //if (!string.IsNullOrEmpty(saveDialog.FileName))
            //    t= $"Data Source = {saveDialog.FileName}";
            SelectedTaskCompact = null;
            SelectedTaskInstanceCompacts.Clear();
            SelectedInstance = null;
            SelectedTask     = null;
            SelectedTask = new DesktopFullTask()
            {
                Id             = 0,
                TryCount       = 1,
                Schedule       = Schedules.First().Name,
                RecepientGroup = RecepientGroups.First().Name
            };
            SelectedTask.ReportId = Reports.First().Id;
            SelectedTask          = SelectedTask;
        }

        public void CreateReport()
        {
            RedactedReport = null;
            RedactedReport = new DesktopReport()
            {
                Id           = 0,
                QueryTimeOut = 60
            };
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = mapper.Map<DesktopInstance>(reportService.GetFullInstanceById(id));
        }

        public async Task DeleteEntity()
        {
            var ts = dialogCoordinator.ShowMessageAsync(this, "Warning",
                "Вы действительно хотите удалить данную запись?"
                , MessageDialogStyle.AffirmativeAndNegative);
            var result = await ts;

            if (result == MessageDialogResult.Affirmative)
            {
                switch (SelectTab)
                {
                    case TaskListView _:
                    {
                        reportService.DeleteTask(SelectedTask.Id);
                        LoadTaskCompacts();
                        break;
                    }
                    //case ReportListView _:
                    //{
                    //    break;
                    //}
                    case SelectedTaskInstancesView _:
                    {
                        reportService.DeleteInstance(SelectedInstance.Id);
                        LoadInstanceCompactsByTaskId(SelectedTask.Id);
                        break;
                    }
                }
            }
        }

        public void OpenPageInBrowser(string htmlPage)
        {

            var path = $"{AppDomain.CurrentDomain.BaseDirectory}testreport.html";
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
                var str = reportService.GetCurrentTaskViewById(taskId);
                OpenPageInBrowser(str);
            });
        }

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = reportService.GetInstancesByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(mapper.Map<DesktopInstanceCompact>(instance));
        }



        public void OnStart()
        {
            LoadSchedules();
            LoadRecepientGroups();
            LoadReports();
            LoadTaskCompacts();
            SelectTab = null;
        }

        public string Title { get; set; }
        public string FullTitle { get; set; }
    }
}