using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
using Newtonsoft.Json;

namespace ReportServer.Desktop.Model
{
    public class ReportService : IReportService
    {
        private readonly ISimpleHttpClient client;

        public ReportService()
        {
            client = JsonHttpClient.Create("http://localhost:12345/");
        }

        public List<ApiTask> GetAllTasks()
        {
            return client.Get<List<ApiTask>>("/api/v1/tasks");
        }

        public ApiFullTask GetFullTaskById(int id)
        {
            return client.Get<ApiFullTask>($"/api/v1/tasks/{id}");
        }

        public List<ApiInstance> GetInstancesByTaskId(int taskId)
        {
            return client.Get<List<ApiInstance>>($"/api/v1/tasks/{taskId}/instances");
        }

        public List<ApiInstance> GetInstanceCompacts()
        {
            return client.Get<List<ApiInstance>>("/api/v1/instances");
        }

        public ApiFullInstance GetFullInstanceById(int id)
        {
            return client.Get<ApiFullInstance>($"/api/v1/instances/{id}");
        }

        public List<ApiSchedule> GetSchedules()
        {
            return client.Get<List<ApiSchedule>>("/api/v1/schedules/");
        }

        public List<ApiReport> GetReports()
        {
            return client.Get<List<ApiReport>>("/api/v1/reports/");
        }

        public List<ApiRecepientGroup> GetRecepientGroups()
        {
            return client.Get<List<ApiRecepientGroup>>("/api/v1/recepientgroups/");
        }

        public string GetCurrentTaskViewById(int taskId)
        {

            var task = Task.Factory.StartNew(() =>
                client.Send<string>(HttpMethod.Get, $"/api/v1/tasks/{taskId}/currentviews"));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");

            return task.Result.Result.Body;
        }

        public int CreateSchedule(ApiSchedule schedule)
        {
            return client.Post("/schedules", schedule);
        }

        public void DeleteTask(int id)
        {
            client.Delete($"/api/v1/tasks/{id}");
        }

        public void DeleteInstance(int id)
        {
            client.Delete($"/api/v1/instances/{id}");
        }

        public int CreateTask(ApiFullTask fullTask)
        {
            return client.Post("/api/v1/tasks/", fullTask);
        }

        public void UpdateTask(ApiFullTask fullTask)
        {
            client.Put($"/api/v1/tasks/{fullTask.Id}", fullTask);
        }

        public int CreateReport(ApiReport report)
        {
            return client.Post("/api/v1/reports/", report);
        }

        public void UpdateReport(ApiReport report)
        {
            client.Put($"/api/v1/reports/{report.Id}", report);
        }
    }

    public static class JsonHttpClientTimeExtension
    {
        public static T Get<T>(this ISimpleHttpClient client, string path)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Get, path));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");

            return JsonConvert.DeserializeObject<T>(task.Result.Result.Body);
        }

        public static void Delete(this ISimpleHttpClient client, string url)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Delete, url));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");
        }

        public static int Post<T>(this ISimpleHttpClient client, string path, T postBody)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Post, path, null, postBody));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");

            return JsonConvert.DeserializeObject<int>(task.Result.Result.Body);
        }

        public static void Put<T>(this ISimpleHttpClient client, string path, T postBody)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Put, path, null, postBody));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");
        }

        public static T CreateClone<T>(this T someObj)
        {
            string tem = JsonConvert.SerializeObject(someObj);
            return JsonConvert.DeserializeObject<T>(tem);
        }

    }
}
