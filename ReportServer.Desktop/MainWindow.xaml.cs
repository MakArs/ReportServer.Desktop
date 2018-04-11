using System.Windows;
using Autofac;
using AutoMapper;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop
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
            var m= BootsTrap.Container.Resolve<IMapper>();
            var newtask = new ApiTask()
            {
                ConnectionString = null,
                Id = 24,
                ScheduleId = 3,
                Query = "hom-hom-om",
                QueryTimeOut = 62,
                ViewTemplate = "om-hom-hom",
                RecepientGroupId = 1,
                TryCount = 15,
                TaskType = 1
            };
            //t.UpdateTask(newtask);
            DataContext = new Core(r,m);
        }
    }
}
