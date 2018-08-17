using System;
using System.IO;
using System.Reactive.Linq;
using AutoMapper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskManagerViewModel : ReactiveObject, IViewModel, IInitializableViewModel
    {
        private readonly IReportService reportService;
        private readonly IMapper mapper;
        private readonly IDistinctShell shell;

        public string Title { get; set; }
        public string FullTitle { get; set; }

        public ReactiveList<DesktopFullTask> Tasks { get; set; }
        public ReactiveList<DesktopInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopReport> Reports { get; set; }

        [Reactive] public DesktopFullTask SelectedTask { get; set; }
        [Reactive] public DesktopInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public DesktopInstance SelectedInstance { get; set; }

        public ReactiveCommand OpenPage { get; set; }

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

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            var instanceList = reportService.GetInstancesByTaskId(taskId);
            SelectedTaskInstanceCompacts.Clear();

            foreach (var instance in instanceList)
                SelectedTaskInstanceCompacts.Add(mapper.Map<DesktopInstanceCompact>(instance));
        }

        public void LoadSelectedInstanceById(int id)
        {
            SelectedInstance = mapper
                .Map<DesktopInstance>(reportService.GetFullInstanceById(id));
        }

        public void OpenPageInBrowser(string htmlPage)
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
