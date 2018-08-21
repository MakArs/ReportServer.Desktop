using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using ReportServer.Desktop.Interfaces;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
using Newtonsoft.Json;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Model
{
    public class CachedService : ICachedService
    {
        private readonly ISimpleHttpClient client;
        private readonly IMapper mapper;

        public ReactiveList<DesktopReport> Reports { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<DesktopFullTask> Tasks { get; set; }
        public ReactiveList<string> DataExecutors { get; set; }
        public ReactiveList<string> ViewExecutors { get; set; }

        public CachedService(IMapper mapper)
        {
            client = JsonHttpClient.Create("http://localhost:12345/");
            this.mapper = mapper;

            Reports = new ReactiveList<DesktopReport>();
            Schedules = new ReactiveList<ApiSchedule>();
            RecepientGroups = new ReactiveList<ApiRecepientGroup>();
            Tasks = new ReactiveList<DesktopFullTask>();
            DataExecutors = new ReactiveList<string>();
            ViewExecutors = new ReactiveList<string>();

            GetExecutors();
            RefreshData();
        }

        #region RefreshLogics

        public void RefreshSchedules()
        {
            Schedules.PublishCollection(client.Get<List<ApiSchedule>>("/api/v1/schedules/"));
        }

        public void RefreshReports()
        {
            Reports.Clear();
            Reports.PublishCollection(client.Get<List<ApiReport>>("/api/v1/reports/")
                .Select(rep => mapper.Map<DesktopReport>(rep)));
        }

        public void RefreshRecepientGroups()
        {
            RecepientGroups.PublishCollection(
                client.Get<List<ApiRecepientGroup>>("/api/v1/recepientgroups/"));
        }

        public void RefreshTasks()
        {
            var deskTasks = client.Get<List<ApiTask>>("/api/v1/tasks")
                .Select(apiTask => mapper.Map<DesktopFullTask>(apiTask)).ToList();

            deskTasks = deskTasks.Select(deskTask => mapper.Map(Reports
                    .FirstOrDefault(rep => rep.Id == deskTask.ReportId), deskTask))
                .Where(t => t != null)
                .ToList();

            foreach (var deskTask in deskTasks)
            {
                deskTask.Schedule = Schedules.FirstOrDefault(sch => sch.Id == deskTask.ScheduleId)
                    ?.Schedule;
                deskTask.RecepientGroup = RecepientGroups
                    .FirstOrDefault(rcg => rcg.Id == deskTask.RecepientGroupId)
                    ?.Name;
            }

            Tasks.PublishCollection(deskTasks);
        }

        public void RefreshData()
        {
            RefreshReports();
            RefreshRecepientGroups();
            RefreshSchedules();
            RefreshTasks();
        }

        #endregion

        public List<ApiInstance> GetInstancesByTaskId(int taskId)
        {
            return client.Get<List<ApiInstance>>($"/api/v1/tasks/{taskId}/instances");
        }

        public List<ApiInstance> GetInstanceCompacts()
        {
            return client.Get<List<ApiInstance>>("/api/v1/instances");
        }

        public ApiFullInstance GetFullInstanceById(int id)
        {
            return client.Get<ApiFullInstance>($"/api/v1/instances/{id}");
        }

        private void GetExecutors()
        {
            DataExecutors.PublishCollection(client
                .Get<List<string>>("/api/v1/reports/customdataexecutors/"));
            ViewExecutors.PublishCollection(client
                .Get<List<string>>("/api/v1/reports/customviewexecutors/"));
        }

        public async Task<string> GetCurrentTaskViewById(int taskId)
        {

            var apiAnswer = await client
                .Send<string>(HttpMethod.Get, $"/api/v1/tasks/{taskId}/currentviews");

            var responseCode = apiAnswer.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                return $"Http return error {responseCode.ToString()}";

            return apiAnswer.Body;
        }

        public int CreateSchedule(ApiSchedule schedule)
        {
            return client.Post("/schedules", schedule);
        }

        public void DeleteTask(int id)
        {
            client.Delete($"/api/v1/tasks/{id}");
        }

        public void DeleteInstance(int id)
        {
            client.Delete($"/api/v1/instances/{id}");
        }

        public int CreateOrUpdateTask(ApiTask task)
        {
            if (task.Id == 0)
                return client.Post("/api/v1/tasks/", task);

            client.Put($"/api/v1/tasks/{task.Id}", task);
            return task.Id;
        }

        public int CreateOrUpdateReport(ApiReport report)
        {
            if (report.Id == 0)
                return client.Post("/api/v1/reports/", report);

            client.Put($"/api/v1/reports/{report.Id}", report);
            return report.Id;
        }

    }

    public static class JsonHttpClientTimeExtension
    {
        public static T Get<T>(this ISimpleHttpClient client, string path)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Get, path));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");

            return JsonConvert.DeserializeObject<T>(task.Result.Result.Body);
        }

        public static void Delete(this ISimpleHttpClient client, string url)
        {
            var task = Task.Factory.StartNew(() => client.Send<string>(HttpMethod.Delete, url));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");
        }

        public static int Post<T>(this ISimpleHttpClient client, string path, T postBody)
        {
            var task = Task.Factory.StartNew(() =>
                client.Send<string>(HttpMethod.Post, path, null, postBody));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");

            return JsonConvert.DeserializeObject<int>(task.Result.Result.Body);
        }

        public static void Put<T>(this ISimpleHttpClient client, string path, T postBody)
        {
            var task = Task.Factory.StartNew(() =>
                client.Send<string>(HttpMethod.Put, path, null, postBody));
            task.Wait();

            var responseCode = task.Result.Result.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                throw new Exception($"Http return error {responseCode.ToString()}");
        }

        public static T CreateClone<T>(this T someObj)
        {
            string tem = JsonConvert.SerializeObject(someObj);
            return JsonConvert.DeserializeObject<T>(tem);
        }

    }
}

//todo: delete hotkey logic changed?
//todo: delete report
//todo: baseaddress
//todo: ui improvements
//todo: recepgroups,schedules,telegramchannels functional
