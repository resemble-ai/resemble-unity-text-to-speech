using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Resemble.Structs;

namespace Resemble
{
    public static class APIBridge
    {
        /// <summary> Max request count per seconds. </summary>
        private const int mv = 9;
        /// <summary> Timout for a request without response. </summary>
        private const float timout = 10.0f;
        /// <summary> API base url. </summary>
        private const string apiUri = "https://app.resemble.ai/api/v1/projects";

        //Execution queue for tasks (Guaranteed a maximum of tasks at the same time)
        private static Queue<Task> tasks = new Queue<Task>();
        private static List<Task> executionLoop = new List<Task>();
        private static bool receiveUpdates;

        //Callbacks for each functions type
        public delegate void GetProjectCallback(Project[] projects, Error error);
        public delegate void GetClipCallback(AudioPreview preview, Error error);
        public delegate void GetPodsCallback(ResemblePod[] pods, Error error);
        public delegate void CreateProjectCallback(ProjectStatus status, Error error);
        public delegate void ErrorCallback(long errorCode, string errorMessage);
        public delegate void GenericCallback(Delegate callback, string content, Error error);
        public delegate void SimpleCallback(string content, Error error);

        /// <summary> Request format in a queue. Also contains the status of the request. </summary>
        public class Task
        {
            public string uri;
            public string data;
            public Delegate callback;
            public GenericCallback resultProcessor;
            public string result;
            public double time;
            public Error error;
            public Type type;
            public Status status;

            public Task(string uri, string data, Delegate callback, GenericCallback resultProcessor, Type type)
            {
                this.uri = uri;
                this.data = data;
                this.callback = callback;
                this.resultProcessor = resultProcessor;
                this.type = type;
                time = EditorApplication.timeSinceStartup;
                result = "";
                error = Error.None;
                status = Status.waitToBeExecuted;
            }

            public enum Type
            {
                Get,
                Post,
            }

            public enum Status
            {
                waitToBeExecuted,
                waitApiResponse,
                completed,
            }
        }

        //Generic functions exposed to the user.
        #region Generics
        public static void SendGetRequest(string uri, GenericCallback callback)
        {
            EnqueueGet(uri, callback, SimpleRequestResult);
        }

        public static void SendPostRequest(string uri, string data, GenericCallback callback)
        {
            EnqueuePost(uri, data, callback, SimpleRequestResult);
        }

        private static void SimpleRequestResult(Delegate callback, string content, Error error)
        {
            callback.Method.Invoke(callback.Target, new object[] { content, error });
        }
        #endregion

        //Functions used by the plugin.
        #region Basics Methodes
        public static void GetProjects(GetProjectCallback callback)
        {
            Debug.Log("Get projects");
            string uri = apiUri;
            EnqueueGet(uri, callback, (Delegate c, string content, Error error) =>
            {
                Project[] projects = error ? null : Project.FromJson(content);
                c.Method.Invoke(c.Target, new object[] { projects, error });
            });
        }

        public static void CreateProject(Project project, CreateProjectCallback callback)
        {
            string uri = apiUri;
            string data = "{\"data\":" + JsonUtility.ToJson(project) + "}";

            Debug.Log(uri);
            Debug.Log(data);

            return;

            EnqueuePost(apiUri, data, callback, (Delegate c, string content, Error error) =>
            {
                ProjectStatus status = error ? null : JsonUtility.FromJson<ProjectStatus>(content);
                c.Method.Invoke(c.Target, new object[] { status, error });
            });
        }

        public static void GetAllPods(GetPodsCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips";
            EnqueueGet(uri, callback, (Delegate c, string content, Error error) =>
            {
                ResemblePod[] pods = error ? null : ResemblePod.FromJson(content);
                c.Method.Invoke(c.Target, new object[] { pods, error });
            });
        }

        public static void CreateClipSync(PostPod podData, GetClipCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips/sync";
            string data = new CreateClipRequest(podData, "high", false).Json();
            
            Debug.Log(uri);
            Debug.Log(data);
            //return;

            EnqueuePost(apiUri, data, callback, (Delegate c, string content, Error error) =>
            {
                if (error)
                {
                    c.Method.Invoke(c.Target, new object[] { null, error });
                }
                else
                {
                    //CreateClipResponse response = new CreateClipResponse(content);
                    Debug.Log(content);
                    //GetClip(response.uuid, callback);
                }
            });
        }

        public static void GetClip(string uuid, GetClipCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips/" + uuid;
            Debug.Log(uri);

            EnqueueGet(uri, callback, (Delegate c, string content, Error error) =>
            {
                if (error)
                {
                    c.Method.Invoke(c.Target, new object[] { null, error });
                }
                else
                {
                    Debug.Log(content);
                }
            });
        }

