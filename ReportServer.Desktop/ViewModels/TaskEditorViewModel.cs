using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
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
    public class TaskEditorViewModel : ViewModelBase, IInitializableViewModel //todo:find some optimal interception between taskeditor and opereditor vms
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly CachedServiceShell shell;

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiOperTemplate> Operations { get; set; }
        [Reactive] public ReactiveList<DesktopTaskOper> BindedOpers { get; set; }

        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int? ScheduleId { get; set; }
        [Reactive] public bool HasSchedule { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool TemplatesListOpened { get; set; }
        [Reactive] public string OperationsSearchString { get; set; }

        private Dictionary<string, Type> DataImporters { get; set; }
        private Dictionary<string, Type> DataExporters { get; set; }
        public ReactiveList<string> OperTemplates { get; set; }
        [Reactive] public OperMode Mode { get; set; }
        [Reactive] public string Type { get; set; }
        [Reactive] public ApiOperTemplate SelectedOperation { get; set; }
        [Reactive] public object SelectedOperationConfig { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand CreateOperConfigCommand { get; set; }
        public ReactiveCommand OpenTemplatesListCommand { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> SelectTemplateCommand { get; set; }
        public ReactiveCommand<DesktopTaskOper, Unit> RemoveTaskOperCommand { get; set; }
        public ReactiveCommand AddTaskOperCommand { get; set; }
        public ReactiveCommand OpenCurrentTaskViewCommand { get; set; }

        public TaskEditorViewModel(ICachedService service, IMapper mapper, IShell shell)
        {
            cachedService = service;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;
            this.shell = shell as CachedServiceShell;

            BindedOpers = new ReactiveList<DesktopTaskOper>();
            Schedules = new ReactiveList<ApiSchedule>();
            Operations = new ReactiveList<ApiOperTemplate>();
            DataImporters = cachedService.DataImporters;
            DataExporters = cachedService.DataExporters;
            OperTemplates=new ReactiveList<string>();

            this.ObservableForProperty(s => s.Mode)
                .Subscribe(mode =>
                {
                    var templates = mode.Value == OperMode.Exporter
                        ? DataExporters.Select(pair => pair.Key)
                        : DataImporters.Select(pair => pair.Key);

                    OperTemplates.PublishCollection(templates);
                    Type = OperTemplates.First();
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

            RemoveTaskOperCommand = ReactiveCommand.Create<DesktopTaskOper>(to =>
                BindedOpers.Remove(to));

            AddTaskOperCommand = ReactiveCommand.Create(() =>
            {
                BindedOpers.Add(new DesktopTaskOper
                {
                    Name = SelectedOperation.Name,
                    TaskId = Id,
                    OperTemplateId = SelectedOperation.Id,
                    Config = JsonConvert.SerializeObject(SelectedOperationConfig)
                });
                SelectedOperation = null;
                SelectedOperationConfig = null;
            });

            OpenCurrentTaskViewCommand = ReactiveCommand
                .CreateFromTask(async () =>
                {
                    var str = await cachedService.GetCurrentTaskViewById(Id);
                    cachedService.OpenPageInBrowser(str);
                });

            var canSave = this.WhenAnyValue(tvm => tvm.IsDirty,
                isd => isd == true);

            SaveChangesCommand = ReactiveCommand.CreateFromTask(async () => await Save(),
                canSave);

            CancelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsDirty)
                {
                    if (!await this.shell.ShowWarningAffirmativeDialogAsync
                        ("All unsaved changes will be lost. Close window?"))
                        return;
                }

                Close();
            });

            CreateOperConfigCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedOperation != null)
                {
                    if (!await this.shell.ShowWarningAffirmativeDialogAsync
                        ("All unsaved operation configuration changes will be lost. Close window?"))

                        return;
                }

                SelectedOperation = new ApiOperTemplate
                {
                    Id = 0,
                    Name = "New Operation",
                    Type = cachedService.DataImporters.First().Key
                };
            });

            OpenTemplatesListCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedOperation != null)
                {
                    if (!await this.shell.ShowWarningAffirmativeDialogAsync
                        ("All unsaved operation configuration changes will be lost. Close window?"))

                        return;

                    SelectedOperation = null;
                    SelectedOperationConfig = null;
                }

                TemplatesListOpened = true;
            });

            SelectTemplateCommand = ReactiveCommand.Create<ApiOperTemplate>(templ =>
            {
                SelectedOperation = templ;

                var type = cachedService.DataExporters.ContainsKey(templ.Type)
                    ? cachedService.DataExporters[templ.Type]
                    : cachedService.DataImporters[templ.Type];

                SelectedOperationConfig = JsonConvert
                    .DeserializeObject(templ.ConfigTemplate, type);

                OperationsSearchString = "";

                TemplatesListOpened = false;
            });

            this.ObservableForProperty(s => s.OperationsSearchString)
                .Subscribe(sstr =>
                {
                    List<ApiOperTemplate> opers;
                    lock (this)
                        opers = cachedService.OperTemplates.Where(oper =>
                                oper.Name.IndexOf(sstr.Value, StringComparison.OrdinalIgnoreCase) >=
                                0)
                            .ToList();

                    Operations.PublishCollection(opers);
                });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());

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

            Schedules.PublishCollection(cachedService.Schedules);
            Operations.PublishCollection(cachedService.OperTemplates);

            if (viewRequest is TaskEditorRequest request)
            {
                mapper.Map(request.Task, this);
                HasSchedule = ScheduleId > 0;

                BindedOpers.ChangeTrackingEnabled = true;

                if (request.TaskOpers != null)
                    BindedOpers.PublishCollection(request.TaskOpers.OrderBy(to => to.Number)
                        .Select(to => new DesktopTaskOper
                        {
                            Id = to.Id,
                            Number = to.Number,
                            IsDefault = to.IsDefault,
                            OperTemplateId = to.OperTemplateId,
                            TaskId = to.TaskId,
                            Name = cachedService.OperTemplates
                                .First(oper => oper.Id == to.OperTemplateId).Name
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

            PropertyChanged += Changed;

            this.ObservableForProperty(s => s.HasSchedule)
                .Subscribe(hassch =>
                    ScheduleId = hassch.Value ? Schedules.FirstOrDefault()?.Id : null);

            this.WhenAnyObservable(tevm => tevm.BindedOpers.Changed)
                .Subscribe(_ => this.RaisePropertyChanged());
        }

        private async Task Save()
        {
            if (!IsValid || !IsDirty) return;

            if (!await shell.ShowWarningAffirmativeDialogAsync(Id > 0
                ? "Save these task parameters?"
                : "Create this task?"))
                return;

            foreach (var oper in BindedOpers)
                oper.Number = BindedOpers.IndexOf(oper) + 1;

            var editedTask = new ApiTask();

            mapper.Map(this, editedTask);

            cachedService.CreateOrUpdateTask(editedTask);

            Close();
            cachedService.RefreshData();
        }
    }
}