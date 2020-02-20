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
        public delegate void GenericCallback(string content, Error error);

        /// <summary> Request format in a queue. Also contains the status of the request. </summary>
        public class Task
        {
            public string uri;
            public string data;
            public GenericCallback resultProcessor;
            public string result;
            public double time;
            public Error error;
            public Type type;
            public Status status;

            public Task(string uri, string data, GenericCallback resultProcessor, Type type)
            {
                this.uri = uri;
                this.data = data;
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
                Delete,
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
            EnqueueGet(uri, (string content, Error error) =>
            {
                callback.Invoke(content, error);
            });
        }

        public static void SendPostRequest(string uri, string data, GenericCallback callback)
        {
            EnqueuePost(uri, data, (string content, Error error) =>
            {
                callback.Invoke(content, error);
            });
        }
        #endregion

        //Functions used by the plugin.
        #region Basics Methodes
        public static void GetProjects(GetProjectCallback callback)
        {
            string uri = apiUri;
            EnqueueGet(uri, (string content, Error error) =>
            {
                Project[] projects = error ? null : Project.FromJson(content);
                callback.Method.Invoke(callback.Target, new object[] { projects, error });
            });
        }

        public static void GetProject(string uuid)
        {
            string uri = apiUri + "/" + uuid;
            EnqueueGet(uri, (string content, Error error) =>
            {
                Debug.Log(content);
            });
        }

        public static void DeleteProject(Project project)
        {
            string uri = apiUri + "/" + project.uuid;
            EnqueueDelete(uri, (string content, Error error) => {});
        }

        public static void CreateProject(Project project, CreateProjectCallback callback)
        {
            string uri = apiUri;
            string data = "{\"data\":" + JsonUtility.ToJson(project) + "}";

            EnqueuePost(uri, data, (string content, Error error) =>
            {
                ProjectStatus status = error ? null : JsonUtility.FromJson<ProjectStatus>(content);
                callback.Method.Invoke(callback.Target, new object[] { status, error });
            });
        }

        public static void GetAllPods(GetPodsCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips";
            EnqueueGet(uri, (string content, Error error) =>
            {
                ResemblePod[] pods = error ? null : ResemblePod.FromJson(content);
                callback.Method.Invoke(callback.Target, new object[] { pods, error });
            });
        }

        public static Task CreateClipSync(CreateClipData podData, GetClipCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips/sync";
            string data = new CreateClipRequest(podData, "x-high", false).Json();

            return EnqueuePost(uri, data, (string content, Error error) =>
            {
                if (error)
                {
                    callback.Method.Invoke(callback.Target, new object[] { null, error });
                }
                else
                {
                    callback.Invoke(new AudioPreview(content), Error.None);
                }
            });
        }

        public static void GetClip(string uuid, GetClipCallback callback)
        {
            string uri = apiUri + "/" + Settings.project.uuid + "/clips/" + uuid;
            Debug.Log(uri);

            EnqueueGet(uri, (string content, Error error) =>
            {
                if (error)
                {
                    callback.Method.Invoke(callback.Target, new object[] { null, error });
                }
                else
                {
                    Debug.Log(content);
                }
            });
        }

        #endregion

        #region Request execution
        /// <summary> Called at each editor frame when a task is waiting to be executed. </summary>
        public static void Update()
        {
            //Update progress bar
            int taskCount = tasks.Count + executionLoop.Count;
            float progress = 0;
            for (int i = 0; i < executionLoop.Count; i++)
                progress += ((int)executionLoop[i].status) / 2.0f;
            progress /= executionLoop.Count;
            string barText = "Remaining clips : " + taskCount;
            EditorProgressBar.Display(barText, progress);

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
                EditorProgressBar.Clear();
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
                        case Task.Type.Delete:
                            SendDeleteRequest(task);
                            break;
                    }
                    task.status = Task.Status.waitApiResponse;
                    task.time = EditorApplication.timeSinceStartup;
                }
            }
        }

        /// <summary> Enqueue a Get web request to the task list. This task will be executed as soon as possible. </summary>
        public static Task EnqueueGet(string uri, GenericCallback resultProcessor)
        {
            Task task = new Task(uri, null, resultProcessor, Task.Type.Get);
            tasks.Enqueue(task);
            if (!receiveUpdates)
            {
                EditorApplication.update += Update;
                receiveUpdates = true;
            }
            return task;
        }

        /// <summary> Enqueue a Pos web request to the task list. This task will be executed as soon as possible. </summary>
        public static Task EnqueuePost(string uri, string data, GenericCallback resultProcessor)
        {
            Task task = new Task(uri, data, resultProcessor, Task.Type.Post);
            tasks.Enqueue(task);
            if (!receiveUpdates)
            {
                EditorApplication.update += Update;
                receiveUpdates = true;
            }
            return task;
        }

        /// <summary> Enqueue a Delete web request to the task list. This task will be executed as soon as possible. </summary>
        public static void EnqueueDelete(string uri, GenericCallback resultProcessor)
        {
            tasks.Enqueue(new Task(uri, null, resultProcessor, Task.Type.Delete));
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

        /// <summary> Send a Delete web request now. Call the callback with the response processed by the resultProcessor. </summary>
        private static void SendDeleteRequest(Task task)
        {
            UnityWebRequest request = UnityWebRequest.Delete(task.uri);
            request.SetRequestHeader("Authorization", string.Format("Token token=\"{0}\"", Settings.token));
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SendWebRequest().completed += (asyncOp) => { CompleteAsyncOperation(asyncOp, request, task); };
        }

        /// <summary> Responses from requests are received here. The content of the response is process by the resultProcessor and then the callback is executed with the result. </summary>
        private static void CompleteAsyncOperation(AsyncOperation asyncOp, UnityWebRequest webRequest, Task task)
        {
            task.status = Task.Status.completed;

            if (task.resultProcessor == null || task.type == Task.Type.Delete)
                return;

            //Fail - Network error
            if (webRequest.isNetworkError)
                task.resultProcessor.Invoke(webRequest.downloadHandler.text, Error.NetworkError);
            //Fail - Http error
            else if (webRequest.isHttpError)
                task.resultProcessor.Invoke(webRequest.downloadHandler.text, Error.FromJson(webRequest.responseCode, webRequest.downloadHandler.text));
            else
            {
                //Fail - Empty reponse
                if (webRequest.downloadHandler == null || string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    task.resultProcessor.Invoke(null, Error.EmptyResponse);
                //Succes
                else
                    task.resultProcessor.Invoke(webRequest.downloadHandler.text, Error.None);
            }
        }
        #endregion
    }
}