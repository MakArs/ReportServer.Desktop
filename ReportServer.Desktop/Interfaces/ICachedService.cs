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
        ReactiveList<string> DataExecutors { get; set; }
        ReactiveList<string> ViewExecutors { get; set; }

        List<ApiInstance> GetInstanceCompacts();
        List<ApiInstance> GetInstancesByTaskId(int taskId);
        ApiFullInstance GetFullInstanceById(int id);

        void RefreshSchedules();
        void RefreshRecepientGroups();
        void RefreshReports();
        void RefreshData();
        bool Init(string serviceUri);


        void DeleteTask(int id);
        void DeleteInstance(int id);

        int? CreateOrUpdateTask(ApiTask fullTask);
        int? CreateOrUpdateReport(ApiReport report);
        int? CreateOrUpdateRecepientGroup(ApiRecepientGroup group);
        int? CreateOrUpdateSchedule(ApiSchedule schedule);

        Task<string> GetCurrentTaskViewById(int taskId);
    }
}
