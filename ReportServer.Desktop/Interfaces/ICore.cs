using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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

    public class ViewModelTask : ReactiveObject
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string ConnectionString { get; set; }
        public string RecepientGroup { get; set; }
        [Reactive] public string ViewTemplate { get; set; }
        [Reactive] public string Query { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public TaskType TaskType { get; set; }
    }

    public enum TaskType : byte
    {
        Common = 1,
        Custom = 2
    }

    public class ViewModelInstanceCompact
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public int TryNumber { get; set; }
    }

    public class ViewModelInstance
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public InstanceState State { get; set; }
        public int TryNumber { get; set; }
    }

    public enum InstanceState
    {
        InProcess = 1,
        Success = 2,
        Failed = 3
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
        void DeleteEntity();
        void SaveTask();
        void CreateTask();
        IObservable<Unit> OpenPageInBrowser(string htmlPage);
        IObservable<Unit> GetHtmlPageByTaskId(int taskId);
    }
}
