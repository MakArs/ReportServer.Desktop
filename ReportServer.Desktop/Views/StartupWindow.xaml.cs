using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autofac;
using AutoMapper;
using MahApps.Metro.Controls;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.ViewModel;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop.Views
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : IView
    {
        public StartupWindow(ICore viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            //BootsTrap.Init();
            // var r = BootsTrap.Container.Resolve<IReportService>();
            // var m = BootsTrap.Container.Resolve<IMapper>();
            //var reps=r.GetReports();
            // DataContext = new Core(r, m);
        }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
        }

        public IViewModel ViewModel { get; set; }

    }
}
