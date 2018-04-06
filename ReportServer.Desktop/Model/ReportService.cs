using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Newtonsoft.Json;

namespace ReportServer.Desktop.Model
{
    public class ReportService:IReportService
    {
        private readonly ISimpleHttpClient _client;
        

        public ReportService(ISimpleHttpClient client)
        {
            _client = client;
        }

        public List<ApiTaskCompact> LoadAllTaskCompacts()
        {
            return _client.Get<List<ApiTaskCompact>>("/api/v1/reports");
        }

        public ApiTask LoadTaskById(int id)
        {
            return _client.Get<ApiTask>($"/api/v1/reports/{id}");
        }

        public ApiInstance LoadInstanceById(int id)
        {
            return _client.Get<ApiInstance>($"/api/v1/instances/{id}");
        }

        public List<ApiInstanceCompact> LoadInstanceCompactsByTaskId(int taskId)
        {
            return _client.Get<List<ApiInstanceCompact>>($"/api/v1/reports/{taskId}/instances");
        }

        public List<ApiInstanceCompact> LoadInstanceCompacts()
        {
            return _client.Get<List<ApiInstanceCompact>>($"/api/v1/instances");
        }
    }
}

