using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportServer.Desktop.Interfaces
{
    public class ViewModelTaskCompact
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string ConnectionString { get; set; }
        public string RecepientGroup { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public TaskType TaskType { get; set; }
    }

    public class ViewModelTask
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string ConnectionString { get; set; }
        public string RecepientAddresses { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public TaskType TaskType { get; set; }
    }

    public enum TaskType : byte
    {
        Common = 1,
        Custom = 2
    }

    public interface ICore
    {
        void LoadTaskCompacts();
        void LoadSchedules();
        void LoadRecepientGroups();
        void LoadSelectedTaskById(int id);
        void LoadSelectedInstanceById(int id);
        void LoadInstanceCompactsByTaskId(int taskId);
        void OnStart();
    }
}
