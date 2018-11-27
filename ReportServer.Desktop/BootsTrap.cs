using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using AutoMapper;
using Monik.Client;
using Monik.Common;
using Newtonsoft.Json;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.ViewModels;
using ReportServer.Desktop.Views;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;
using MainWindow = Ui.Wpf.Common.MainWindow;

namespace ReportServer.Desktop
{
    public class BootsTrap : IBootstraper
    {
        public static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CachedServiceShell>()
                .As<IShell>()
                .SingleInstance();

            builder
                .RegisterType<MainWindow>()
                .As<IDockWindow>();

            builder
                .RegisterType<CachedService>()
                .As<ICachedService>()
                .SingleInstance();

            ConfigureView<TaskManagerViewModel, TaskManagerView>(builder);

            ConfigureView<OperTemplatesManagerViewModel, OperTemplatesManagerView>(builder);

            ConfigureView<TaskEditorViewModel, TaskEditorView>(builder);

            ConfigureView<OperEditorViewModel, OperEditorView>(builder);

            ConfigureView<RecepientManagerViewModel, RecepientManagerView>(builder);

            ConfigureView<CronEditorViewModel, CronEditorView>(builder);

            ConfigureView<ScheduleManagerViewModel, ScheduleManagerView>(builder);

            builder.RegisterType<RecepientEditorViewModel>();

            #region monik

            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"],
                "incoming");

            builder.RegisterInstance(logSender)
                .As<IMonikSender>();

            var monikSettings = new ClientSettings()
            {
                SourceName = "ReportServer",
                InstanceName = ConfigurationManager.AppSettings["InstanceName"],
                AutoKeepAliveEnable = true
            };

            builder.RegisterInstance(monikSettings)
                .As<IMonikSettings>();

            builder
                .RegisterType<MonikClient>()
                .As<IMonik>()
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

            TelegramChannelsSource.Container = container;
            RecepGroupsSource.Container = container;
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
            CreateMap<ApiTask, DesktopTask>();

            CreateMap<DesktopTask, ApiTask>();

            CreateMap<ApiTaskInstance, DesktopTaskInstance>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<ApiOperInstance, DesktopOperInstance>()
                .ForMember("State", opt => opt.MapFrom(s => (InstanceState) s.State));

            CreateMap<TaskEditorViewModel, ApiTask>()
                .ForMember("ScheduleId", opt =>
                    opt.MapFrom(s => s.ScheduleId > 0 ? s.ScheduleId : null))
                .ForMember("Parameters",opt=>opt.MapFrom(s =>
                   JsonConvert.SerializeObject(s.TaskParameters.Items
                    .ToDictionary(param => param.Name, param => param.Value))))
                .ForMember("BindedOpers", opt => opt.Ignore());

            CreateMap<ApiOperation, DesktopOperation>();
            CreateMap<DesktopOperation, ApiOperation>();

            CreateMap<ApiTask, TaskEditorViewModel>();

            CreateMap<ApiRecepientGroup, RecepientEditorViewModel>();
            CreateMap<RecepientEditorViewModel, ApiRecepientGroup>();

            CreateMap<CachedService, IOperationConfig>();

            CreateMap<ApiOperTemplate, DesktopOperation>()
                .ForMember("Id", opt => opt.Ignore())
                .ForMember("Config", opt => opt.MapFrom(s => s.ConfigTemplate));

            CreateMap<ApiOperTemplate, OperEditorViewModel>();
            CreateMap<OperEditorViewModel, ApiOperTemplate>()
                .ForMember("ConfigTemplate", opt => opt.MapFrom
                    (s => JsonConvert.SerializeObject(s.Configuration)));
        }
    }
}