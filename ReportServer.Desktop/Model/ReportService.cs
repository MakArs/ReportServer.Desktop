using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Newtonsoft.Json;

namespace ReportServer.Desktop.Model
{
    public class ReportService : IReportService
    {
        private readonly ISimpleHttpClient _client;


        public ReportService(ISimpleHttpClient client)
        {
            _client = client;
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

        public HttpResult<string> DeleteTask(int id)
        {
            return _client.Delete($"/api/v1/reports/{id}");
        }

        public HttpResult<string> DeleteInstance(int id)
        {
            return _client.Delete($"/api/v1/instances/{id}");
        }

        public HttpResult<string> PostTask(ApiTask task)
        {
            return _client.Post("/api/v1/reports/", task);
        }
        public HttpResult<string> PutTask(ApiTask task)
        {
            return _client.Put($"/api/v1/reports/{task.Id}", task);
        }
    }
}

