using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ReportServer.Desktop.Entities;

namespace ReportServer.Desktop.Interfaces
{
    public interface ICachedService
    {
        ReactiveList<DesktopReport> Reports { get; set; }
        ReactiveList<ApiSchedule> Schedules { get; set; }
        ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        ReactiveList<DesktopFullTask> Tasks { get; set; }

        List<ApiInstance> GetInstanceCompacts();
        List<ApiInstance> GetInstancesByTaskId(int taskId);
        ApiFullInstance GetFullInstanceById(int id);

        void RefreshSchedules();
        void RefreshRecepientGroups();
        void RefreshReports();
        void RefreshData();

        int CreateSchedule(ApiSchedule schedule);

        void DeleteTask(int id);
        void DeleteInstance(int id);

        int CreateOrUpdateTask(ApiTask fullTask);
        int CreateOrUpdateReport(ApiReport report);

        Task<string> GetCurrentTaskViewById(int taskId);
    }
}
