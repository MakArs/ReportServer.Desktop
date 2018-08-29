using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.ViewModels
{
    public class ScheduleManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public DistinctShell Shell { get; }

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        [Reactive] public ApiSchedule SelectedSchedule { get; set; }

        public ReactiveCommand<ApiSchedule, Unit> EditScheduleCommand { get; set; }

        public ScheduleManagerViewModel(ICachedService cachedService, IShell shell)
        {
            this.cachedService = cachedService;
            Shell = shell as DistinctShell;

            EditScheduleCommand = ReactiveCommand.Create<ApiSchedule>(sched =>
            {
                if (sched == null) return;

                var fullName = $"Schedule {SelectedSchedule.Id} editor";

                Shell.ShowDistinctView<CronEditorView>(fullName,
                    new CronEditorRequest {Schedule = sched, FullId = fullName},
                    new UiShowOptions {Title = fullName});
            });

        }

        public void Initialize(ViewRequest viewRequest)
        {
            Schedules = cachedService.Schedules;
        }
    }
}
