using System.Linq;
using System.Reactive;
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
    public class OperTemplatesManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        private readonly IDialogCoordinator dialogCoordinator;
        public CachedServiceShell Shell { get; }

        public ReactiveList<ApiOper> OperTemplates { get; set; }
        [Reactive] public ApiOper SelectedOper { get; set; }

        public ReactiveCommand EditOperCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public OperTemplatesManagerViewModel(ICachedService cachedService, IShell shell,
                                    IDialogCoordinator dialogCoordinator)
        {
            CanClose = false;
            this.cachedService = cachedService;
            this.dialogCoordinator = dialogCoordinator;
            Shell = shell as CachedServiceShell;

            EditOperCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedOper == null) return;

                var fullName = $"Oper template {SelectedOper.Id} editor";

                Shell.ShowView<OperEditorView>(new OperEditorRequest
                        { Oper = SelectedOper, ViewId = fullName },
                    new UiShowOptions { Title = fullName });
            });

            DeleteCommand = ReactiveCommand.CreateFromTask(async () =>
                await Delete());
        }

        public void Initialize(ViewRequest viewRequest)
        {
            Shell.AddVMCommand("File", "Delete",
                    "DeleteCommand", this)
                .SetHotKey(ModifierKeys.None, Key.Delete);
            
            Shell.AddVMCommand("Edit", "Change oper template",
                "EditOperCommand", this);

            OperTemplates = cachedService.Operations;
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