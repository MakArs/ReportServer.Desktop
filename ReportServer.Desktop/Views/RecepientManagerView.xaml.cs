using ReportServer.Desktop.ViewModels;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views
{
    public partial class RecepientManagerView : IView
    {
        public RecepientManagerView(RecepientManagerViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
        }

        public IViewModel ViewModel { get; set; }
    }
}