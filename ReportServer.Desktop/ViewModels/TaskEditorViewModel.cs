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
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;


namespace ReportServer.Desktop.ViewModels
{
    public class
        TaskEditorViewModel : ViewModelBase,
            IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly CachedServiceShell shell;

        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int? ScheduleId { get; set; }

        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool HasSchedule { get; set; }
        [Reactive] public bool TemplatesListOpened { get; set; }
        [Reactive] public string OperationsSearchString { get; set; }

        private readonly SourceList<DesktopOperation> bindedOpers;
        private readonly SourceList<TaskParameter> taskParameters;
        public ObservableCollectionExtended<TaskParameter> TaskParameters { get; set; }
        public ObservableCollectionExtended<DesktopOperation> BindedOpers { get; set; }
        public ReadOnlyObservableCollection<ApiSchedule> Schedules { get; set; }
        public ObservableCollectionExtended<ApiOperTemplate> OperTemplates { get; set; }
        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }
        private readonly SourceList<string> implementationTypes;
        public ReadOnlyObservableCollection<string> ImplementationTypes { get; set; }
        [Reactive] public OperMode Mode { get; set; }
        [Reactive] public string Type { get; set; }
        [Reactive] public DesktopOperation SelectedOperation { get; set; }
        [Reactive] public ApiOperTemplate SelectedTemplate { get; set; }
        [Reactive] public object SelectedOperationConfig { get; set; }
        [Reactive] public string SelectedOperationName { get; set; }

        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> SelectTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CloseTemplatesListCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateOperConfigCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenTemplatesListCommand { get; set; }
        public ReactiveCommand<string, Unit> ClipBoardFillCommand { get; set; }
        public ReactiveCommand<DesktopOperation, Unit> RemoveOperationCommand { get; set; }
        public ReactiveCommand<TaskParameter, Unit> RemoveParameterCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddOperationCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddParameterCommand { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> AddFullTemplateCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenCurrentTaskViewCommand { get; set; }
        public ReactiveCommand<DesktopOperation, Unit> SelectOperationCommand { get; set; }

        public TaskEditorViewModel(ICachedService service, IMapper mapper, IShell shell)
        {
            cachedService = service;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;
            this.shell = shell as CachedServiceShell;

            taskParameters = new SourceList<TaskParameter>();

            bindedOpers = new SourceList<DesktopOperation>();
            DataImporters = cachedService.DataImporters;
            DataExporters = cachedService.DataExporters;
            implementationTypes = new SourceList<string>();

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty, tvm => tvm.IsValid,
                (isd, isv) => isd && isv);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(Cancel);

            ClipBoardFillCommand = ReactiveCommand.Create<string>(Clipboard.SetText);

            OpenCurrentTaskViewCommand = ReactiveCommand
                .CreateFromTask(async () =>
                    cachedService.OpenPageInBrowser(
                        await cachedService.GetCurrentTaskViewById(Id)));

            RemoveOperationCommand = ReactiveCommand.Create<DesktopOperation>(to =>
            {
                if (SelectedOperation?.Id == to.Id)
                    ClearSelections();
                bindedOpers.Remove(to);
            });

            RemoveParameterCommand = ReactiveCommand
                .Create<TaskParameter>(par => taskParameters.Remove(par));

            AddParameterCommand = ReactiveCommand.Create(() => taskParameters.Add(new TaskParameter
            {
                Name = "@RepPar"
            }));

            AddFullTemplateCommand = ReactiveCommand.Create<ApiOperTemplate>(AddFullTemplate);

            AddOperationCommand = ReactiveCommand.CreateFromTask(AddOperation);

            CreateOperConfigCommand = ReactiveCommand.CreateFromTask(CreateOperConfig);

            OpenTemplatesListCommand = ReactiveCommand.CreateFromTask(OpenTemplatesList);

            CloseTemplatesListCommand = ReactiveCommand.Create(() => { TemplatesListOpened = false; });

            SelectTemplateCommand = ReactiveCommand.Create<ApiOperTemplate>(SelectTemplate);

            SelectOperationCommand = ReactiveCommand.CreateFromTask<DesktopOperation>
                (SelectOperation);

            this.ObservableForProperty(s => s.OperationsSearchString)
                .Subscribe(sstr =>
                {
                    OperTemplates.Clear();

                    cachedService.OperTemplates.Connect().Filter(oper =>
                            oper.Name.IndexOf(sstr.Value, StringComparison.OrdinalIgnoreCase) >=
                            0)
                        .Bind(OperTemplates).Subscribe();
                });

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
        }

        private async Task Cancel()
        {
            if (IsDirty)
            {
                if (!await shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved changes will be lost. Close window?"))
                    return;
            }

            Close();
        }

        private async Task AddOperation()
        {
            if (!await shell.ShowWarningAffirmativeDialogAsync
                ("Add edited template to task?"))
                return;

            SelectedOperation.Config = JsonConvert.SerializeObject(SelectedOperationConfig);
            SelectedOperation.Name = SelectedOperationName;

            if (SelectedOperation.Id == null)
            {
                /*if u already added operation to binded list,
                 next time redacting only config will be changed;
                 */
                SelectedOperation.Id = 0;
                SelectedOperation.TaskId = Id;
                if (!string.IsNullOrEmpty(Type)) SelectedOperation.ImplementationType = Type;
                bindedOpers.Add(SelectedOperation);
            }

            ClearSelections();
        }

        private void AddFullTemplate(ApiOperTemplate template)
        {
            var oper = mapper.Map<DesktopOperation>(template);
            oper.TaskId = Id;
            bindedOpers.Add(oper);
        }

        private async Task SelectOperation(DesktopOperation operation)
        {
            if (SelectedOperationConfig != null)
            {
                if (!await shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved operation configuration changes will be lost. Close window?"))
                    return;
            }

            ClearSelections();

            SelectedOperation = operation;

            var type = cachedService.DataExporters.ContainsKey(SelectedOperation.ImplementationType)
                ? cachedService.DataExporters[SelectedOperation.ImplementationType]
                : cachedService.DataImporters[SelectedOperation.ImplementationType];

            SelectedOperationName = SelectedOperation.Name;

            SelectedOperationConfig = JsonConvert
                .DeserializeObject(SelectedOperation.Config, type);
        }

        private void ClearSelections()
        {
            if (TemplatesListOpened)
                TemplatesListOpened = false;
            Mode = 0;
            Type = null;
            SelectedOperationName = null;
            SelectedTemplate = null;
            SelectedOperation = null;
            SelectedOperationConfig = null; //todo:find the way for risepropertychanged
        }

        private async Task CreateOperConfig()
        {
            if (SelectedOperationConfig != null)
            {
                if (!await shell.ShowWarningAffirmativeDialogAsync
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

        private async Task OpenTemplatesList()
        {
            if (SelectedOperationConfig != null)
            {
                if (!await shell.ShowWarningAffirmativeDialogAsync
                    ("All unsaved operation configuration changes will be lost. Close window?"))

                    return;
            }

            ClearSelections();
            TemplatesListOpened = true;
        }

        private void SelectTemplate(ApiOperTemplate templ)
        {
            SelectedOperation = mapper.Map<DesktopOperation>(templ);

            var type = cachedService.DataExporters.ContainsKey(SelectedOperation.ImplementationType)
                ? cachedService.DataExporters[SelectedOperation.ImplementationType]
                : cachedService.DataImporters[SelectedOperation.ImplementationType];

            SelectedOperationConfig = JsonConvert
                .DeserializeObject(templ.ConfigTemplate, type);

            SelectedOperationName = templ.Name;

            OperationsSearchString = "";

            TemplatesListOpened = false;
        }

        public void Initialize(ViewRequest viewRequest)
        {
            shell.AddVMCommand("File", "Save",
                    "SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            shell.AddVMCommand("Edit", "Add operation from existing templates",
                "OpenTemplatesListCommand", this);

            shell.AddVMCommand("Edit", "Add new operation",
                "CreateOperConfigCommand", this);

            Schedules = cachedService.Schedules.SpawnCollection();

            OperTemplates = new ObservableCollectionExtended<ApiOperTemplate>();

            cachedService.OperTemplates.Connect()
                .Bind(OperTemplates).Subscribe();

            if (viewRequest is TaskEditorRequest request)
            {
                mapper.Map(request.Task, this);
                HasSchedule = ScheduleId > 0;

                if (request.TaskOpers != null)
                    bindedOpers.ClearAndAddRange(request.TaskOpers.OrderBy(to => to.Number)
                        .Select(to => mapper.Map<DesktopOperation>(to)));

                if (!string.IsNullOrEmpty(request.Task.Parameters))
                    taskParameters.ClearAndAddRange(JsonConvert
                        .DeserializeObject<Dictionary<string, object>>(request.Task.Parameters)
                        .Select(pair => new TaskParameter
                        {
                            Name = pair.Key,
                            Value = pair.Value
                        }));
            }

            if (Id == 0)
            {
                HasSchedule = true;
                ScheduleId = Schedules.First()?.Id;
                Name = "New task";
            }

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                if (IsDirty || e.PropertyName == "SelectedOperation") return;
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

            ImplementationTypes = implementationTypes.SpawnCollection();

            BindedOpers = new ObservableCollectionExtended<DesktopOperation>();

            bindedOpers.Connect()
                .Bind(BindedOpers)
                .Subscribe();

            PropertyChanged += Changed;
        } //init vm

        private async Task Save()
        {
            if (!IsValid || !IsDirty) return;

            if (!await shell.ShowWarningAffirmativeDialogAsync(Id > 0
                ? "Save these task settings?"
                : "Create this task?"))
                return;

            var opersToSave = BindedOpers.ToList();

            foreach (var oper in opersToSave)
                oper.Number = opersToSave.IndexOf(oper) + 1;

            var editedTask = new ApiTask();

            mapper.Map(this, editedTask);

            editedTask.BindedOpers =
                opersToSave.Select(oper => mapper.Map<ApiOperation>(oper)).ToArray();

            cachedService.CreateOrUpdateTask(editedTask);

            Close();
            cachedService.RefreshData();
        } //save to base

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
    }
}