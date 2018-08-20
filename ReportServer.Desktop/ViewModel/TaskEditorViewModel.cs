using System.ComponentModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModel
{
    public class TaskEditorViewModel : ViewModelBase, IInitializableViewModel, ISaveableViewModel
    {
        private readonly IDialogCoordinator dialogCoordinator = DialogCoordinator.Instance;
        private readonly IReportService reportService;
        private readonly IMapper mapper;

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopReport> Reports { get; set; }

        [Reactive] public DesktopReport SelectedReport { get; set; }
        public int Id { get; set; }
        public int? TelegramChannelId { get; set; }
        [Reactive] public int? ReportId { get; set; }
        [Reactive] public int? ScheduleId { get; set; }
        [Reactive] public int? RecepientGroupId { get; set; }
        [Reactive] public int TryCount { get; set; }
        [Reactive] public bool HasHtmlBody { get; set; }
        [Reactive] public bool HasJsonAttachment { get; set; }
        [Reactive] public bool HasXlsxAttachment { get; set; }
        [Reactive] public bool IsDirty { get; set; }
        [Reactive] public bool IsValid { get; set; }

        public ReactiveCommand SaveChangesCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand OpenCurrentTaskViewCommand { get; set; }

        public TaskEditorViewModel(IReportService reportService, IMapper mapper)
        {
            this.reportService = reportService;
            this.mapper = mapper;
            validator = new TaskEditorValidator();
            IsValid = true;

            OpenCurrentTaskViewCommand = ReactiveCommand
                .CreateFromTask(async () =>
                {
                    var str = await reportService.GetCurrentTaskViewById(Id);
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
                        "Все несохранённые изменения пропадут. Действительно закрыть окно редактирования?"
                        , MessageDialogStyle.AffirmativeAndNegative);

                    if (dialogResult != MessageDialogResult.Affirmative)
                        return;
                }

                Close();
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
            Schedules = reportService.Schedules;
            RecepientGroups = reportService.RecepientGroups;
            Reports = reportService.Reports;

            if (viewRequest is TaskEditorRequest request)
            {
                mapper.Map(request.Task, this);
            }

            if (Id == 0)
            {
                SelectedReport = Reports.FirstOrDefault();
                ReportId = Reports.FirstOrDefault()?.Id;
                RecepientGroupId = RecepientGroups.First()?.Id;
                ScheduleId = Schedules.First()?.Id;
            }
            else
                SelectedReport = Reports.First(rep => rep.Id == ReportId);

            PropertyChanged += Changed;

            IsDirty = false;

            void Changed(object sender, PropertyChangedEventArgs e)
            {
                IsDirty = true;
                if (Title.Last() != '*')
                    Title += '*';
            }

        }

        public async Task Save()
        {
            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                Id > 0
                    ? "Вы действительно хотите изменить эту задачу?"
                    : "Вы действительно хотите создать задачу?"
                , MessageDialogStyle.AffirmativeAndNegative);

            if (dialogResult != MessageDialogResult.Affirmative) return;

            var editedTask = new ApiTask();
            mapper.Map(this, editedTask);

            reportService.CreateOrUpdateTask(editedTask);

            Close();
            reportService.RefreshData();
        }
    }
}