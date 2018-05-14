using System;
using System.Collections.Generic;

namespace ReportServer.Desktop.Interfaces
{
    public class ApiRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
    }

    public class ApiSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class ApiReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int ReportType { get; set; }
        public int QueryTimeOut { get; set; } //seconds
    }

    public class ApiFullTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int? ScheduleId { get; set; }
        public int? RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int QueryTimeOut { get; set; }
        public int ReportType { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
    }

  public class ApiTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int ScheduleId { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int ReportType { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
    }

    public class ApiFullInstance
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string ViewData { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class ApiInstance
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public interface IReportService
    {
        List<ApiTask> GetAllTasks();
        ApiFullTask GetFullTaskById(int id);
        List<ApiInstance> GetInstanceCompacts();
        List<ApiInstance> GetInstancesByTaskId(int taskId);
        ApiFullInstance GetFullInstanceById(int id);
        List<ApiSchedule> GetSchedules();
        List<ApiRecepientGroup> GetRecepientGroups();
        List<ApiReport> GetReports();

        string GetCurrentTaskViewById(int taskId);
        int CreateSchedule(ApiSchedule schedule);

        void DeleteTask(int id);
        void DeleteInstance(int id);

        int CreateTask(ApiFullTask fullTask);
        void UpdateTask(ApiFullTask fullTask);

        int CreateReport(ApiReport report);
        void UpdateReport(ApiReport report);
    }
}
