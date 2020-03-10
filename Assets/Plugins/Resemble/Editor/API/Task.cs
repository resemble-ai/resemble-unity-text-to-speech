using UnityEngine;
using UnityEditor;
using Resemble.Structs;

namespace Resemble
{
    /// <summary> Request format in a queue. Also contains the status of the request. </summary>
    [System.Serializable]
    public class Task
    {
        //API data
        public string uri;
        public string data;
        public string result;
        public AudioPreview preview;
        public Callback.Simple resultProcessor;

        //Status data
        public Type type;
        public Status status;
        public double time;
        public Error error;

        public Task(string uri, string data, Callback.Simple resultProcessor, Type type)
        {
            this.uri = uri;
            this.data = data;
            this.resultProcessor = resultProcessor;
            this.type = type;
            time = EditorApplication.timeSinceStartup;
            result = "";
            error = Error.None;
            status = Status.InQueue;
        }

        public static Task DownloadTask(string uri, Callback.Download callback)
        {
            Task task = new Task(uri, "", null, Type.Download);
            task.preview = new AudioPreview(uri);
            task.preview.onDowloaded += () => 
            {
                task.status = Status.Completed;
                task.time = EditorApplication.timeSinceStartup;
                callback.Invoke(task.preview.data, Error.None);
            };
            task.error = Error.None;
            task.status = Status.Downloading;
            return task;
        }

        public static Task WaitTask()
        {
            Task task = new Task("", "", null, Type.Get);
            task.time = EditorApplication.timeSinceStartup;
            task.error = Error.None;
            task.status = Status.Processing;
            return task;
        }

        public enum Type
        {
            Get,
            Post,
            Delete,
            Download,
            Patch,
            Head,
        }

        public enum Status
        {
            InQueue,
            Processing,
            Downloading,
            Completed,
        }
    }
}