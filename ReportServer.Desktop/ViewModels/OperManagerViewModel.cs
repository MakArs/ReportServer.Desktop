using System.Reactive;
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
    public class OperManagerViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly ICachedService cachedService;
        public DistinctShell Shell { get; }

        public ReactiveList<ApiOper> Operations { get; set; }
        [Reactive] public ApiOper SelectedOper { get; set; }

        public ReactiveCommand<ApiOper, Unit> EditOperCommand { get; set; }

        public OperManagerViewModel(ICachedService cachedService, IShell shell)
        {
            this.cachedService = cachedService;
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
    }
}