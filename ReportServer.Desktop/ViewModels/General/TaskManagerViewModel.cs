using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
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

namespace ReportServer.Desktop.ViewModels.General
{
    public class TaskManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        public CachedServiceShell Shell { get; }

        [Reactive] public string TaskNameSearchString { get; set; }
        [Reactive] public string TaskIdSearchString { get; set; }
        [Reactive] public bool AllGroupsExpanded { get; set; }
        [Reactive] public ReadOnlyObservableCollection<DesktopTask> Tasks { get; set; }
        private readonly SourceList<DesktopTaskInstance> selectedTaskInstances;
        [Reactive] public ReadOnlyObservableCollection<DesktopTaskInstance> SelectedTaskInstances { get; set; }
        private readonly SourceList<DesktopOperInstance> operInstances;
        [Reactive] public ReadOnlyObservableCollection<DesktopOperInstance> OperInstances { get; set; }

        [Reactive] public DesktopTask SelectedTask { get; set; }
        [Reactive] public DesktopTaskInstance SelectedTaskInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedOperInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedInstanceData { get; set; }

        public ReactiveCommand<string, Unit> OpenPage { get; set; }
        //public ReactiveCommand<Unit, Unit> SwitchExpandCommand { get; set; }
        public ReactiveCommand<Unit, Unit> EditTaskCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CopyTaskCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }
        public ReactiveCommand<long, string> StopTaskCommand { get; set; }
        public ReactiveCommand<DesktopTask, Unit> RunTaskCommand { get; set; }

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

            //SwitchExpandCommand = ReactiveCommand.Create(() => { AllGroupsExpanded = !AllGroupsExpanded; });

            OpenPage = ReactiveCommand.Create<string>
                (cachedService.OpenPageInBrowser, canOpenInstancePage);

            EditTaskCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedTask == null) return;
                var id = SelectedTask.Id;

                cachedService.RefreshData();
                if (cachedService
                        .Tasks.Items.FirstOrDefault(task => task.Id == id) == null)
                {
                    await Shell.ShowMessageAsync("Task not longer exists");
                    return;
                }

                var name = $"Task {id} editor";
                Shell.ShowView<TaskEditorView>(new TaskEditorRequest
                {
                    ViewId = name,
                    Task = cachedService
                            .Tasks.Items.FirstOrDefault(task => task.Id == id),
                    TaskOpers = cachedService.Operations.Items.Where(to => to.TaskId == id).ToList()
                },
                    new UiShowOptions { Title = name });
            });

            var canCopy = this.WhenAnyValue(tvm => tvm.SelectedTask, stsk => stsk?.Id > 0)
                .Concat(Shell.CanEdit);

            CopyTaskCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedTask == null) return;
                var id = SelectedTask.Id;

                cachedService.RefreshData();

                if (cachedService
                        .Tasks.Items.FirstOrDefault(task => task.Id == id) is var selectedTask && selectedTask == null)
                {
                    await Shell.ShowMessageAsync("Task not longer exists");
                    return;
                }

                var name = $"Task {id} copy";

                selectedTask.Id = 0;

                var taskOpers = cachedService.Operations.Items.Where(to => to.TaskId == id)
                    .ToList();
                taskOpers.ForEach(oper => oper.Id = 0);

                var copyRequest = new TaskEditorRequest
                {
                    ViewId = name,
                    Task = selectedTask,
                    TaskOpers = taskOpers
                };

                Shell.ShowView<TaskEditorView>(copyRequest,
                    new UiShowOptions { Title = name });
            }, canCopy);

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete(), Shell.CanEdit);


            StopTaskCommand = ReactiveCommand.CreateFromTask<long, string>(async par =>
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync("Сancel task execution?"))
                    return "False";

                var t = await this.cachedService.StopTaskByInstanceId(par);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return t;
            }, Shell.CanStopRun);

            RunTaskCommand = ReactiveCommand.CreateFromTask<DesktopTask>(async par =>
            {
                var workingInstances = await cachedService.GetWorkingTaskInstancesById(par.Id);
                if (workingInstances.Count > 0)
                    await Shell.ShowMessageAsync(
                        $"This task already has working instances ({string.Join(", ", workingInstances)}). " +
                        "You can try to execute it later");
                else
                {
                    if (!await Shell.ShowWarningAffirmativeDialogAsync($"Do you want to execute task {par.Name}?"))
                        return;

                    var res = await cachedService.StartTaskById(par.Id);
                    LoadInstanceCompactsByTaskId(SelectedTask.Id);
                    await Shell.ShowMessageAsync(res, "Task execution");
                }

            }, Shell.CanStopRun);

            this.ObservableForProperty(s => s.TaskNameSearchString)
                .Subscribe(sstr => RefreshTaskList(TaskIdSearchString, sstr.Value));

            this.ObservableForProperty(s => s.TaskIdSearchString)
                .Subscribe(idstr => RefreshTaskList(idstr.Value, TaskNameSearchString));

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
                        operInstances.ClearAndAddRange(GetNamedOperInstancesByTaskInstanceId(x.Id));
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

                        data.OperName = cachedService.Operations.Items
                            .FirstOrDefault(op => op.Id == fullInstance.OperationId)?.Name;

                        if (fullInstance.DataSet != null)
                        {
                            try
                            {
                                data.DataSet = JsonConvert.SerializeObject(packageBuilder.GetPackageValues(
                                    OperationPackage.Parser.ParseFrom(fullInstance.DataSet)));
                            }
                            catch
                            {
                                await Shell.ShowMessageAsync("Exception occured during dataset decoding");
                            }
                        }
                    }

                    SelectedInstanceData = x == null
                        ? null
                        : data;
                });
        }

        private IEnumerable<DesktopOperInstance> GetNamedOperInstancesByTaskInstanceId(long id)
        {
            var instances = cachedService.GetOperInstancesByTaskInstanceId(id)
                                  .Select(mapper.Map<DesktopOperInstance>).ToList();

            instances.ForEach(apiinst => apiinst.OperName = cachedService.Operations.Items
               .FirstOrDefault(op => op.Id == apiinst.OperationId)?.Name);

            return instances;
        }

        private void LoadInstanceCompactsByTaskId(long taskId)
        {
            selectedTaskInstances
                .ClearAndAddRange(cachedService.GetInstancesByTaskId(taskId)
                    .Select(ti => mapper.Map<DesktopTaskInstance>(ti)).OrderByDescending(inst => inst.StartTime));
        }

        private static string ParseGroupName(string name)
        {
            var match = Regex.Match(name, @"\[(?<groupName>.*?)\].*");
            return match.Success
                ? match.Groups["groupName"].Value
                : "[Default]";
        }

        private void RefreshTaskList(string idTemplate = null, string nameTemplate = null)
        {
            cachedService.Tasks
                .Connect()
                .Filter(task =>
                    string.IsNullOrEmpty(idTemplate) ||
                    task.Id.ToString().Contains(idTemplate.ToString() ?? "No value"))
                .Filter(task =>
                    string.IsNullOrEmpty(nameTemplate) ||
                    task.Name.IndexOf(nameTemplate, StringComparison.OrdinalIgnoreCase) >=
                    0)
                .Transform(task => new DesktopTask
                {
                    Id = task.Id,
                    Name = task.Name,
                    GroupName = ParseGroupName(task.Name),
                    Schedule = cachedService.Schedules.Items
                        .FirstOrDefault(sched => sched.Id == task.ScheduleId)?.Name,

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

            // temp = temp.Where();

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

            if (Shell.Role == ServiceUserRole.Editor)
            {
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
            }

            if (Shell.Role == ServiceUserRole.Viewer)
                Shell.AddVMCommand("View", "View task",
                    "EditTaskCommand", this);

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.CountChanged)
                .Subscribe(_ => RefreshTaskList());

            AllGroupsExpanded = true;
        }

        private async Task Delete()
        {
            if (SelectedTaskInstance != null)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ("Do you want to delete this task instance?")) return;

                cachedService.DeleteInstance(SelectedTaskInstance.Id);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return;
            }

            if (SelectedTask != null)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ($"Do you want to delete task {SelectedTask.Name} and all it's instances?")
                ) return;

                cachedService.DeleteTask(SelectedTask.Id);
                cachedService.RefreshData();
            }
        }
    }
}