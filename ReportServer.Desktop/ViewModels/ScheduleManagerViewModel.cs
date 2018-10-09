using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public class ScheduleManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IDialogCoordinator dialogCoordinator;
        public CachedServiceShell Shell { get; }

        public ReactiveList<ApiSchedule> Schedules { get; set; }
        [Reactive] public ApiSchedule SelectedSchedule { get; set; }

        public ReactiveCommand EditScheduleCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public ScheduleManagerViewModel(ICachedService cachedService, IShell shell,
                                        IDialogCoordinator dialogCoordinator)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.dialogCoordinator = dialogCoordinator;
            Shell = shell as CachedServiceShell;

            EditScheduleCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedSchedule == null) return;

                var fullName = $"Schedule {SelectedSchedule.Id} editor";

                Shell.ShowView<CronEditorView>(new CronEditorRequest
                        { Schedule = SelectedSchedule, ViewId = fullName},
                    new UiShowOptions {Title = fullName});
            });

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Shell.AddVMCommand("File", "Delete",
                    "DeleteCommand", this)
                .SetHotKey(ModifierKeys.None, Key.Delete);

            Shell.AddVMCommand("Edit", "Change schedule",
                "EditScheduleCommand", this);

            Schedules = cachedService.Schedules;
        }

        public async Task Delete()
        {
            if (SelectedSchedule != null)
            {
                var tasksWithSchedule = cachedService.Tasks.Where(task => task.ScheduleId == SelectedSchedule.Id)
                    .ToList();

                if (tasksWithSchedule.Any())
                {
                    var bindedTasks = string.Join(", ", tasksWithSchedule.Select(to => to.Id));
                    await dialogCoordinator.ShowMessageAsync(this, "Warning",
                        $"You can't delete this schedule. It is used in tasks {bindedTasks}");
                    return;
                }

                if (!await ShowWarningAffirmativeDialog
                    ($"Do you really want to delete schedule {SelectedSchedule.Name}?")) return;

                cachedService.DeleteSchedule(SelectedSchedule.Id);
                cachedService.RefreshSchedules();
            }
        }

        private async Task<bool> ShowWarningAffirmativeDialog(string question)
        {
            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                question
                , MessageDialogStyle.AffirmativeAndNegative);
            return dialogResult == MessageDialogResult.Affirmative;
        }
    }
}
