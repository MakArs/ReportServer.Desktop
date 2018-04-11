using System;
using System.Collections.Generic;

namespace ReportServer.Desktop.Interfaces
{
    public class ApiTask
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class ApiTaskCompact 
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class ApiInstance
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class ApiInstanceCompact
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class ApiSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class ApiRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
    }

    public interface IReportService
    {
        List<ApiTaskCompact> GetAllTaskCompacts();
        ApiTask GetTaskById(int id);
        List<ApiInstanceCompact> GetInstanceCompacts();
        List<ApiInstanceCompact> GetInstanceCompactsByTaskId(int taskId);
        ApiInstance GetInstanceById(int id);
        List<ApiSchedule> GetSchedules();
        List<ApiRecepientGroup> GetRecepientGroups();

        void DeleteTask(int id);
        void DeleteInstance(int id);
        int CreateTask(ApiTask task);
        void UpdateTask(ApiTask task);
    }
}
