using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public class ScheduleManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public CachedServiceShell Shell { get; }

        public ReadOnlyObservableCollection<ApiSchedule> Schedules { get; set; }

        [Reactive] public ApiSchedule SelectedSchedule { get; set; }

        public ReactiveCommand<Unit, Unit> EditScheduleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }

        public ScheduleManagerViewModel(ICachedService cachedService, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            Shell = shell as CachedServiceShell;

            EditScheduleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedSchedule == null) return;

                var fullName = $"Schedule {SelectedSchedule.Id} editor";

                Shell.ShowView<CronEditorView>(new CronEditorRequest
                        {Schedule = SelectedSchedule, ViewId = fullName},
                    new UiShowOptions {Title = fullName});
            });

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete());
        }

        public void Initialize(ViewRequest viewRequest)
        {

            Shell.AddVMCommand("Edit", "Change schedule",
                "EditScheduleCommand", this);
            Shell.AddVMCommand("Edit", "Delete sched",
                    "DeleteCommand", this)
                .SetHotKey(ModifierKeys.None, Key.Delete);

            Schedules = cachedService.Schedules.SpawnCollection();
        }

        public async Task Delete()
        {
            if (SelectedSchedule != null)
            {
                var tasksWithSchedule = cachedService.Tasks.Items
                    .Where(task => task.ScheduleId == SelectedSchedule.Id)
                    .ToList();

                if (tasksWithSchedule.Any())
                {
                    var bindedTasks = string.Join(", ", tasksWithSchedule.Select(to => to.Id));
                    await Shell.ShowMessageAsync(
                        $"You can't delete this schedule. It is used in tasks {bindedTasks}");
                    return;
                }

                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ($"Do you really want to delete schedule {SelectedSchedule.Name}?"))
                    return;

                cachedService.DeleteSchedule(SelectedSchedule.Id);
                cachedService.RefreshSchedules();
            }
        }
    }
}
