using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.Model
{
    public class ReportService:IReportService
    {
        public List<ApiTaskCompact> LoadAllTaskCompacts()
        {
            throw new NotImplementedException();
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
