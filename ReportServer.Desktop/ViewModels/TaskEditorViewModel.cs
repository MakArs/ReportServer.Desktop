using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace ReportServer.Desktop.ViewModels
{
    public class TaskEditorViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly IShell shell;

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiOperTemplate> Operations { get; set; }
        [Reactive] public ReactiveList<DesktopTaskOper> BindedOpers { get; set; }

        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int? ScheduleId { get; set; }
        [Reactive] public bool HasSchedule { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        [Reactive] public ApiOperTemplate SelectedOperation { get; set; }
        [Reactive] public object SelectedOperationConfig { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand<DesktopTaskOper, Unit> RemoveOperCommand { get; set; }
        public ReactiveCommand<ApiOperTemplate, Unit> AddOperCommand { get; set; }
        public ReactiveCommand OpenCurrentTaskViewCommand { get; set; }

        public TaskEditorViewModel(ICachedService cachedService, IMapper mapper,
                                   IDialogCoordinator dialogCoordinator, IShell shell)
        {
            this.cachedService = cachedService;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;
            this.dialogCoordinator = dialogCoordinator;
            this.shell = shell;

            BindedOpers = new ReactiveList<DesktopTaskOper>();
            Schedules = new ReactiveList<ApiSchedule>();

            RemoveOperCommand = ReactiveCommand.Create<DesktopTaskOper>(to =>
                BindedOpers.Remove(to));

            AddOperCommand = ReactiveCommand.Create<ApiOperTemplate>(op =>
                BindedOpers.Add(new DesktopTaskOper
                {
                    Name = op.Name,
                    TaskId = Id,
                    OperTemplateId = op.Id
                }));

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
                    var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                        "All unsaved changes will be lost. Close window?"
                        , MessageDialogStyle.AffirmativeAndNegative);

                    if (dialogResult != MessageDialogResult.Affirmative)
                        return;
                }

                Close();
            });

            this.ObservableForProperty(s => s.SelectedOperation)
                .Where(sop => sop.Value != null)
                .Subscribe(selop =>
                {
                    var val = selop.Value;

                    var type = cachedService.DataExporters.ContainsKey(val.Type)
                        ? cachedService.DataExporters[val.Type]
                        : cachedService.DataImporters[val.Type];
                    SelectedOperationConfig = JsonConvert
                        .DeserializeObject(val.ConfigTemplate, type);
                });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            shell.AddVMCommand("File", "Save",
                    "SaveChangesCommand", this)
                .SetHotKey(ModifierKeys.Control, Key.S);

            Schedules.PublishCollection(cachedService.Schedules);
            Operations = cachedService.OperTemplates;

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

            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                Id > 0
                    ? "Save these task parameters?"
                    : "Create this task?"
                , MessageDialogStyle.AffirmativeAndNegative);

            if (dialogResult != MessageDialogResult.Affirmative) return;

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