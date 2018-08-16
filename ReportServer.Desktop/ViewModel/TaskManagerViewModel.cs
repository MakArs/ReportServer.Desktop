using System;
using System.IO;
using System.Linq;
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

        public ReactiveList<DesktopTask> TaskCompacts { get; set; }
        public ReactiveList<DesktopInstanceCompact> SelectedTaskInstanceCompacts { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopReport> Reports { get; set; }

        [Reactive] public DesktopTask SelectedTaskCompact { get; set; }
        [Reactive] public DesktopInstanceCompact SelectedInstanceCompact { get; set; }
        [Reactive] public DesktopInstance SelectedInstance { get; set; }

        public ReactiveCommand OpenPage { get; set; }

        public TaskManagerViewModel(IReportService reportService, IMapper mapper, IShell shell)
        {
            this.reportService = reportService;
            this.mapper = mapper;
            this.shell = shell as IDistinctShell;
            TaskCompacts = new ReactiveList<DesktopTask>();
            SelectedTaskInstanceCompacts = new ReactiveList<DesktopInstanceCompact>();
            Schedules = new ReactiveList<ApiSchedule>();
            RecepientGroups = new ReactiveList<ApiRecepientGroup>();
            Reports = new ReactiveList<DesktopReport>();

            IObservable<bool> canOpenInstancePage = this
                .WhenAnyValue(t => t.SelectedInstance,
                    si => !string.IsNullOrEmpty(si?.ViewData));

            OpenPage = ReactiveCommand.Create<string>
                (OpenPageInBrowser, canOpenInstancePage);

            this.WhenAnyObservable(s => s.TaskCompacts.Changed)
                .Subscribe(x =>
                {
                    SelectedTaskInstanceCompacts.Clear();
                    SelectedTaskCompact = null;
                });

            this.ObservableForProperty(s => s.SelectedTaskCompact)
                .Where(x => x.Value != null)
                .Subscribe(x => LoadInstanceCompactsByTaskId(x.Value.Id));

            this.ObservableForProperty(s => s.SelectedInstanceCompact)
                .Subscribe(x =>
                {
                    if (x.Value == null)
                        SelectedInstance = null;
                    else
                        LoadSelectedInstanceById(x.Value.Id);
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
            LoadSchedules();
            LoadRecepientGroups();
            LoadReports();
            LoadTaskCompacts();
        }

        public void LoadSchedules()
        {
            var scheduleList = reportService.GetSchedules();

            Schedules.PublishCollection(scheduleList);
        }

        public void LoadRecepientGroups()
        {
            var recepientGroupList = reportService.GetRecepientGroups();

            RecepientGroups.PublishCollection(recepientGroupList);
        }

        public void LoadReports()
        {
            var reportsList = reportService.GetReports()
                .Select(rep => mapper.Map<DesktopReport>(rep));

            Reports.PublishCollection(reportsList);
        }

        public void LoadTaskCompacts() //
        {
            var taskList = reportService.GetAllTasks();
            TaskCompacts.Clear();

            foreach (var task in taskList)
            {
                var vtask = mapper.Map<DesktopTask>(task);

                vtask.Schedule = Schedules
                    .FirstOrDefault(s => s.Id == task.ScheduleId)?.Name;

                vtask.RecepientGroup = RecepientGroups
                    .FirstOrDefault(r => r.Id == task.RecepientGroupId)?.Name;

                vtask.ReportName = Reports.FirstOrDefault(r => r.Id == task.ReportId)?.Name;

                TaskCompacts.Add(vtask);
            }
        }
    }
}
