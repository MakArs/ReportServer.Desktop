﻿using System;
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

        Task<bool> Connect(string serviceUri);
        bool IsConnected { get; }
        bool Init(string token);
        Task<ServiceUserRole> GetUserRole(string token);
        void RefreshOperTemplates();
        void RefreshRecipientGroups();
        void RefreshTelegramChannels();
        void RefreshSchedules();
        void RefreshTasks();
        void RefreshOperations();
        void RefreshData();

        List<ApiTaskInstance> GetInstancesByTaskId(long taskId);
        List<ApiOperInstance> GetOperInstancesByTaskInstanceId(long taskInstanceId);
        ApiOperInstance GetFullOperInstanceById(long id);
        Task<string> GetCurrentTaskViewById(long taskId);
        Task<string> StartTaskById(long taskId);
        Task<List<long>> GetWorkingTaskInstancesById(long taskId);

        int? CreateOrUpdateOper(ApiOperTemplate operTemplate);
        int? CreateOrUpdateRecipientGroup(ApiRecepientGroup group);
        int? CreateOrUpdateTelegramChannel(ApiTelegramChannel channel);
        int? CreateOrUpdateSchedule(ApiSchedule schedule);
        long? CreateOrUpdateTask(ApiTask task);

        void DeleteOperTemplate(int id);
        void DeleteSchedule(int id);
        void DeleteTask(long id);
        void DeleteInstance(long id);

        void OpenPageInBrowser(string htmlPage);
        Task<string> StopTaskByInstanceId(long taskInstanceId);
    }
}
