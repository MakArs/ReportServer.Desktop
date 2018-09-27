using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
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
    public class OperManagerViewModel : ViewModelBase, IInitializableViewModel, IDeleteableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IDialogCoordinator dialogCoordinator;
        public DistinctShell Shell { get; }

        public ReactiveList<ApiOper> Operations { get; set; }
        [Reactive] public ApiOper SelectedOper { get; set; }

        public ReactiveCommand<ApiOper, Unit> EditOperCommand { get; set; }

        public OperManagerViewModel(ICachedService cachedService, IShell shell,
                                    IDialogCoordinator dialogCoordinator)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.dialogCoordinator = dialogCoordinator;
            Shell = shell as DistinctShell;

            EditOperCommand = ReactiveCommand.Create<ApiOper>(oper =>
            {
                if (oper == null) return;

                var fullName = $"Oper {oper.Id} editor";

                Shell.ShowDistinctView<OperEditorView>(fullName,
                    new OperEditorRequest {Oper = oper, FullId = fullName},
                    new UiShowOptions {Title = fullName});
            });
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Operations = cachedService.Operations;
        }

        public async Task Delete()
        {
            if (SelectedOper != null)

            {
                var taskOpers = cachedService.TaskOpers.Where(to => to.OperId == SelectedOper.Id)
                    .ToList();

                if (taskOpers.Any())
                {
                    var bindedTasks = string.Join(", ", taskOpers.Select(to => to.TaskId));
                    await dialogCoordinator.ShowMessageAsync(this, "Warning",
                        $"You can't delete this operation. It is used in tasks {bindedTasks}");
                    return;
                }

                if (!await ShowWarningAffirmativeDialog
                    ($"Do you really want to delete operation {SelectedOper.Name}?")) return;

                cachedService.DeleteOperation(SelectedOper.Id);
                cachedService.RefreshOpers();
            }
        }

        private async Task<bool> ShowWarningAffirmativeDialog(string question)
        {
            var dialogResult = await dialogCoordinator.ShowMessageAsync(this, "Warning",
                question,
                MessageDialogStyle.AffirmativeAndNegative);
            return dialogResult == MessageDialogResult.Affirmative;
        }
    }
}