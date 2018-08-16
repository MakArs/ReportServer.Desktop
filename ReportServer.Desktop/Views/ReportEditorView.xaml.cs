using System.Windows.Controls;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views
{
    /// <summary>
    /// Interaction logic for SelectedReportFullView.xaml
    /// </summary>
    public partial class ReportEditorView : IView
    {
        public ReportEditorView(ICore viewModel)
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
