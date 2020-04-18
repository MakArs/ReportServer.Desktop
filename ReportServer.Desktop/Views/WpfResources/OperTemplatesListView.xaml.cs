using ReportServer.Desktop.ViewModels.Editors;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views.WpfResources
{
    public partial class OperTemplatesListView : IView
    {
        public OperTemplatesListView(OperTemplatesListViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
        }

        public IViewModel ViewModel { get; set; }
    }
}
