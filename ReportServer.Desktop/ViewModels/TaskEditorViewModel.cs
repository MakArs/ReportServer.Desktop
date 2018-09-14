using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
    public class TaskEditorViewModel : ViewModelBase, IInitializableViewModel, ISaveableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiOper> Operations { get; set; }
        [Reactive] public ReactiveList<DesktopTaskOper> BindedOpers { get; set; } 

        public int Id { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int? ScheduleId { get; set; }
        [Reactive] public bool HasSchedule { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        [Reactive] public ApiOper SelectedOperation { get; set; }
        [Reactive] public object SelectedOperationConfig { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand<DesktopTaskOper,Unit> RemoveOperCommand { get; set; }
        public ReactiveCommand<ApiOper, Unit> AddOperCommand { get; set; }
        public ReactiveCommand OpenCurrentTaskViewCommand { get; set; }

        public TaskEditorViewModel(ICachedService cachedService, IMapper mapper,
                                   IDialogCoordinator dialogCoordinator)
        {
            this.cachedService = cachedService;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;
            this.dialogCoordinator = dialogCoordinator;

            BindedOpers = new ReactiveList<DesktopTaskOper>();

            RemoveOperCommand = ReactiveCommand.Create<DesktopTaskOper>(to=>
                BindedOpers.Remove(to));

            AddOperCommand = ReactiveCommand.Create<ApiOper>(op =>
                BindedOpers.Add(new DesktopTaskOper
                {
                    Name = op.Name,
                    TaskId = Id,
                    OperId = op.Id
                }));

            OpenCurrentTaskViewCommand = ReactiveCommand
                .CreateFromTask(async () =>
                {
                    var str = await cachedService.GetCurrentTaskViewById(Id);
                    OpenPageInBrowser(str);
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
                    var type = val.Type == "Exporter"
                        ? cachedService.DataExporters[val.Name]
                        : cachedService.DataImporters[val.Name];
                    SelectedOperationConfig = JsonConvert
                        .DeserializeObject(val.Config, type);
                });

            this.WhenAnyObservable(s => s.AllErrors.Changed)
                .Subscribe(_ => IsValid = !AllErrors.Any());
        }

        private void OpenPageInBrowser(string htmlPage)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}testreport.html";
            using (FileStream fstr = new FileStream(path, FileMode.Create))
            {
                byte[] bytePage = System.Text.Encoding.UTF8.GetBytes(htmlPage);
                fstr.Write(bytePage, 0, bytePage.Length);
            }

            System.Diagnostics.Process.Start(path);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Schedules = cachedService.Schedules;
            Operations = cachedService.Operations;

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
                            OperId = to.OperId,
                            TaskId = to.TaskId,
                            Name = cachedService.Operations
                                .First(oper => oper.Id == to.OperId).Name
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
                .Subscribe(_=>this.RaisePropertyChanged());

            IsDirty = false;
        }

        public async Task Save()
        {
            if (!IsValid) return;

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