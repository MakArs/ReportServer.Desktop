using System.Linq;
using Autofac;
using AutoMapper;
using Domain0.Api.Client;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using ReportServer.Desktop.Models;
using ReportServer.Desktop.ViewModels.Editors;
using ReportServer.Desktop.ViewModels.General;
using ReportServer.Desktop.Views;
using ReportServer.Desktop.Views.WpfResources;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace ReportServer.Desktop
{
    public class BootsTrap : IBootstraper
    {
        public static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => new AppConfigStorage())
                .As<IAppConfigStorage>()
                .SingleInstance();
            
            builder.RegisterType<CachedServiceShell>()
                .As<IShell>()
                .SingleInstance();

            builder
                .RegisterType<MainWindow>()
                .As<IDockWindow>()
                .SingleInstance();

            builder.RegisterInstance(DialogCoordinator.Instance)
                .As<IDialogCoordinator>()
                .SingleInstance();

            builder
                .RegisterType<CachedService>()
                .As<ICachedService>()
                .SingleInstance();

            builder.RegisterInstance(new AuthenticationContext(enableAutoRefreshTimer: true))
                .As<IAuthenticationContext>()
                .SingleInstance();

            ConfigureView<TaskManagerViewModel, TaskManagerView>(builder);

            ConfigureView<OperTemplatesManagerViewModel, OperTemplatesManagerView>(builder);

            ConfigureView<TaskEditorViewModel, TaskEditorView>(builder);

            ConfigureView<OperEditorViewModel, OperEditorView>(builder);

            ConfigureView<RecepientManagerViewModel, RecepientManagerView>(builder);

            ConfigureView<CronEditorViewModel, CronEditorView>(builder);

            ConfigureView<ScheduleManagerViewModel, ScheduleManagerView>(builder);

            ConfigureView<RecepientEditorViewModel, RecepientEditorView>(builder);

            ConfigureView<OperTemplatesListViewModel, OperTemplatesListView>(builder);

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
                .ForMember("Parameters", opt => opt.MapFrom(s => s.TaskParameters.Count == 0
                    ? null
                    : JsonConvert.SerializeObject(s.TaskParameters
                        .ToDictionary(param => param.Name, param => param.Value))))
                .ForMember("BindedOpers", opt => opt.Ignore())
                .ForMember("DependsOn", opt => opt.Ignore())
                .ForMember("ParameterInfos", opt => opt.MapFrom(s => s.TaskParameterInfos.Count == 0
                    ? null
                    : JsonConvert.SerializeObject(s.TaskParameterInfos)));

            CreateMap<ApiTaskDependence, DesktopTaskDependence>();
            CreateMap<DesktopTaskDependence, ApiTaskDependence>();

            CreateMap<ApiOperation, DesktopOperation>();
            CreateMap<DesktopOperation, ApiOperation>();

            CreateMap<ApiTask, TaskEditorViewModel>()
                //.ForMember("DependsOn", opt => opt.MapFrom((s,d)=>))
                .ForMember("BindedOpers", opt => opt.Ignore());

            CreateMap<ApiTask, DesktopTaskNameId>()
                .ForMember("Name", opt => opt.MapFrom(s =>
                    s.Name + " (" + s.Id + ")"));

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
