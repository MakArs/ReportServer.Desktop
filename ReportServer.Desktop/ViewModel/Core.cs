using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using MessageBox = System.Windows.MessageBox;
using WindowState = Xceed.Wpf.Toolkit.WindowState;

namespace ReportServer.Desktop.ViewModel
{
    public class Core : ReactiveObject, ICore
    {
        private readonly IReportService _reportService;//
        private readonly IMapper _mapper;//
        private readonly IDialogCoordinator _dialogCoordinator=DialogCoordinator.Instance;

        public ReactiveList<ViewModelTask> TaskCompacts { get; set; }//
        public ReactiveList<ViewModelInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }//
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }//
        public ReactiveList<ApiReport> Reports { get; set; }//
        public ReactiveList<string> ViewTemplates { get; set; }
        public ReactiveList<string> QueryTemplates { get; set; }


        [Reactive] public ViewModelTask SelectedTaskCompact { get; set; }//
        [Reactive] public ViewModelInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public ViewModelFullTask SelectedTask { get; set; }
        [Reactive] public ViewModelInstance SelectedInstance { get; set; }
        [Reactive] public WindowState ViewTemplateChildWindowState { get; set; } = WindowState.Closed;
        [Reactive] public WindowState QueryTemplateChildWindowState { get; set; } = WindowState.Closed;

        public ReactiveCommand RefreshTasksCommand { get; set; }
        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand OpenCurrentTaskView { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }
        public ReactiveCommand SaveTaskCommand { get; set; }
        public ReactiveCommand CreateTaskCommand { get; set; }
        public ReactiveCommand OpenViewTemplateWindowCommand { get; set; }
        public ReactiveCommand SaveViewTemplateCommand { get; set; }
        public ReactiveCommand CancelViewTemplateCommand { get; set; }
        public ReactiveCommand OpenQueryTemplateWindowCommand { get; set; }
        public ReactiveCommand SaveQueryTemplateCommand { get; set; }
        public ReactiveCommand CancelQueryTemplateCommand { get; set; }

        public Core(IReportService reportService, IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;

            TaskCompacts = new ReactiveList<ViewModelTask>(); //  //{ChangeTrackingEnabled = true};
            SelectedTaskInstanceCompacts = new ReactiveList<ViewModelInstanceCompact>();
            Schedules = new ReactiveList<ApiSchedule>();
            RecepientGroups = new ReactiveList<ApiRecepientGroup>();
            Reports=new ReactiveList<ApiReport>();
            RefreshTasksCommand = ReactiveCommand.Create(LoadTaskCompacts);
            ViewTemplates = new ReactiveList<string> {"weeklyreport_ve", "dailyreport_ve"};
            QueryTemplates = new ReactiveList<string> {"weeklyreport_de", "dailyreport_de"};

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
                .WhenAnyValue(t => t.SelectedTask.ViewTemplate, t => t.SelectedTask, (st, vt) =>
                    vt != null && !string.IsNullOrWhiteSpace(st));
            SaveTaskCommand = ReactiveCommand.Create(() => SaveTask(), canSaveTask);

            CreateTaskCommand = ReactiveCommand.Create(CreateTask);

            IObservable<bool> openTWindow = this
                .WhenAnyValue(t => t.SelectedTask.ReportType, st =>
                    st == ReportType.Common);
            OpenViewTemplateWindowCommand =
                ReactiveCommand.Create(() => ViewTemplateChildWindowState = WindowState.Open, openTWindow);
            OpenQueryTemplateWindowCommand =
                ReactiveCommand.Create(() => QueryTemplateChildWindowState = WindowState.Open, openTWindow);


            SaveViewTemplateCommand = ReactiveCommand.Create<string>(templ =>
            {
                SelectedTask.ViewTemplate = templ;
                ViewTemplateChildWindowState = WindowState.Closed;
            });
            CancelViewTemplateCommand = ReactiveCommand.Create(() => ViewTemplateChildWindowState = WindowState.Closed);

            SaveQueryTemplateCommand = ReactiveCommand.Create<string>(templ =>
            {
                SelectedTask.Query = templ;
                QueryTemplateChildWindowState = WindowState.Closed;
            });
            CancelQueryTemplateCommand =
                ReactiveCommand.Create(() => QueryTemplateChildWindowState = WindowState.Closed);

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
                .Subscribe(rId =>
                {
                    var rep = Reports.First(r => r.Id == rId.Value);
                    SelectedTask.ConnectionString = rep.ConnectionString;
                    SelectedTask.Query = rep.Query;
                    SelectedTask.QueryTimeOut = rep.QueryTimeOut;
                    SelectedTask.ViewTemplate = rep.ViewTemplate;
                    SelectedTask.ReportType = (ReportType)rep.ReportType;
                });

            OnStart();
        }

        public void LoadTaskCompacts()//
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
                Reports.Add(rep);
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

        public async Task SaveTask() 
        {
            var ts = _dialogCoordinator.ShowMessageAsync(this, "Warning",
                SelectedTask.Id > 0
                    ? "Вы действительно хотите изменить эту задачу?"
                    : "Вы действительно хотите создать эту задачу?"
                ,MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(continueOnCapturedContext: false);
            
            var result = await  ts;
            //var result = MessageBox.Show(
            //    SelectedTask.Id > 0
            //        ? "Вы действительно хотите изменить эту задачу?"
            //        : "Вы действительно хотите создать эту задачу?", "Warning",
            //    MessageBoxButton.YesNo, MessageBoxImage.Question);
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

                LoadTaskCompacts();
                SelectedTask = null;
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
                ReportId = Reports.First().Id,
                TryCount = 1,
                Schedule = Schedules.First().Name,
                RecepientGroup = RecepientGroups.First().Name,
            };
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = _mapper.Map<ViewModelInstance>(_reportService.GetFullInstanceById(id));
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
        }
    }
}

