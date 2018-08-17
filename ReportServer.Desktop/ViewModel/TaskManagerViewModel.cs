using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IReportService reportService;
        private readonly IMapper mapper;
        private readonly IDistinctShell shell;

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

        public TaskManagerViewModel(IReportService reportService, IMapper mapper, IShell shell)
        {
            this.reportService = reportService;
            this.mapper = mapper;
            this.shell = shell as IDistinctShell;
            SelectedTaskInstanceCompacts = new ReactiveList<DesktopInstanceCompact>();

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
                    SelectedTaskInstanceCompacts.Clear();
                    SelectedTask = null;
                });

            this.WhenAnyValue(s => s.SelectedTask)
                .Where(x => x != null)
                .Subscribe(x => LoadInstanceCompactsByTaskId(x.Id));

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
            var instanceList = reportService.GetInstancesByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(mapper.Map<DesktopInstanceCompact>(instance));
        }

        private void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = mapper
                .Map<DesktopInstance>(reportService.GetFullInstanceById(id));
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
            Schedules = reportService.Schedules;
            RecepientGroups = reportService.RecepientGroups;
            Reports = reportService.Reports;
            Tasks = reportService.Tasks;
        }
    }
}