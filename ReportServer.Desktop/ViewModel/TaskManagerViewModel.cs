using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskManagerViewModel : ViewModelBase, IInitializableViewModel, IDeleteableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IMapper mapper;
        private readonly DistinctShell shell;
        private readonly IDialogCoordinator dialogCoordinator; 

        public ReactiveList<DesktopFullTask> Tasks { get; set; }
        public ReactiveList<DesktopInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopReport> Reports { get; set; }

        [Reactive] public DesktopFullTask SelectedTask { get; set; }
        [Reactive] public DesktopInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public DesktopInstance SelectedInstance { get; set; }

        public ReactiveCommand OpenPage { get; set; }
        public ReactiveCommand<DesktopFullTask, Unit> EditTaskCommand { get; set; }

        public TaskManagerViewModel(ICachedService cachedService, IMapper mapper, IShell shell)
        {
            this.cachedService = cachedService;
            this.mapper = mapper;
            this.shell = shell as DistinctShell;
            SelectedTaskInstanceCompacts = new ReactiveList<DesktopInstanceCompact>();
            dialogCoordinator = DialogCoordinator.Instance;

            IObservable<bool> canOpenInstancePage = this
                .WhenAnyValue(t => t.SelectedInstance,
                    si => !string.IsNullOrEmpty(si?.ViewData));

            OpenPage = ReactiveCommand.Create<string>
                (OpenPageInBrowser, canOpenInstancePage);

            EditTaskCommand = ReactiveCommand.Create<DesktopFullTask>(task =>
            {
                var name = $"Task {task.Id} editor";
                this.shell.ShowDistinctView<TaskEditorView>(name,
                    new TaskEditorRequest {Task = task},
                    new UiShowOptions {Title = name});
            });

            this.WhenAnyObservable(s => s.Tasks.Changed) // todo: add and test when element changed
                .Subscribe(x =>
                {
                    SelectedTask = null;
                    SelectedTaskInstanceCompacts.Clear();
                });

            this.WhenAnyValue(s => s.SelectedTask)
                .Where(x => x != null)
                .Subscribe(x => { LoadInstanceCompactsByTaskId(x.Id); });

            this.WhenAnyValue(s => s.SelectedInstanceCompact)
                .Subscribe(x =>
                {
                    if (x == null)
                        SelectedInstance = null;
                    else
                        LoadSelectedInstanceById(x.Id);
                });
        }

        private void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = cachedService.GetInstancesByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(mapper.Map<DesktopInstanceCompact>(instance));
        }

        private void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = mapper
                .Map<DesktopInstance>(cachedService.GetFullInstanceById(id));
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
            RecepientGroups = cachedService.RecepientGroups;
            Reports = cachedService.Reports;
            Tasks = cachedService.Tasks;
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
            if (SelectedInstance != null)

            {
               if(!await ShowWarningAffirmativeDialog
                   ("Вы действительно хотите удалить информацию о выполненной задаче?")) return;

                cachedService.DeleteInstance(SelectedInstance.Id);
                LoadInstanceCompactsByTaskId(SelectedTask.Id);
                return;
            }

            if (SelectedTask != null)
            {
                if (!await ShowWarningAffirmativeDialog
                    ("Вы действительно хотите удалить задачу и всю информацию о ней?")) return;

                cachedService.DeleteTask(SelectedTask.Id);
                cachedService.RefreshData();
            }
        }

    }
}