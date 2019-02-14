using System.Collections.ObjectModel;
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

namespace ReportServer.Desktop.ViewModels.General
{
    public class OperTemplatesManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public CachedServiceShell Shell { get; }

        public ReadOnlyObservableCollection<ApiOperTemplate> OperTemplates { get; set; }
        [Reactive] public ApiOperTemplate SelectedOperTemplate { get; set; }

        public ReactiveCommand<Unit, Unit> EditOperCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }

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
                await Delete(), Shell.CanEdit);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (Shell.Role == ServiceUserRole.Editor)
            {
                Shell.AddVMCommand("Edit", "Delete optempl",
                        "DeleteCommand", this)
                    .SetHotKey(ModifierKeys.None, Key.Delete);

                Shell.AddVMCommand("Edit", "Change oper template",
                    "EditOperCommand", this);
            }

            if (Shell.Role == ServiceUserRole.Viewer) Shell.AddVMCommand("View", "View oper template",
                "EditOperCommand", this);

            OperTemplates = cachedService.OperTemplates.SpawnCollection();
        }

        public async Task Delete()
        {
            if (SelectedOperTemplate != null)

            {
                if (!await Shell.ShowWarningAffirmativeDialogAsync
                    ($"Do you really want to delete operation {SelectedOperTemplate.Name}?"))
                    return;

                cachedService.DeleteOperTemplate(SelectedOperTemplate.Id);
                cachedService.RefreshOperTemplates();
            }
        }
    }
}