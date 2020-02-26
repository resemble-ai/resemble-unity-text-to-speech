using UnityEditor;
using Resemble.Structs;

namespace Resemble
{
    /// <summary> Request format in a queue. Also contains the status of the request. </summary>
    public class Task
    {
        public string uri;
        public string data;
        public string result;
        public double time;
        public int poolID;
        public Error error;
        public Type type;
        public Status status;
        public Callback.Simple resultProcessor;
        public AudioPreview preview;

        public Task(string uri, string data, Callback.Simple resultProcessor, Type type)
        {
            this.uri = uri;
            this.data = data;
            this.resultProcessor = resultProcessor;
            this.type = type;
            time = EditorApplication.timeSinceStartup;
            result = "";
            error = Error.None;
            status = Status.WaitToBeExecuted;
        }

        public static Task DowloadTask(string uri, Callback.Download callback)
        {
            Task task = new Task(uri, "", null, Type.Download);
            task.preview = new AudioPreview(uri);
            task.preview.onDowloaded += () => 
            {
                task.status = Status.Completed;
                task.time = EditorApplication.timeSinceStartup;
                callback.Invoke(task.preview.data, Error.None);
            };
            task.status = Status.Downloading;
            return task;
        }

        public void DownloadResult(string uri, Callback.Download callback)
        {
            type = Type.Download;
            this.uri = uri;
            preview = new AudioPreview(uri);
            time = EditorApplication.timeSinceStartup;
            preview.onDowloaded += () =>
            {
                status = Status.Completed;
                time = EditorApplication.timeSinceStartup;
                callback.Invoke(preview.data, Error.None);
            };
            status = Status.Downloading;
        }

        public enum Type
        {
            Get,
            Post,
            Delete,
            Download,
        }

        public enum Status
        {
            WaitToBeExecuted,
            WaitApiResponse,
            Downloading,
            Completed,
        }
    }
}