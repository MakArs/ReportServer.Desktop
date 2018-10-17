using System.Linq;
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
    public class OperTemplatesManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public CachedServiceShell Shell { get; }

        public ReactiveList<ApiOperTemplate> OperTemplates { get; set; }
        [Reactive] public ApiOperTemplate SelectedOperTemplate { get; set; }

        public ReactiveCommand EditOperCommand { get; set; }
        public ReactiveCommand DeleteCommand { get; set; }

        public OperTemplatesManagerViewModel(ICachedService cachedService, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            Shell = shell as CachedServiceShell;

            EditOperCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedOperTemplate == null) return;

                var fullName = $"Oper template {SelectedOperTemplate.Id} editor";

                Shell.ShowView<OperEditorView>(new OperEditorRequest
                        {Oper = SelectedOperTemplate, ViewId = fullName},
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

            Shell.AddVMCommand("Edit", "Change oper template",
                "EditOperCommand", this);

            OperTemplates = cachedService.OperTemplates;
        }

        public async Task Delete()
        {
            if (SelectedOperTemplate != null)

            {
                var taskOpers = cachedService.Operations
                    .ToList();

                if (taskOpers.Any())
                {
                    var bindedTasks = string.Join(", ", taskOpers.Select(to => to.TaskId));
                    await Shell.ShowMessageAsync(
                        $"You can't delete this operation. It is used in tasks {bindedTasks}");
                    return;
                }

                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ($"Do you really want to delete operation {SelectedOperTemplate.Name}?"))
                    return;

                cachedService.DeleteOperTemplate(SelectedOperTemplate.Id);
                cachedService.RefreshOperTemplates();
            }
        }
    }
}