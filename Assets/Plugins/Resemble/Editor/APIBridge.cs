using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class APIBridge
{
    private static string apiUri = "https://app.resemble.ai/api/v1/projects";
    private static Queue<Task> tasks = new Queue<Task>();

    public delegate void taskAction();
    public delegate void GetProjectCallback(Project[] projects, Error error);
    public delegate void GetClipCallback(AudioPreview preview, Error error);
    public delegate void GetPodsCallback(ResemblePod[] pods, Error error);
    public delegate void CreateProjectCallback(ProjectStatus status, Error error);
    public delegate void ErrorCallback(long errorCode, string errorMessage);
    public delegate void GenericCallback(Delegate callback, string content, Error error);
    public delegate void SimpleCallback(string content, Error error);

    public struct Task
    {
        public taskAction action;
    }

    #region Generics
    public static void SendGetRequest(string uri, GenericCallback callback)
    {
        SendGetRequest(uri, callback, SimpleRequestResult);
    }

    public static void SendPostRequest(string uri, string data, GenericCallback callback)
    {
        SendPostRequest(uri, data, callback, SimpleRequestResult);
    }

    private static void SimpleRequestResult(Delegate callback, string content, Error error)
    {
        callback.Method.Invoke(callback.Target, new object[] { content, error });
    }
    #endregion

    #region Basics Methodes
    public static void GetProjects(GetProjectCallback callback)
    {
        string uri = apiUri;
        SendGetRequest(uri, callback, (Delegate c, string content, Error error) =>
        {
            Project[] projects = error ? null : Project.FromJson(content);
            c.Method.Invoke(c.Target, new object[] { projects, error });
        });
    }

    public static void CreateProject(Project project, CreateProjectCallback callback)
    {
        string uri = apiUri;
        string data = "{\"data\":" + JsonUtility.ToJson(project) + "}";
        SendPostRequest(apiUri, data, callback, (Delegate c, string content, Error error) =>
        {
            ProjectStatus status = error ? null : JsonUtility.FromJson<ProjectStatus>(content);
            c.Method.Invoke(c.Target, new object[] { status, error });
        });
    }

    public static void GetAllPods(GetPodsCallback callback)
    {
        string uri = apiUri + "/" + Resemble_Settings.project.uuid + "/clips";
        SendGetRequest(uri, callback, (Delegate c, string content, Error error) =>
        {
            ResemblePod[] pods = error ? null : ResemblePod.FromJson(content);
            c.Method.Invoke(c.Target, new object[] { pods, error });
        });
    }

    public static void CreateClipSync(PostPod podData, GetClipCallback callback)
    {
        string uri = apiUri + "/" + Resemble_Settings.project.uuid + "/clips/sync";
        string data = "{\"data\":" + JsonUtility.ToJson(podData) + "}";
        Debug.Log(data);
        SendPostRequest(apiUri, data, callback, (Delegate c, string content, Error error) =>
        {
            Debug.Log(content);
            //ProjectStatus status = error ? null : JsonUtility.FromJson<ProjectStatus>(content);
            //c.Method.Invoke(c.Target, new object[] { status, error });
        });
    }

    #endregion

    public static void SendGetRequest(string uri, Delegate callback, GenericCallback resultMethode)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.SetRequestHeader("Authorization", string.Format("Token token=\"{0}\"", Resemble_Settings.token));
        request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
        request.SendWebRequest().completed += (asyncOp) => { CompleteAsyncOperation(asyncOp, request, callback, resultMethode); };
    }

    public static void SendPostRequest(string uri, string data, Delegate callback, GenericCallback resultMethode)
    {
        //https://forum.unity.com/threads/posting-raw-json-into-unitywebrequest.397871/
        UnityWebRequest request = UnityWebRequest.Put(uri, data);
        request.method = "POST";
        request.SetRequestHeader("Authorization", string.Format("Token token=\"{0}\"", Resemble_Settings.token));
        request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
        request.SendWebRequest().completed += (asyncOp) => { CompleteAsyncOperation(asyncOp, request, callback, resultMethode); };
    }

    private static void CompleteAsyncOperation(AsyncOperation asyncOp, UnityWebRequest webRequest, Delegate callback, GenericCallback resultMethode)
    {
        if (callback == null)
            return;

        //Fail - Network error
        if (webRequest.isNetworkError)
            resultMethode.Invoke(callback, webRequest.downloadHandler.text, Error.NetworkError);
        //Fail - Http error
        else if (webRequest.isHttpError)
            resultMethode.Invoke(callback, webRequest.downloadHandler.text, Error.FromJson(webRequest.responseCode, webRequest.downloadHandler.text));
        else
        {
            //Fail - Empty reponse
            if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                resultMethode.Invoke(callback, webRequest.downloadHandler.text, Error.EmptyReponse);
            //Succes
            else if (callback != null)
                resultMethode.Invoke(callback, webRequest.downloadHandler.text, Error.None);
        }
    }
}
