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
        private readonly ISimpleHttpClient _client;

        public ReportService()
        {
            _client = JsonHttpClient.Create("http://localhost:12345/");
        }

        public List<ApiTaskCompact> GetAllTaskCompacts()
        {
            return _client.Get<List<ApiTaskCompact>>("/api/v1/reports");
        }

        public ApiTask GetTaskById(int id)
        {
            return _client.Get<ApiTask>($"/api/v1/reports/{id}");
        }

        public ApiInstance GetInstanceById(int id)
        {
            return _client.Get<ApiInstance>($"/api/v1/instances/{id}");
        }

        public List<ApiInstanceCompact> GetInstanceCompactsByTaskId(int taskId)
        {
            return _client.Get<List<ApiInstanceCompact>>($"/api/v1/reports/{taskId}/instances");
        }

        public List<ApiInstanceCompact> GetInstanceCompacts()
        {
            return _client.Get<List<ApiInstanceCompact>>("/api/v1/instances");
        }

        public void DeleteTask(int id)
        {
            _client.Delete($"/api/v1/reports/{id}");
        }

        public void DeleteInstance(int id)
        {
            _client.Delete($"/api/v1/instances/{id}");
        }

        public int CreateTask(ApiTask task)
        {
            return _client.Post("/api/v1/reports/", task);
        }

        public void UpdateTask(ApiTask task)
        {
            _client.Put($"/api/v1/reports/{task.Id}", task);
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
    }
}
