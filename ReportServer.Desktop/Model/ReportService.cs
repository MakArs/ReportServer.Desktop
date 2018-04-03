using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;

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
            var resp=Task.Factory.StartNew(() => _client.SendEnsure<string>(HttpMethod.Get, "/api/v1/reports"));
            var result = resp.Result;
            return new List<ApiTaskCompact>();
        }

        public ApiTask LoadTaskById(int id)
        {
            throw new NotImplementedException();
        }

        public ApiInstance LoadInstanceById(int id)
        {
            throw new NotImplementedException();
        }

        public List<ApiInstanceCompact> LoadInstanceCompactsByTaskId(int taskId)
        {
            throw new NotImplementedException();
        }

        public List<ApiInstanceCompact> LoadInstanceCompacts()
        {
            throw new NotImplementedException();
        }
    }
}
