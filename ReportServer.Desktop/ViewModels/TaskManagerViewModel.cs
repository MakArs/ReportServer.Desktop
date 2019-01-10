using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using ReportService;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class TaskManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        public CachedServiceShell Shell { get; }

        [Reactive]public ReadOnlyObservableCollection<DesktopTask> Tasks { get; set; }
        private readonly SourceList<DesktopTaskInstance> selectedTaskInstances;
        [Reactive] public ReadOnlyObservableCollection<DesktopTaskInstance> SelectedTaskInstances { get; set; }
        private readonly SourceList<DesktopOperInstance> operInstances;
        [Reactive] public ReadOnlyObservableCollection<DesktopOperInstance> OperInstances { get; set; }

        [Reactive] public DesktopTask SelectedTask { get; set; }
        [Reactive] public DesktopTaskInstance SelectedTaskInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedOperInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedInstanceData { get; set; }

        public ReactiveCommand<string, Unit> OpenPage { get; set; }
        public ReactiveCommand<Unit, Unit> EditTaskCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<int, string> StopTaskCommand { get; set; }

        public TaskManagerViewModel(ICachedService cachedService, IMapper mapper, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.mapper = mapper;
            Shell = shell as CachedServiceShell;
            var packageBuilder = new ProtoPackageBuilder();

            selectedTaskInstances = new SourceList<DesktopTaskInstance>();
            operInstances = new SourceList<DesktopOperInstance>();

            IObservable<bool> canOpenInstancePage = this //todo:some check for is viewdataset?"
                .WhenAnyValue(t => t.SelectedInstanceData,
                    si => !string.IsNullOrEmpty(si?.DataSet));

            OpenPage = ReactiveCommand.Create<string>
                (cachedService.OpenPageInBrowser, canOpenInstancePage);

            EditTaskCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedTask == null) return;
                var id = SelectedTask.Id;

                var name = $"Task {id} editor";
                Shell.ShowView<TaskEditorView>(new TaskEditorRequest
                    {
                        ViewId = name,
                        Task = cachedService
                            .Tasks.Items.FirstOrDefault(task => task.Id == id),
                        TaskOpers = cachedService.Operations.Items.Where(to => to.TaskId == id).ToList()
                    },
                    new UiShowOptions {Title = name});
            });

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete());

            StopTaskCommand = ReactiveCommand.CreateFromTask<int, string>(async par =>
            {
                var t= await this.cachedService.StopTaskByInstanceId(par);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return t;
            });

            this.WhenAnyValue(s => s.SelectedTask)
                .Where(x => x != null)
                .Subscribe(x => LoadInstanceCompactsByTaskId(x.Id));

            this.WhenAnyValue(s => s.SelectedTaskInstance)
                .Subscribe(x =>
                {
                    if (x == null)
                        operInstances.Clear();
                    else
                    {
                        operInstances.ClearAndAddRange(
                            cachedService.GetOperInstancesByTaskInstanceId(x.Id)
                                .Select(mapper.Map<DesktopOperInstance>));
                    }
                });

            this.WhenAnyValue(s => s.SelectedOperInstance)
                .Subscribe(async x =>
                {
                    var data = new DesktopOperInstance();

                    if (x != null)
                    {
                        var fullInstance = cachedService
                            .GetFullOperInstanceById(SelectedOperInstance.Id);

                        data = mapper.Map<DesktopOperInstance>(fullInstance);

                        if (fullInstance.DataSet != null)
                        {
                            try
                            {
                            data.DataSet = JsonConvert.SerializeObject(packageBuilder.GetPackageValues(
                                OperationPackage.Parser.ParseFrom(fullInstance.DataSet)));
                            }
                            catch 
                            {
                              await  Shell.ShowMessageAsync("Exception occured during dataset decoding");
                            }
                        }
                    }
                    
                    SelectedInstanceData = x == null
                        ? null
                        : data;
                });
        }

        private void LoadInstanceCompactsByTaskId(int taskId)
        {
            selectedTaskInstances
                .ClearAndAddRange(cachedService.GetInstancesByTaskId(taskId)
                    .Select(ti => mapper.Map<DesktopTaskInstance>(ti)));
        }

        private void RefreshTaskList()
        {
            cachedService.Tasks
                .Connect()
                .Transform(task => new DesktopTask
                {
                    Id = task.Id,
                    Name = task.Name,
                    Schedule = cachedService.Schedules.Items
                        .FirstOrDefault(sched => sched.Id == task.ScheduleId)?.Schedule,

                    Operations = string.Join("=>", cachedService.Operations
                        .Items.Where(oper => oper.TaskId == task.Id)
                        .Select(oper => new
                        {
                            oper.Number,
                            oper.Name
                        })
                        .OrderBy(pair => pair.Number)
                        .Select(pair => pair.Name)
                        .ToList())
                })
                .Bind(out var temp)
                .Subscribe();

            cachedService.Tasks.Connect()
                .Subscribe(_ =>
                {
                    SelectedTask = null;
                    selectedTaskInstances.Clear();
                    operInstances.Clear();
                });

            selectedTaskInstances.Connect()
                .Bind(out var tempti)
                .Subscribe();

            SelectedTaskInstances = tempti;

            OperInstances = operInstances.SpawnCollection();

            Tasks = temp;
        }

        public void Initialize(ViewRequest viewRequest)
        {
            RefreshTaskList();

            Shell.AddGlobalCommand("File", "Refresh",
                    "Shell.RefreshCommand", this)
                .SetHotKey(ModifierKeys.None, Key.F5);

            Shell.AddGlobalCommand("Edit", "New task",
                    "Shell.CreateTaskCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.T);

            Shell.AddGlobalCommand("Edit", "New operation template",
                    "Shell.CreateOperTemplateCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.O);

            Shell.AddGlobalCommand("Edit", "New schedule",
                    "Shell.CreateScheduleCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.H);

            Shell.AddVMCommand("Edit", "Change task",
                "EditTaskCommand", this);

            Shell.AddVMCommand("File", "Delete",
                    "DeleteCommand", this)
                .SetHotKey(ModifierKeys.None, Key.Delete);

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.CountChanged)
                .Subscribe(_ => RefreshTaskList());
        }

        private async Task Delete()
        {
            if (SelectedTaskInstance != null)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ("Do you really want to delete this task instance?")) return;

                cachedService.DeleteInstance(SelectedTaskInstance.Id);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return;
            }

            if (SelectedTask != null)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ($"Do you really want to delete task {SelectedTask.Name} and all it's instances?")
                ) return;

                cachedService.DeleteTask(SelectedTask.Id);
                cachedService.RefreshData();
            }
        }
    }
}