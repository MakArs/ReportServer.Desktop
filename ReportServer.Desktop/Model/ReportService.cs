using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;

namespace ReportServer.Desktop.Model
{
    public class ReportService:IReportService
    {
        
        public List<ApiTaskCompact> LoadAllTaskCompacts()
        {
            throw new NotImplementedException();
            // CustomHttpClient client = CustomHttpClient.Create
            //(JsonContentSerializer, JsonContentSerializer, "http://localhost:12345");
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
