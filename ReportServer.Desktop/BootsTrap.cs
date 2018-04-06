using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
using Newtonsoft.Json;
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

            var jsonClient = JsonHttpClient.Create("http://localhost:12345/");

            builder.RegisterInstance(jsonClient)
                .As<ISimpleHttpClient>()
                .SingleInstance(); 

            Container = builder.Build();
        }
    }
}

public static class JsonHttpClientTimeExtension
{
    public static T Get<T>(this ISimpleHttpClient client, string path)
    {
        var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Get, path));
        task.Wait();
        return JsonConvert.DeserializeObject<T>(task.Result.Result.Body);
    }

    public static HttpResult<string> Delete(this ISimpleHttpClient client, string path)
    {
        var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Delete, path));
        task.Wait();
        return task.Result.Result;
    }

    public static HttpResult<string> Post<T>(this ISimpleHttpClient client, string path, T postBody)
    {
        var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Post, path, null, postBody));
        task.Wait();
        return task.Result.Result;
    }
    public static HttpResult<string> Put<T>(this ISimpleHttpClient client, string path, T postBody)
    {
        var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Put, path, null, postBody));
        task.Wait();
        return task.Result.Result;
    }
}