        #endregion

        /// <summary> Called at each editor frame when a task is waiting to be executed. </summary>
        public static void Update()
        {
            //Remove completed tasks from execution loop
            int exeCount = executionLoop.Count;
            double time = EditorApplication.timeSinceStartup;
            for (int i = 0; i < exeCount; i++)
            {
                Task task = executionLoop[i];

                //Tag completed tasks waiting response for more tan 10 seconds
                if (time - task.time > timout)
                {
                    task.error = Error.Timeout;
                    task.status = Task.Status.completed;
                }

                //Remove completed tasks
                if (task.status == Task.Status.completed)
                {
                    executionLoop.RemoveAt(i);
                    exeCount--;
                    i--;
                }
            }

            //There's nothing left to execute, end of updates.
            if (tasks.Count == 0 && executionLoop.Count == 0)
            {
                EditorApplication.update -= Update;
                receiveUpdates = false;
                return;
            }

            //Send tasks from waiting queue to execution loop queue if fit.
            if (executionLoop.Count < mv)
            {
                int count = Mathf.Clamp(mv - executionLoop.Count, 0, tasks.Count);
                for (int i = 0; i < count; i++)
                    executionLoop.Add(tasks.Dequeue());
            }

            //Execute tasks in execution loop queue. 
            for (int i = 0; i < executionLoop.Count; i++)
            {
                Task task = executionLoop[i];
                if (task.status == Task.Status.waitToBeExecuted)
                {
                    switch (task.type)
                    {
                        case Task.Type.Get:
                            SendGetRequest(task);
                            break;
                        case Task.Type.Post:
                            SendPostRequest(task);
                            break;
                    }
                    task.status = Task.Status.waitApiResponse;
                    task.time = EditorApplication.timeSinceStartup;
                }
            }
        }

        /// <summary> Enqueue a Get web request to the task list. This task will be executed as soon as possible. </summary>
        public static void EnqueueGet(string uri, Delegate callback, GenericCallback resultProcessor)
        {
            tasks.Enqueue(new Task(uri, null, callback, resultProcessor, Task.Type.Get));
            if (!receiveUpdates)
            {
                EditorApplication.update += Update;
                receiveUpdates = true;
            }
        }

        /// <summary> Enqueue a Pos web request to the task list. This task will be executed as soon as possible. </summary>
        public static void EnqueuePost(string uri, string data, Delegate callback, GenericCallback resultProcessor)
        {
            tasks.Enqueue(new Task(uri, data, callback, resultProcessor, Task.Type.Post));
            if (!receiveUpdates)
            {
                EditorApplication.update += Update;
                receiveUpdates = true;
            }
        }

        /// <summary> Send a Get web request now. Call the callback with the response processed by the resultProcessor. </summary>
        private static void SendGetRequest(Task task)
        {
            UnityWebRequest request = UnityWebRequest.Get(task.uri);
            request.SetRequestHeader("Authorization", string.Format("Token token=\"{0}\"", Settings.token));
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SendWebRequest().completed += (asyncOp) => { CompleteAsyncOperation(asyncOp, request, task); };
        }

        /// <summary> Send a Post web request now. Call the callback with the response processed by the resultProcessor. </summary>
        private static void SendPostRequest(Task task)
        {
            //https://forum.unity.com/threads/posting-raw-json-into-unitywebrequest.397871/
            UnityWebRequest request = UnityWebRequest.Put(task.uri, task.data);
            request.method = "POST";
            request.SetRequestHeader("Authorization", string.Format("Token token=\"{0}\"", Settings.token));
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SendWebRequest().completed += (asyncOp) => { CompleteAsyncOperation(asyncOp, request, task); };
        }

        /// <summary> Responses from requests are received here. The content of the response is process by the resultProcessor and then the callback is executed with the result. </summary>
        private static void CompleteAsyncOperation(AsyncOperation asyncOp, UnityWebRequest webRequest, Task task)
        {
            task.status = Task.Status.completed;

            if (task.callback == null)
                return;

            //Fail - Network error
            if (webRequest.isNetworkError)
                task.resultProcessor.Invoke(task.callback, webRequest.downloadHandler.text, Error.NetworkError);
            //Fail - Http error
            else if (webRequest.isHttpError)
                task.resultProcessor.Invoke(task.callback, webRequest.downloadHandler.text, Error.FromJson(webRequest.responseCode, webRequest.downloadHandler.text));
            else
            {
                //Fail - Empty reponse
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    task.resultProcessor.Invoke(task.callback, webRequest.downloadHandler.text, Error.EmptyResponse);
                //Succes
                else if (task.callback != null)
                    task.resultProcessor.Invoke(task.callback, webRequest.downloadHandler.text, Error.None);
            }
        }
    }
}