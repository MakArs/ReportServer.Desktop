﻿using System;
using System.Linq;
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
        private readonly ProtoPackageBuilder packageBuilder;
        public CachedServiceShell Shell { get; }

        public ReactiveList<DesktopTask> Tasks { get; set; }
        public ReactiveList<DesktopTaskInstance> SelectedTaskInstances { get; set; }
        public ReactiveList<DesktopOperInstance> OperInstances { get; set; }

        [Reactive] public DesktopTask SelectedTask { get; set; }
        [Reactive] public DesktopTaskInstance SelectedTaskInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedOperInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedInstanceData { get; set; }

        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand EditTaskCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public TaskManagerViewModel(ICachedService cachedService, IMapper mapper, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.mapper = mapper;
            Shell = shell as CachedServiceShell;
            packageBuilder=new ProtoPackageBuilder();

            Tasks = new ReactiveList<DesktopTask>();
            SelectedTaskInstances = new ReactiveList<DesktopTaskInstance>();
            OperInstances = new ReactiveList<DesktopOperInstance>();

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
                            .Tasks.FirstOrDefault(task => task.Id == id),
                        TaskOpers = cachedService.Operations.Where(to => to.TaskId == id).ToList()
                    },
                    new UiShowOptions {Title = name});
            });

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete());

            this.WhenAnyObservable(s => s.Tasks.Changed)
                .Subscribe(x =>
                {
                    SelectedTask = null;
                    SelectedTaskInstances.Clear();
                    OperInstances.Clear();
                });

            this.WhenAnyValue(s => s.SelectedTask)
                .Where(x => x != null)
                .Subscribe(x => LoadInstanceCompactsByTaskId(x.Id));

            this.WhenAnyValue(s => s.SelectedTaskInstance)
                .Subscribe(x =>
                {
                    if (x == null)
                        OperInstances.Clear();
                    else
                    {
                        OperInstances.PublishCollection(
                            cachedService.GetOperInstancesByTaskInstanceId(x.Id)
                                .Select(mapper.Map<DesktopOperInstance>));
                    }
                });

            this.WhenAnyValue(s => s.SelectedOperInstance)
                .Subscribe(x =>
                {
                    //.ForMember("DataSet",
                    //    opt => opt.MapFrom(s =>
                    //        JsonConvert.SerializeObject(OperationPackage.Parser.ParseFrom(s.DataSet))))

                    var data = new DesktopOperInstance();

                    if (x != null)
                    {
                        var fullInstance = cachedService
                            .GetFullOperInstanceById(SelectedOperInstance.Id);

                        data = mapper.Map<DesktopOperInstance>(fullInstance);

                        if(fullInstance.DataSet!=null)
                        data.DataSet = JsonConvert.SerializeObject(packageBuilder.GetPackageValues(
                            OperationPackage.Parser.ParseFrom(fullInstance.DataSet)));
                    }

                    //if (x != null)
                    //{
                    //    var fi;
                    //    data = mapper.Map<DesktopOperInstance>();
                    //    data.DataSet = OperationPackage.Parser.ParseFrom(s.DataSet));
                    //}

                    SelectedInstanceData = x == null
                        ? null
                        : data;
                });
        }

        private void LoadInstanceCompactsByTaskId(int taskId)
        {
            SelectedTaskInstances
                .PublishCollection(cachedService.GetInstancesByTaskId(taskId)
                    .Select(ti => mapper.Map<DesktopTaskInstance>(ti)));
        }

        private void RefreshTaskList()
        {
            Tasks.PublishCollection(cachedService.Tasks.Select(task => new DesktopTask
            {
                Id = task.Id,
                Name = task.Name,
                Schedule = cachedService.Schedules
                    .FirstOrDefault(sched => sched.Id == task.ScheduleId)?.Schedule,

                Operations = string.Join("=>", cachedService.Operations
                    .Where(oper => oper.TaskId == task.Id)
                    .Select(oper => new
                    {
                        oper.Number,
                        oper.Name
                    })
                    .OrderBy(pair => pair.Number)
                    .Select(pair => pair.Name)
                    .ToList())
            }));
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

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.Changed)
                .Subscribe(_ => RefreshTaskList());

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.ItemChanged)
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