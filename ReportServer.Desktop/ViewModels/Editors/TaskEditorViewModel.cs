using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.ViewModels.General;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels.Editors
{
    public class
        TaskEditorViewModel : ViewModelBase,
            IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        public CachedServiceShell Shell { get; }
        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int? ScheduleId { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool HasSchedule { get; set; }

        private readonly SourceList<DesktopOperation> bindedOpers;
        private readonly SourceList<TaskParameter> taskParameters;
        private readonly SourceList<TaskParameterInfos> taskParameterInfos;
        private readonly SourceList<string> incomingPackages;
        private readonly SourceList<DesktopTaskNameId> tasks;
        private readonly SourceList<DesktopTaskDependence> taskDependencies;
        public ObservableCollectionExtended<DesktopOperation> BindedOpers { get; set; }
        public ObservableCollectionExtended<TaskParameter> TaskParameters { get; set; }
        public ObservableCollectionExtended<TaskParameterInfos> TaskParameterInfos { get; set; }
        public ObservableCollectionExtended<DesktopTaskDependence> TaskDependencies { get; set; }
        public ReadOnlyObservableCollection<ApiSchedule> Schedules { get; set; }
        public ReadOnlyObservableCollection<string> IncomingPackages { get; set; }
        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }
        private readonly SourceList<string> implementationTypes;
        public ReadOnlyObservableCollection<string> ImplementationTypes { get; set; }
        [Reactive] public OperMode Mode { get; set; }
        [Reactive] public string Type { get; set; }
        [Reactive] public DesktopOperation SelectedOperation { get; set; }
        [Reactive] public object SelectedOperationConfig { get; set; }
        [Reactive] public string SelectedOperationName { get; set; }
        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateOperConfigCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenTemplatesListCommand { get; set; }
        public ReactiveCommand<string, Unit> ClipBoardFillCommand { get; set; }
        public ReactiveCommand<DesktopOperation, Unit> RemoveOperationCommand { get; set; }
        public ReactiveCommand<TaskParameter, Unit> RemoveParameterCommand { get; set; }
        public ReactiveCommand<DesktopTaskDependence, Unit> RemoveDependenceCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddOperationCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddParameterCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddDependenceCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenCurrentTaskViewCommand { get; set; }
        public ReactiveCommand<DesktopOperation, Unit> SelectOperationCommand { get; set; }

        public TaskEditorViewModel(ICachedService service, IMapper mapper, IShell shell)
        {
            cachedService = service;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;
            Shell = shell as CachedServiceShell;

            taskParameters = new SourceList<TaskParameter>();
            taskDependencies = new SourceList<DesktopTaskDependence>();
            bindedOpers = new SourceList<DesktopOperation>();
            incomingPackages = new SourceList<string>();
            tasks = new SourceList<DesktopTaskNameId>();
            taskParameterInfos = new SourceList<TaskParameterInfos>();

            DataImporters = cachedService.DataImporters;
            DataExporters = cachedService.DataExporters;
            implementationTypes = new SourceList<string>();

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty, tvm => tvm.IsValid,
                (isd, isv) => isd && isv).Concat(Shell.CanEdit);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(Cancel);

            ClipBoardFillCommand = ReactiveCommand.Create<string>(Clipboard.SetText);

            OpenCurrentTaskViewCommand = ReactiveCommand //todo: make without interface blocking 
                .CreateFromTask(async () =>
                    cachedService.OpenPageInBrowser(
                        await cachedService.GetCurrentTaskViewById(Id)));

            RemoveOperationCommand = ReactiveCommand.Create<DesktopOperation>(to =>
            {
                if (SelectedOperation?.Id == to.Id)
                    ClearSelections();
                bindedOpers.Remove(to);
            }, Shell.CanEdit);

            RemoveParameterCommand = ReactiveCommand
                .Create<TaskParameter>(par => taskParameters.Remove(par), Shell.CanEdit);

            RemoveDependenceCommand = ReactiveCommand
                .Create<DesktopTaskDependence>(dep => taskDependencies.Remove(dep), Shell.CanEdit);

            AddParameterCommand = ReactiveCommand.Create(() => taskParameters.Add(new TaskParameter
            {
                Name = "@RepPar"
            }), Shell.CanEdit);

            AddDependenceCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (tasks.Count == 0)
                {
                    await Shell.ShowMessageAsync("No any existing tasks in service");
                    return;
                }

                tasks
                    .Connect()
                    .Bind(out var cachedTasks)
                    .Subscribe();

                var dependence = new DesktopTaskDependence {Tasks = cachedTasks, TaskId = cachedTasks.First().Id};
                taskDependencies.Add(dependence);
            }, Shell.CanEdit);

            AddOperationCommand = ReactiveCommand.CreateFromTask(AddOperation, Shell.CanEdit);


            CreateOperConfigCommand = ReactiveCommand.CreateFromTask(CreateOperConfig, Shell.CanEdit);

            OpenTemplatesListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedOperationConfig != null && Shell.Role == ServiceUserRole.Editor)
                {
                    if (!await Shell.ShowWarningAffirmativeDialogAsync
                        ("All unsaved operation configuration changes will be lost. Close window?"))
                        return;
                }

                await SelectOperFromTemplates();

            }, Shell.CanEdit);

            SelectOperationCommand = ReactiveCommand.CreateFromTask<DesktopOperation>
                (SelectOperation);

            this.ObservableForProperty(s => s.Mode)
                .Subscribe(mode =>
                {
                    var templates = mode?.Value == OperMode.Exporter
                        ? DataExporters.Select(pair => pair.Key)
                        : DataImporters.Select(pair => pair.Key);

                    implementationTypes.ClearAndAddRange(templates);
                    Type = implementationTypes.Items.FirstOrDefault();
                });

            this.ObservableForProperty(s => s.Type)
                .Where(type => type.Value != null)
                .Subscribe(type =>
                {
                    var operType = Mode == OperMode.Exporter
                        ? DataExporters[type.Value]
                        : DataImporters[type.Value];
                    if (operType == null) return;

                    SelectedOperationConfig = Activator.CreateInstance(operType);
                    mapper.Map(cachedService, SelectedOperationConfig);
                });

            this.WhenAnyValue(tvm => tvm.SelectedOperationConfig)
                .Where(selop => selop != null)
                .Subscribe(conf =>
                    IncomingPackages = incomingPackages.SpawnCollection());
        }

        private async Task Cancel()
        {
            if (IsDirty)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved changes will be lost. Close window?"))
                    return;
            }

            Close();
        }

        private async Task AddOperation()
        {
            if (!await Shell.ShowWarningAffirmativeDialogAsync
                ("Add edited template to task?"))
                return;

            SelectedOperation.Config = JsonConvert.SerializeObject(SelectedOperationConfig);
            SelectedOperation.Name = SelectedOperationName;

            if (SelectedOperation.Id == null)
            {
                SelectedOperation.Id = 0;
                SelectedOperation.TaskId = Id;
                if (!string.IsNullOrEmpty(Type)) SelectedOperation.ImplementationType = Type;
                bindedOpers.Add(SelectedOperation);
            }

            ClearSelections();
        }

        private async Task SelectOperation(DesktopOperation operation)
        {
            if (Shell.Role == ServiceUserRole.Editor && SelectedOperationConfig != null)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved operation configuration changes will be lost. Close window?"))
                    return;
            }

            ClearSelections();

            SelectedOperation = operation;

            SelectedOperationName = SelectedOperation.Name;

            SelectedOperationConfig =
                DeserializeOperationConfigByType(SelectedOperation.ImplementationType, SelectedOperation.Config);
        }

        private void ClearSelections()
        {
            Mode = 0;
            Type = null;
            SelectedOperationName = null;
            SelectedOperation = null;
            SelectedOperationConfig = null; //todo:find the way for risepropertychanged
        }

        private async Task CreateOperConfig()
        {
            if (SelectedOperationConfig != null && Shell.Role == ServiceUserRole.Editor)
            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved operation configuration changes will be lost. Close window?"))
                    return;
            }

            ClearSelections();

            SelectedOperation = new DesktopOperation
            {
                Id = null,
                Name = "New Operation"
            };

            SelectedOperationName = "New Operation";

            Mode = OperMode.Importer;
        }

        private async Task SelectOperFromTemplates()
        {
            var editorOptions = new UiShowChildWindowOptions
            {
                IsModal = true,
                AllowMove = true,
                CanClose = true,
                ShowTitleBar = true,
                Title = "Select operation template",
                Width = 700
            };

            await Shell.ShowChildWindowView<OperTemplatesListView, Unit>(
                new OperTemplatesListRequest
                    {TaskEditor = this},
                editorOptions);
        }

        public void ParseTemplate(ApiOperTemplate templ)
        {
            SelectedOperation = mapper.Map<DesktopOperation>(templ);

            SelectedOperationConfig =
                DeserializeOperationConfigByType(templ.ImplementationType, templ.ConfigTemplate);

            SelectedOperationName = templ.Name;
        }

        public void AddFullTemplate(ApiOperTemplate template)
        {
            var oper = mapper.Map<DesktopOperation>(template);
            oper.TaskId = Id;
            bindedOpers.Add(oper);
        }

        private void UpdateParametersList()
        {
            using (TaskParameters.SuspendNotifications())
            {
                foreach (var dim in TaskParameters)
                {
                    dim.IsDuplicate = false;
                }

                var duplicatgroups = TaskParameters
                    .GroupBy(dim => dim.Name)
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in duplicatgroups)
                {
                    foreach (var dim in group)
                        dim.IsDuplicate = true;
                }
            }
        }

        private object DeserializeOperationConfigByType(string implementationType, string configTemplate)
        {
            var type = cachedService.DataExporters.ContainsKey(implementationType)
                ? DataExporters[implementationType]
                : DataImporters[implementationType];

            return JsonConvert
                .DeserializeObject(configTemplate, type);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (Shell.Role == ServiceUserRole.Editor)
            {
                Shell.AddVMCommand("File", "Save",
                        "SaveChangesCommand", this)
                    .SetHotKey(ModifierKeys.Control, Key.S);

                Shell.AddVMCommand("Edit", "Add operation from existing templates",
                    "OpenTemplatesListCommand", this);

                Shell.AddVMCommand("Edit", "Add new operation",
                    "CreateOperConfigCommand", this);
            }

            tasks.ClearAndAddRange(cachedService.Tasks.Items
                .Select(task => mapper.Map<DesktopTaskNameId>(task)));

            Schedules = cachedService.Schedules.SpawnCollection();

            if (viewRequest is TaskEditorRequest request)
            {
                mapper.Map(request.Task, this);
                HasSchedule = ScheduleId > 0;

                if (request.TaskOpers != null)
                {
                    bindedOpers.ClearAndAddRange(request.TaskOpers.OrderBy(to => to.Number)
                        .Select(to => mapper.Map<DesktopOperation>(to)));
                }

                if (request.Task.DependsOn != null)
                    taskDependencies.ClearAndAddRange(request.Task.DependsOn
                        .Select(dep =>
                        {
                            tasks
                                .Connect()
                                .Bind(out var cachedTasks)
                                .Subscribe();

                            var desktopDep = mapper.Map<DesktopTaskDependence>(dep);
                            desktopDep.Tasks = cachedTasks;

                            return desktopDep;
                        }));

                if (request.DependsOn != null)
                {
                    taskDependencies.ClearAndAddRange(request
                        .DependsOn.OrderBy(dep => dep.TaskId));
                }

                if (!string.IsNullOrEmpty(request.Task.Parameters))
                    taskParameters.ClearAndAddRange(JsonConvert
                        .DeserializeObject<Dictionary<string, object>>(request.Task.Parameters)
                        .Select(pair => new TaskParameter
                        {
                            Name = pair.Key,
                            Value = pair.Value
                        }));

                var test = JsonConvert
                    .DeserializeObject<TaskParameterInfos[]>(request.Task.ParameterInfos);

                if (!string.IsNullOrEmpty(request.Task.ParameterInfos))
                    taskParameterInfos.ClearAndAddRange(
                        JsonConvert
                        .DeserializeObject<TaskParameterInfos[]>(request.Task.ParameterInfos)
                        .Select(item => new TaskParameterInfos
                        {
                            Name = item.Name,
                            Type = item.Type,
                            IsRequired = item.IsRequired,
                            Description = item.Description,
                            DefaultValue = item.DefaultValue
                        }));

                TaskDependencies = new ObservableCollectionExtended<DesktopTaskDependence>();

                taskDependencies.Connect()
                    .Bind(TaskDependencies)
                    .Subscribe(_ => this.RaisePropertyChanged(nameof(TaskDependencies)));

                taskDependencies.Connect()
                    .WhenAnyPropertyChanged("TaskId", "MaxSecondsPassed")
                    .Subscribe(_ => this.RaisePropertyChanged(nameof(TaskDependencies)));

                if (request.ViewId == "Creating new Task")
                {
                    HasSchedule = true;
                    ScheduleId = Schedules.First()?.Id;
                    Name = "New task";
                }

                if (request.ViewId.Contains("copy"))
                {
                    Name = request.ViewId;
                }
            }

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                if (Shell.Role == ServiceUserRole.Viewer || IsDirty || e.PropertyName == "SelectedOperation") return;
                IsDirty = true;
                Title += '*';
            }

            AllErrors.Connect()
                .Subscribe(_ => IsValid = !AllErrors.Items.Any());

            this.ObservableForProperty(s => s.HasSchedule)
                .Subscribe(hassch =>
                    ScheduleId = hassch.Value ? Schedules.FirstOrDefault()?.Id : null);

            taskParameters.Connect() //todo: real-time updating of duplicates indicators
                .WhenAnyPropertyChanged("Name")
                .Subscribe(_ =>
                {
                    foreach (var dim in TaskParameters)
                        dim.IsDuplicate = false;

                    var duplicatgroups = TaskParameters
                        .GroupBy(dim => dim.Name)
                        .Where(g => g.Count() > 1)
                        .ToList();

                    foreach (var group in duplicatgroups)
                    {
                        foreach (var dim in group)
                        {
                            dim.IsDuplicate = true;
                            dim.HasErrors = true;
                        }
                    }

                    this.RaisePropertyChanged(nameof(TaskParameters));
                });

            taskParameters.Connect()
                .WhenAnyPropertyChanged("Value")
                .Subscribe(_ => this.RaisePropertyChanged(nameof(TaskParameters)));

            taskParameters.Connect()
                .WhenAnyPropertyChanged("HasErrors")
                .Subscribe(_ =>
                {
                    IsValid = !AllErrors.Items.Any() &&
                              !TaskParameters.Any(param => param.HasErrors);
                });

            TaskParameters = new ObservableCollectionExtended<TaskParameter>();

            taskParameters.Connect()
                .Bind(TaskParameters)
                .Skip(1)
                .Subscribe(_ =>
                {
                    UpdateParametersList();
                    this.RaisePropertyChanged(nameof(TaskParameters));
                });

            TaskParameterInfos = new ObservableCollectionExtended<TaskParameterInfos>();

            taskParameterInfos.Connect().Bind(TaskParameterInfos).Subscribe(_ => this.RaisePropertyChanged(nameof(taskParameterInfos)));

            ImplementationTypes = implementationTypes.SpawnCollection();

            BindedOpers = new ObservableCollectionExtended<DesktopOperation>();

            bindedOpers.Connect()
                .Bind(BindedOpers)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(bindedOpers)));



            bindedOpers.Connect()
                .Subscribe(_ =>
                {
                    var packages = bindedOpers.Items.Where(oper => DataImporters.ContainsKey(oper.ImplementationType))
                        .Select(oper =>
                            (DeserializeOperationConfigByType(oper.ImplementationType, oper.Config) as
                                IPackagedImporterConfig)
                            ?.PackageName).Where(name => !string.IsNullOrEmpty(name)).Distinct().ToList();

                    incomingPackages.ClearAndAddRange(packages);
                    this.RaisePropertyChanged(nameof(bindedOpers));
                });

            PropertyChanged += Changed;
        } //init vm

        private async Task<bool> CheckCanSave()
        {
            if (!IsValid || !IsDirty) return false;

            if (!await Shell.ShowWarningAffirmativeDialogAsync(Id > 0
                ? "Save these task settings?"
                : "Create this task?"))
                return false;


            cachedService.RefreshTasks();

            var deletedTasksList = new List<long>();

            foreach (var dependence in taskDependencies.Items)
                if (cachedService.Tasks.Items.All(task => task.Id != dependence.TaskId))
                    deletedTasksList.Add(dependence.TaskId);

            if (deletedTasksList.Count > 0)
            {
                await Shell.ShowMessageAsync($"Tasks not longer exists: {string.Join(", ", deletedTasksList)}");

                taskDependencies.RemoveMany(taskDependencies.Items
                    .Where(dep => deletedTasksList.Contains(dep.TaskId)));

                return false;
            }

            return true;
        }

        private async Task Save()
        {
            if (!await CheckCanSave())
                return;

            var opersToSave = BindedOpers.ToList();

            foreach (var oper in opersToSave)
                oper.Number = opersToSave.IndexOf(oper) + 1;

            var editedTask = new ApiTask();

            mapper.Map(this, editedTask);

            editedTask.DependsOn = taskDependencies.Count == 0
                ? null
                : taskDependencies.Items.Select(dep => mapper.Map<ApiTaskDependence>(dep))
                    .ToArray();

            editedTask.BindedOpers =
                opersToSave.Select(oper => mapper.Map<ApiOperation>(oper)).ToArray();

            cachedService.CreateOrUpdateTask(editedTask);

            Close();
            cachedService.RefreshData();
        } //saving to base
    }
}
