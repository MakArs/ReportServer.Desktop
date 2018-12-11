using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;
using ReportServer.Desktop.Entities;

namespace ReportServer.Desktop.Interfaces
{
    public interface ICachedService
    {
        SourceList<ApiOperTemplate> OperTemplates { get; set; }
        SourceList<ApiRecepientGroup> RecepientGroups { get; set; }
        SourceList<ApiTelegramChannel> TelegramChannels { get; set; }
        SourceList<ApiSchedule> Schedules { get; set; }
        SourceList<ApiTask> Tasks { get; set; }
        SourceList<ApiOperation> Operations { get; set; }
        Dictionary<string, Type> DataImporters { get; set; }
        Dictionary<string, Type> DataExporters { get; set; }

        bool Init(string serviceUri);
        void RefreshOperTemplates();
        void RefreshRecepientGroups();
        void RefreshTelegramChannels();
        void RefreshSchedules();
        void RefreshTasks();
        void RefreshOperations();
        void RefreshData();

        List<ApiTaskInstance> GetInstancesByTaskId(int taskId);
        List<ApiOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId);
        ApiOperInstance GetFullOperInstanceById(int id);
        Task<string> GetCurrentTaskViewById(int taskId);

        int? CreateOrUpdateOper(ApiOperTemplate operTemplate);
        int? CreateOrUpdateRecepientGroup(ApiRecepientGroup group);
        int? CreateOrUpdateTelegramChannel(ApiTelegramChannel channel);
        int? CreateOrUpdateSchedule(ApiSchedule schedule);
        int? CreateOrUpdateTask(ApiTask task);

        void DeleteOperTemplate(int id);
        void DeleteSchedule(int id);
        void DeleteTask(int id);
        void DeleteInstance(int id);

        void OpenPageInBrowser(string htmlPage);
        Task<string> StopTaskByInstanceId(long taskInstanceId);
    }
}