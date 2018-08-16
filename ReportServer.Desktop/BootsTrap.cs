using System.Configuration;
using Autofac;
using AutoMapper;
using Monik.Client;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.ViewModel;
using ReportServer.Desktop.Views;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;
using MainWindow = ReportServer.Desktop.Views.MainWindow;

namespace ReportServer.Desktop
{
    public class BootsTrap : IBootstraper
    {
        public static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DistinctShell>()
                .As<IShell>()
                .SingleInstance();

            builder
                .RegisterType<MainWindow>()
                .As<IDockWindow>();

            builder
                .RegisterType<ReportService>()
                .As<IReportService>()
                .SingleInstance();

            ConfigureView<TaskManagerViewModel, TaskManagerView>(builder);

            builder.RegisterType<ReportManagerView>();
            builder.RegisterType<TaskEditorView>();
            builder.RegisterType<ReportEditorView>();


            //builder
            //    .RegisterType<Core>()
            //    .As<ICore>()
            //    .SingleInstance();

            #region monik

            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"],
                "incoming");

            builder.RegisterInstance(logSender)
                .As<IClientSender>();

            var monikSettings = new ClientSettings()
            {
                SourceName = "ReportServer",
                InstanceName = ConfigurationManager.AppSettings["InstanceName"],
                AutoKeepAliveEnable = true
            };

            builder.RegisterInstance(monikSettings)
                .As<IClientSettings>();

            builder
                .RegisterType<MonikInstance>()
                .As<IClientControl>()
                .SingleInstance();

            #endregion

            #region mapper

            var mapperConfig =
                new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));

            builder.RegisterInstance(mapperConfig)
                .As<MapperConfiguration>()
                .SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>()
                    .CreateMapper())
                .As<IMapper>()
                .SingleInstance();

            #endregion

            var container = builder.Build();
            return container;
        }

        public IShell Init()
        {
            var container = InitContainer();
            var shell = container.Resolve<IShell>();
            shell.Container = container;
            return shell;
        }

        private static void ConfigureView<TViewModel, TView>(ContainerBuilder builder)
            where TViewModel : IViewModel
            where TView : IView
        {
            builder.RegisterType<TViewModel>();
            builder.RegisterType<TView>();
        }
    }

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ApiTask, DesktopTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));

            CreateMap<ApiFullTask, DesktopFullTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));

            CreateMap<DesktopFullTask, ApiFullTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (int) s.ReportType));

            CreateMap<ApiInstance, DesktopInstanceCompact>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<ApiFullInstance, DesktopInstance>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<ApiReport, DesktopReport>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));
            CreateMap<DesktopReport, ApiReport>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (int) s.ReportType));
        }
    }
}