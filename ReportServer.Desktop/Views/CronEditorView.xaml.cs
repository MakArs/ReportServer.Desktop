using ReportServer.Desktop.ViewModels;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views
{
    public partial class CronEditorView : IView
    {
        public CronEditorView(CronEditorViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
            ViewModel.FullTitle = options.Title;
        }

        public IViewModel ViewModel { get; set; }
    }
}
