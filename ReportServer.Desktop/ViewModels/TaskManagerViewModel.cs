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
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class TaskManagerViewModel : ViewModelBase, IInitializableViewModel, IDeleteableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        public DistinctShell Shell { get; }
        private readonly IDialogCoordinator dialogCoordinator;

        public ReactiveList<DesktopTask> Tasks { get; set; }
        public ReactiveList<DesktopTaskInstance> SelectedTaskInstances { get; set; }
        public ReactiveList<DesktopOperInstance> OperInstances { get; set; }

        [Reactive] public DesktopTask SelectedTask { get; set; }
        [Reactive] public DesktopTaskInstance SelectedTaskInstance { get; set; }
        [Reactive] public DesktopOperInstance SelectedOperInstance { get; set; }
        [Reactive] public ApiOperInstance SelectedInstanceData { get; set; }

        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand<int?, Unit> EditTaskCommand { get; set; }

        public TaskManagerViewModel(ICachedService cachedService, IMapper mapper, IShell shell,
                                    IDialogCoordinator dialogCoordinator)
        {
            this.cachedService = cachedService;
            this.mapper = mapper;
            Shell = shell as DistinctShell;
            this.dialogCoordinator = dialogCoordinator;

            Tasks = new ReactiveList<DesktopTask>();
            SelectedTaskInstances = new ReactiveList<DesktopTaskInstance>();
            OperInstances = new ReactiveList<DesktopOperInstance>();

            IObservable<bool> canOpenInstancePage = this //todo:some check for is viewdataset?"
                .WhenAnyValue(t => t.SelectedInstanceData,
                    si => !string.IsNullOrEmpty(si?.DataSet));

            OpenPage = ReactiveCommand.Create<string>
                (OpenPageInBrowser, canOpenInstancePage);

            EditTaskCommand = ReactiveCommand.Create<int?>(id =>
            {
                if (id == null) return;
                var name = $"Task {id} editor";
                Shell.ShowDistinctView<TaskEditorView>(name,
                    new TaskEditorRequest
                    {
                        Task = cachedService
                            .Tasks.FirstOrDefault(task => task.Id == id),
                        TaskOpers = cachedService.TaskOpers.Where(to => to.TaskId == id).ToList()
                    },
                    new UiShowOptions {Title = name});
            });

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

                        foreach (var operinst in OperInstances)
                            operinst.OperName = cachedService.Operations
                                .First(op => op.Id == operinst.OperId).Name;
                    }
                });

            this.WhenAnyValue(s => s.SelectedOperInstance)
                .Subscribe(x =>
                {
                    if (x == null)
                        SelectedInstanceData = null;
                    else
                        SelectedInstanceData =
                            cachedService.GetFullOperInstanceById(SelectedOperInstance.Id);
                });
        }

        private void LoadInstanceCompactsByTaskId(int taskId)
        {
            SelectedTaskInstances
                .PublishCollection(cachedService.GetInstancesByTaskId(taskId)
                    .Select(ti => mapper.Map<DesktopTaskInstance>(ti)));
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

        private void RefreshTaskList()
        {
            Tasks.PublishCollection(cachedService.Tasks.Select(task => new DesktopTask
            {
                Id = task.Id,
                Name = task.Name,
                Schedule = cachedService.Schedules
                    .FirstOrDefault(sched => sched.Id == task.ScheduleId)?.Schedule,

                Operations = string.Join("=>", cachedService.TaskOpers
                    .Where(taskOper => taskOper.TaskId == task.Id)
                    .Select(taskOper => new
                    {
                        taskOper.Number,
                        cachedService.Operations.First(oper => oper.Id == taskOper.OperId).Name
                    })
                    .OrderBy(pair => pair.Number)
                    .Select(pair => pair.Name)
                    .ToList())
            }));
        }

        public void Initialize(ViewRequest viewRequest)
        {
            RefreshTaskList();

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.Changed)
                .Subscribe(_ => RefreshTaskList());

            this.WhenAnyObservable(tmvm => tmvm.cachedService.Tasks.ItemChanged)
                .Subscribe(_ => RefreshTaskList());
        }

        private async Task<bool> ShowWarningAffirmativeDialog(string question)
        {
            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                question
                , MessageDialogStyle.AffirmativeAndNegative);
            return dialogResult == MessageDialogResult.Affirmative;
        }

        public async Task Delete()
        {
            if (SelectedOperInstance != null)

            {
                if (!await ShowWarningAffirmativeDialog
                    ("Do you really want to delete this task instance?")) return;

                cachedService.DeleteInstance(SelectedOperInstance.Id);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return;
            }

            if (SelectedTask != null)
            {
                if (!await ShowWarningAffirmativeDialog
                    ("Do you really want to delete this task and all it's instances?")) return;

                cachedService.DeleteTask(SelectedTask.Id);
                cachedService.RefreshData();
            }
        }
    }
}