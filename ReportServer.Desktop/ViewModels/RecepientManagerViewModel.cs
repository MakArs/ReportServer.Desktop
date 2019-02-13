using System.Collections.ObjectModel;
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
    public class RecepientManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public CachedServiceShell Shell { get; }

        public ReadOnlyObservableCollection<ApiRecepientGroup> RecepientGroups { get; set; }
        [Reactive] public ApiRecepientGroup SelectedGroup { get; set; }

        public ReactiveCommand<Unit, Unit> EditGroupCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; set; }
      
        public RecepientManagerViewModel(ICachedService cachedService, IShell shell)
        {
            CanClose = false;
            this.cachedService = cachedService;
            Shell = shell as CachedServiceShell;

            var editorOptions = new UiShowChildWindowOptions
            {
                IsModal = true,
                AllowMove = true,
                CanClose = true,
                ShowTitleBar = true,
                Title = "Recepient group editing"
            };

            CreateGroupCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await Shell.ShowChildWindowView<RecepientEditorView, Unit>(
                    new RecepientEditorRequest
                        {Group = new ApiRecepientGroup()},
                    editorOptions);
            }, Shell.CanEdit);

            EditGroupCommand = ReactiveCommand.CreateFromTask(async () =>
            {
               await Shell.ShowChildWindowView<RecepientEditorView, Unit>(
                    new RecepientEditorRequest
                        {Group = SelectedGroup},
                    editorOptions);

            }, Shell.CanEdit);
        }

        public void Initialize(ViewRequest viewRequest)
        {
            if (Shell.Role == ServiceUserRole.Editor)
                Shell.AddVMCommand("Edit", "Change recepient group",
                    "EditGroupCommand", this);

            RecepientGroups = cachedService.RecepientGroups.SpawnCollection();
        }
    }
}