using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Domain0.Api.Client;
using DynamicData;
using Gerakul.HttpUtils.Core;
using Gerakul.HttpUtils.Json;
using Newtonsoft.Json;
using ReportServer.Desktop.Entities;
using ReportServer.Desktop.Interfaces;
using Ui.Wpf.Common;

namespace ReportServer.Desktop.Models
{
    public class CachedService : ICachedService
    {
        private ISimpleHttpClient client;
        private readonly string baseApiPath = "api/v3/";
        private readonly string authScheme = "Bearer";
        private string authToken;

        public SourceList<ApiOperTemplate> OperTemplates { get; set; }
        public SourceList<ApiRecepientGroup> RecepientGroups { get; set; }
        public SourceList<ApiTelegramChannel> TelegramChannels { get; set; }
        public SourceList<ApiSchedule> Schedules { get; set; }
        public SourceList<ApiTask> Tasks { get; set; }
        public SourceList<ApiOperation> Operations { get; set; }
        public Dictionary<string, Type> DataImporters { get; set; }
        public Dictionary<string, Type> DataExporters { get; set; }
        
        public CachedService(IAuthenticationContext context)
        {
            OperTemplates = new SourceList<ApiOperTemplate>();
            RecepientGroups = new SourceList<ApiRecepientGroup>();
            TelegramChannels = new SourceList<ApiTelegramChannel>();
            Schedules = new SourceList<ApiSchedule>();
            Tasks = new SourceList<ApiTask>();
            Operations = new SourceList<ApiOperation>();
        }

        private Task AddAuthorization(HttpRequestMessage message)
        {
            message.Headers.Authorization =
                new AuthenticationHeaderValue(authScheme, authToken);

            return Task.CompletedTask;
        }

        public async Task<bool> Connect(string serviceUri)
        {
            try
            {
                client = JsonHttpClient.Create(
                    new Uri(new Uri(serviceUri), baseApiPath).ToString(),
                    AddAuthorization);

                var result = await client.Send<string>(HttpMethod.Get, "/");

                return result.Response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                client = null;
                return false;
            }
        }

        public bool IsConnected => client != null;

        public async Task<ServiceUserRole> GetUserRole(string token)
        {
            return (await client.Send<ServiceUserRole>(HttpMethod.Get, $"/roles")).Body;
        }

        public bool Init(string token)
        {
            authToken = token;
            try
            {
                GetOperTemplates();
                RefreshData();
                return true;
            }
            catch (Exception)
            {
                return false;
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
            OperTemplates.ClearAndAddRange(client.Get<List<ApiOperTemplate>>("opertemplates/"));
        }

        public void RefreshRecipientGroups()
        {
            RecepientGroups.ClearAndAddRange(
                client.Get<List<ApiRecepientGroup>>("recipientgroups/"));
        }

        public void RefreshTelegramChannels()
        {
            TelegramChannels.ClearAndAddRange(
                client.Get<List<ApiTelegramChannel>>("telegramchannels/"));
        }

        public void RefreshSchedules()
        {
            Schedules.ClearAndAddRange(client.Get<List<ApiSchedule>>("schedules/"));
        }

        public void RefreshTasks()
        {
            Tasks.ClearAndAddRange(client.Get<List<ApiTask>>("tasks"));
        }

        public void RefreshOperations()
        {
            Operations.ClearAndAddRange(client.Get<List<ApiOperation>>("opertemplates/taskopers"));
        }

        public void RefreshData()
        {
            RefreshOperTemplates();
            RefreshRecipientGroups();
            RefreshTelegramChannels();
            RefreshSchedules();
            RefreshOperations();
            RefreshTasks();
        }

        #endregion

        public List<ApiTaskInstance> GetInstancesByTaskId(long taskId)
        {
            return client.Get<List<ApiTaskInstance>>($"tasks/{taskId}/instances");
        }

        public List<ApiOperInstance> GetOperInstancesByTaskInstanceId(long taskInstanceId)
        {
            return client.Get<List<ApiOperInstance>>($"instances/{taskInstanceId}/operinstances");
        }

        public ApiOperInstance GetFullOperInstanceById(long id)
        {
            return client.Get<ApiOperInstance>($"instances/operinstances/{id}");
        }

        public async Task<string> GetCurrentTaskViewById(long taskId) //currently doesn't work
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

        public int? CreateOrUpdateRecipientGroup(ApiRecepientGroup group)
        {
            if (group.Id == 0)
                return client.Post("recipientgroups/", group);

            client.Put($"recipientgroups/{group.Id}", group);
            return group.Id;
        }

        public int? CreateOrUpdateTelegramChannel(ApiTelegramChannel channel)
        {
            if (channel.Id == 0)
                return client.Post("telegramchannels/", channel);

            client.Put($"telegramchannels/{channel.Id}", channel);
            return channel.Id;
        }

        public int? CreateOrUpdateSchedule(ApiSchedule schedule)
        {
            if (schedule.Id == 0)
                return client.Post("schedules/", schedule);

            client.Put($"schedules/{schedule.Id}", schedule);
            return schedule.Id;
        }

        public long? CreateOrUpdateTask(ApiTask task)
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

        public void DeleteTask(long id)
        {
            client.Delete($"tasks/{id}");
        }

        public void DeleteInstance(long id)
        {
            client.Delete($"instances/taskinstances/{id}");
        }

        public async Task<string> StartTaskById(long taskId)
        {
            var apiAnswer = await client.Send<string>(HttpMethod.Get, $"tasks/run/{taskId}");
            return apiAnswer.Body;
        }

        public async Task<List<long>> GetWorkingTaskInstancesById(long taskId)
        {
            var apiAnswer = await client.Send<string>(HttpMethod.Get, $"tasks/working-{taskId}");
            return JsonConvert.DeserializeObject<List<long>>(apiAnswer.Body);
        }

        public async Task<string> StopTaskByInstanceId(long taskInstanceId)
        {
            var apiAnswer = await client
                .Send<string>(HttpMethod.Get, $"tasks/stop/{taskInstanceId}");

            var responseCode = apiAnswer.Response.StatusCode;

            if (responseCode != HttpStatusCode.OK)
                return "False";

            return apiAnswer.Body;
        }

        public void OpenPageInBrowser(string htmlPage) //maybe worker-model class for features?
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
    }
}
