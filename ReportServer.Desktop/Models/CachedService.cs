using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
using Newtonsoft.Json;
using ReactiveUI;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Models
{
    public class CachedService : ICachedService
    {
        private ISimpleHttpClient client;
        private readonly IMapper mapper;

        public ReactiveList<ApiOperTemplate> OperTemplates { get; set; }
        public ReactiveList<ApiRecepientGroup> RecepientGroups { get; set; }
        public ReactiveList<ApiTelegramChannel> TelegramChannels { get; set; }
        public ReactiveList<ApiSchedule> Schedules { get; set; }
        public ReactiveList<ApiTask> Tasks { get; set; }
        public ReactiveList<ApiTaskOper> TaskOpers { get; set; }
        public Dictionary<string, Type> DataImporters { get; set; }
        public Dictionary<string, Type> DataExporters { get; set; }

        public CachedService(IMapper mapper)
        {
            this.mapper = mapper;
            OperTemplates = new ReactiveList<ApiOperTemplate> {ChangeTrackingEnabled = true};
            RecepientGroups = new ReactiveList<ApiRecepientGroup> {ChangeTrackingEnabled = true};
            TelegramChannels = new ReactiveList<ApiTelegramChannel> {ChangeTrackingEnabled = true};
            Schedules = new ReactiveList<ApiSchedule> {ChangeTrackingEnabled = true};
            Tasks = new ReactiveList<ApiTask> {ChangeTrackingEnabled = true};
            TaskOpers = new ReactiveList<ApiTaskOper> {ChangeTrackingEnabled = true};
        }

        public bool Init(string serviceUri)
        {
            client = JsonHttpClient.Create(serviceUri + "/api/v2/");
            try
            {
                GetOperTemplates();
                RefreshData();
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void GetOperTemplates() 
        {
            DataImporters = client
                .Get<Dictionary<string, string>>("opertemplates/registeredimporters/")
                .ToDictionary(pair => pair.Key,
                    pair => Type.GetType("ReportServer.Desktop.Entities." + pair.Value));

            DataExporters = client
                .Get<Dictionary<string, string>>("opertemplates/registeredexporters/")
                .ToDictionary(pair => pair.Key,
                    pair => Type.GetType("ReportServer.Desktop.Entities." + pair.Value));
        }
        
        #region RefreshLogics

        public void RefreshOperTemplates()
        {
            OperTemplates.PublishCollection(client.Get<List<ApiOperTemplate>>("opertemplates/"));
            // .Select(rep => mapper.Map<DesktopOper>(rep)));
        }

        public void RefreshRecepientGroups()
        {
            RecepientGroups.PublishCollection(
                client.Get<List<ApiRecepientGroup>>("recepientgroups/"));
        }

        public void RefreshTelegramChannels()
        {
            TelegramChannels.PublishCollection(
                client.Get<List<ApiTelegramChannel>>("telegrams/"));
        }

        public void RefreshSchedules()
        {
            Schedules.PublishCollection(client.Get<List<ApiSchedule>>("schedules/"));
        }

        public void RefreshTasks()
        {
            Tasks.PublishCollection(client.Get<List<ApiTask>>("tasks"));
        }

        public void RefreshTaskOpers()
        {
            TaskOpers.PublishCollection(client.Get<List<ApiTaskOper>>("opertemplates/taskopers"));
        }

        public void RefreshData()
        {
            RefreshOperTemplates();
            RefreshRecepientGroups();
            RefreshTelegramChannels();
            RefreshSchedules();
            RefreshTaskOpers();
            RefreshTasks();
        }

        #endregion

        public List<ApiTaskInstance> GetInstancesByTaskId(int taskId)
        {
            return client.Get<List<ApiTaskInstance>>($"tasks/{taskId}/instances");
        }

        public List<ApiOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId)
        {
            return client.Get<List<ApiOperInstance>>($"instances/{taskInstanceId}/operinstances");
        }

        public ApiOperInstance GetFullOperInstanceById(int id)
        {
            return client.Get<ApiOperInstance>($"instances/operinstances/{id}");
        }

        public async Task<string> GetCurrentTaskViewById(int taskId) //currently doesn't work
        {
            var apiAnswer = await client
                .Send<string>(HttpMethod.Get, $"tasks/{taskId}/currentviews");

            var responseCode = apiAnswer.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                return $"Http return error {responseCode.ToString()}";

            return apiAnswer.Body;
        }

        public int? CreateOrUpdateOper(ApiOperTemplate operTemplateTemplate)
        {
            if (operTemplateTemplate.Id == 0)
                return client.Post("opertemplates/", operTemplateTemplate);

            client.Put($"opertemplates/{operTemplateTemplate.Id}", operTemplateTemplate);
            return operTemplateTemplate.Id;
        }

        public int? CreateOrUpdateRecepientGroup(ApiRecepientGroup group)
        {
            if (@group.Id == 0)
                return client.Post("recepientgroups/", group);

            client.Put($"recepientgroups/{group.Id}", group);
            return group.Id;
        }

        public int? CreateOrUpdateTelegramChannel(ApiTelegramChannel channel)
        {
            if (channel.Id == 0)
                return client.Post("telegrams/", channel);

            client.Put($"telegrams/{channel.Id}", channel);
            return channel.Id;
        }

        public int? CreateOrUpdateSchedule(ApiSchedule schedule)
        {
            if (schedule.Id == 0)
                return client.Post("schedules/", schedule);

            client.Put($"schedules/{schedule.Id}", schedule);
            return schedule.Id;
        }

        public int? CreateOrUpdateTask(ApiTask task)
        {
            if (task.Id == 0)
                return client.Post("tasks/", task);

            client.Put($"tasks/{task.Id}", task);
            return task.Id;
        }

        public void DeleteOperTemplate(int id)
        {
            client.Delete($"opertemplates/{id}");
        }

        public void DeleteSchedule(int id)
        {
            client.Delete($"schedules/{id}");
        }

        public void DeleteTask(int id)
        {
            client.Delete($"tasks/{id}");
        }

        public void DeleteInstance(int id)
        {
            client.Delete($"instances/{id}");
        }


        public void OpenPageInBrowser(string htmlPage)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}testreport.html";
            using (FileStream fstr = new FileStream(path, FileMode.Create))
            {
                byte[] bytePage = System.Text.Encoding.UTF8.GetBytes(htmlPage);
                fstr.Write(bytePage, 0, bytePage.Length);
            }

            System.Diagnostics.Process.Start(path);
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
//todo: baseaddress
