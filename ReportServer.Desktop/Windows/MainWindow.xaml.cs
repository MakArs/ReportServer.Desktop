using System.Windows;
using Autofac;
using AutoMapper;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BootsTrap.Init();
            var r = BootsTrap.Container.Resolve<IReportService>();
            var m = BootsTrap.Container.Resolve<IMapper>();
            //t.UpdateTask(newtask);
            DataContext = new Core(r, m);
        }
    }
}

