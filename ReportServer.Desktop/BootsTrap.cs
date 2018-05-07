using System.Configuration;
using Autofac;
using AutoMapper;
using Monik.Client;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Model;
using ReportServer.Desktop.ViewModel;

namespace ReportServer.Desktop
{
    public class BootsTrap
    {
        public static IContainer Container { get; set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<Core>()
                .As<ICore>()
                .SingleInstance();

            builder
                .RegisterType<ReportService>()
                .As<IReportService>()
                .SingleInstance();

            //monik
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

            //mapper
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));

            builder.RegisterInstance(mapperConfig)
                .As<MapperConfiguration>()
                .SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>()
                    .CreateMapper())
                .As<IMapper>()
                .SingleInstance();

            Container = builder.Build();
        }
    }

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ApiTask, ViewModelTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));

            CreateMap<ApiFullTask, ViewModelFullTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));

            CreateMap<ViewModelFullTask, ApiFullTask>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (int)s.ReportType));

            CreateMap<ApiInstance, ViewModelInstanceCompact>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<ApiFullInstance, ViewModelInstance>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<ApiReport, ViewModelReport>()
                .ForMember("ReportType", opt => opt.MapFrom(s => (ReportType) s.ReportType));
        }
    }
}
