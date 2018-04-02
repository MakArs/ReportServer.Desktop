using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
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

            builder
                .RegisterType<JsonContentSerializer>()
                .As<IHttpContentGetter>()
                .SingleInstance();

            builder
                .RegisterType<JsonContentSerializer>()
                .As<IHttpContentParser>()
                .SingleInstance();

            builder
                .RegisterType<JsonHttpClient>()
                .As<ISimpleHttpClient>()
                .SingleInstance();

            Container = builder.Build();
        }
    }
}
