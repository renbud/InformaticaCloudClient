using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace InformaticaCloudClient
{
    public static class IodClientMethods
    {
        public static Dictionary<string, string> TaskTypes
            = new Dictionary<string, string>()
            {
                ["AVS"] = "Contact Validation task",
                ["DMASK"] = " Data Masking task",
                ["DNB_TASK"] = "D & B360 task.",
                ["DNB_WORKFLOW"] = "D & B360 workflow.",
                ["WORKFLOW"] = "Workflow." ,
                ["DQA"] = "Data Assessment task.",
                ["DRS"] = "Data Replication task.",
                ["DSS"] = "Data Synchronization task.",
                ["MTT"] = "Mapping Configuration task.",
                ["PCS"] = "PowerCenter task"
            };

        public static async Task<IodClientSession> DoLogin(string user, string password)
        {
            const string MethodUrl = "ma/api/v2/user/login";
            const string ContentType = "application/json"; // use json both directions
            IodClientSession session;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(IodClientSession.BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestContent = new
                {
                    @type = "login",
                    username = user,
                    password = password
                };
                var requestMessage = MakeRequestMessage(null, requestContent, ContentType, MethodUrl);

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                #if DEBUG
                Console.WriteLine(resp.ToString());
                #endif
                if (resp.IsSuccessStatusCode)
                {
                    string content = await resp.Content.ReadAsStringAsync();
                    IodSessionStub contentObject = JsonConvert.DeserializeObject<IodSessionStub>(content);
                    session = new IodClientSession(contentObject.serverUrl, contentObject.icSessionId, true);
                }
                else
                {
                    throw new HttpResponseException(resp);
                }
                return session;
            }
        }


        public static async Task<bool> DoLogout(IodClientSession session)
        {
            const string MethodUrl = "api/v2/user/logout";
            const string ContentType = "application/json"; // use json both directions

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestContent = new
                {
                    @type = "logout"
                };
                var requestMessage = MakeRequestMessage(session, requestContent, ContentType, MethodUrl);

                HttpResponseMessage resp = await client.SendAsync(requestMessage);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpResponseException(resp);
                }
                return resp.IsSuccessStatusCode;
            }
        }

        public static async Task<string> DoGetTaskId(IodClientSession session, string taskName, string taskType)
        {
            const string MethodUrl = "api/v2/task?type={type}";
            const string ContentType = "application/json"; // use json both directions

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestMessage = MakeRequestMessage(session, null, ContentType, MethodUrl.Replace("{type}", taskType));

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    // successful request returns a list of Task as Json
                    string content = await resp.Content.ReadAsStringAsync();
                    var contentObject = JsonConvert.DeserializeObject<List<Task>>(content);
                    var theTask = contentObject
                        .Where(task => task.name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();
                    if (theTask == null)
                    {
                        throw new ApplicationException($"task: {taskName} of type: {taskType} was not found");
                    }
                    return theTask.id;
                }
                else
                {
                    throw new HttpResponseException(resp);
                }
            }
        }

        public static async Task<bool> DoStopTask(IodClientSession session, string taskId, string taskType)
        {
            const string MethodUrl = "api/v2/job/stop";
            bool ok =  await DoStartOrStopTask(session, taskId, taskType, MethodUrl);
            System.Threading.Thread.Sleep(15000);
            return ok;
        }

        public static async Task<bool> DoStartTask(IodClientSession session, string taskId, string taskType)
        {
            const string MethodUrl = "api/v2/job";
            return await DoStartOrStopTask(session, taskId, taskType, MethodUrl);
        }

        private static async Task<bool> DoStartOrStopTask(IodClientSession session, string taskId, string taskType, string methodUrl)
        {
            const string ContentType = "application/json"; // use json both directions

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestContent = new
                {
                    @type = "job",
                    taskId = taskId,
                    taskType = taskType
                };
                var requestMessage = MakeRequestMessage(session, requestContent, ContentType, methodUrl);

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new HttpResponseException(resp);
                }
                return resp.IsSuccessStatusCode;
            }
        }


        public static async Task<bool> DoWaitTask(IodClientSession session, string taskId, string taskType)
        {
            const string MethodUrl = "api/v2/activity/activityMonitor?details=true";
            const string ContentType = "application/json"; // use json both directions
            DateTime startTime = DateTime.Now;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));


                bool isRunning = true;
                while (isRunning)
                {
                    // keep checking the activity monior
                    var requestMessage = MakeRequestMessage(session, null, ContentType, MethodUrl);
                    HttpResponseMessage resp = await client.SendAsync(requestMessage);
                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new HttpResponseException(resp);
                    }

                    string content = await resp.Content.ReadAsStringAsync();
                    var contentObject = JsonConvert.DeserializeObject<List<ActivityMonitorEntry>>(content);
                    // its running if the monitor contains an entry with the taskId and taskType
                    isRunning = contentObject.Exists(entry =>
                        entry.taskId.Equals(taskId,StringComparison.InvariantCultureIgnoreCase) &&
                        entry.type.Equals(taskType, StringComparison.InvariantCultureIgnoreCase));

                    if (isRunning)
                    {
                        System.Threading.Thread.Sleep(15000);
                        if (startTime.AddMinutes(120).CompareTo(DateTime.Now) < 0)
                            return false;   // timed out after 2 hours
                    }
                }
                return true;    // its no longer running
            }
        }

        public static async Task<IEnumerable<ActivityLogEntry>> DoGetActivityLog(IodClientSession session, string taskId, int rowLimit=800)
        {
            const string MethodUrl = "api/v2/activity/activityLog?taskId={taskId}";//&rowLimit={rowLimit}";
            const string ContentType = "application/json"; // use json both directions
 
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestMessage = MakeRequestMessage(
                    session,
                    null,
                    ContentType,
                    MethodUrl.Replace("{rowLimit}", rowLimit.ToString()).Replace("{taskId}", taskId)
                    );

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    string content = await resp.Content.ReadAsStringAsync();
                    var contentObject = JsonConvert.DeserializeObject<List<ActivityLogEntry>>(content);
                    return contentObject;
                }
                else
                {
                    throw new HttpResponseException(resp);
                }
            }
        }

        public static async Task<IEnumerable<ActivityMonitorEntry>> DoGetActivityMonitor(IodClientSession session, string taskId, string taskType)
        {
            const string MethodUrl = "api/v2/activity/activityMonitor?details=true";
            const string ContentType = "application/json"; // use json both directions

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestMessage = MakeRequestMessage(
                    session,
                    null,
                    ContentType,
                    MethodUrl
                    );

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    string content = await resp.Content.ReadAsStringAsync();
                    var contentObject = JsonConvert.DeserializeObject<List<ActivityMonitorEntry>>(content);
                    return contentObject.Where(entry =>
                        entry.taskId.Equals(taskId, StringComparison.InvariantCultureIgnoreCase) &&
                        entry.type.Equals(taskType, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    throw new HttpResponseException(resp);
                }
            }
        }

        /// <summary>
        /// This is supposed to return the error log as a stream
        /// but it does not work unforunately whenever I tested it results in 404 not found
        /// </summary>
        /// <param name="session"></param>
        /// <param name="logId"></param>
        /// <returns></returns>
        [System.Obsolete("Does not work")]
        public static async Task<System.IO.Stream> DoGetErrorLog(IodClientSession session, string logId)
        {
            const string MethodUrl = "api/v2/activity/errorLog/id?{id}";
            const string ContentTypeAccept = "application/json"; 
            const string ContentTypeRequest = "application/json";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentTypeAccept));

                var requestMessage = MakeRequestMessage(
                    session,
                    null,
                    ContentTypeRequest,
                    MethodUrl.Replace("{id}", logId)
                    );

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {

                    var content = await resp.Content.ReadAsStreamAsync();
                    return content;
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<IEnumerable<ActivityMonitorEntry>> DoGetActivityMonitor(IodClientSession session)
        {
            const string MethodUrl = "api/v2/activity/activityMonitor?details=true";
            const string ContentType = "application/json"; // use json both directions

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(session.serverUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

                var requestMessage = MakeRequestMessage(
                    session,
                    null,
                    ContentType,
                    MethodUrl
                    );

                HttpResponseMessage resp = await client.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    string content = await resp.Content.ReadAsStringAsync();
                    var contentObject = JsonConvert.DeserializeObject<List<ActivityMonitorEntry>>(content);
                    return contentObject;
                }
                else
                {
                    throw new HttpResponseException(resp);
                }
            }
        }

        /// <summary>
        /// Construct the message, which can be
        /// a) Either GET or POST depending on requestContent being null or not
        /// b) Either a login request to session.serverUrl  or a session based requestto IodSession.BaseAddress
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestContent"></param>
        /// <param name="contentType"></param>
        /// <param name="methodUrl"></param>
        /// <returns></returns>
        private static HttpRequestMessage MakeRequestMessage(IodClientSession session, object requestContent, string contentType, string methodUrl)
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.Version = HttpVersion.Version10;
            var uri1 = session == null ? IodClientSession.BaseAddress : session.serverUrl;
            if (!uri1.EndsWith("/"))
                uri1 = uri1 + "/";
            uri1 = uri1 + methodUrl;
            requestMessage.RequestUri = new Uri(uri1);
            if (session != null)
                requestMessage.Headers.Add("icSessionId", session.icSessionId);


            if (requestContent == null)
            {
                requestMessage.Method = HttpMethod.Get;
            }
            else
            {
                requestMessage.Method = HttpMethod.Post;
                string json = JsonConvert.SerializeObject(requestContent, Formatting.Indented);
                json = json.Replace("\"type\":", "\"@type\":");// hack conversion to what IOD wants "@type": "xxxx" job/login etc

                var httpContent = new StringContent(json, Encoding.UTF8, contentType);
                requestMessage.Content = httpContent;
            }
            return requestMessage;
        }
    }

    public class HttpResponseException : ApplicationException
    {
        public HttpResponseException(HttpResponseMessage response) :
            base(response.RequestMessage.RequestUri +
                "\ncode:" + ((int)response.StatusCode).ToString() + ": " +
                response.ReasonPhrase)
        {

        }
    }
}

