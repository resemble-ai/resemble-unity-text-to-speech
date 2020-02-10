using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace Resemble
{
    public static class PlaceHolderAPIBridge
    {

        private static string apiUri = "https://api.soundoftext.com";
        private static List<ClipRequest> requests = new List<ClipRequest>();
        public delegate void GetClipCallback(AudioClip clip, Error error);
        public delegate void GenericCallback(string content, Error error);

        public class ClipRequest
        {
            public ClipRequestStep status;
            public CreateRequest createRequest;
            public CreateResponse createResponse;
            public DownloadResponse downloadResponse;
            public Error error;
            public AudioClip clip;
            public GetClipCallback callback;
            public DateTime lastRequest;
            public int tryCount;

            public ClipRequest(string text, GetClipCallback callback)
            {
                status = ClipRequestStep.WaitInQueue;
                createRequest = new CreateRequest() { engine = "Google", data = new CreateRequest.Data() { text = text, voice = "en-US" } };
                createResponse = new CreateResponse();
                downloadResponse = new DownloadResponse();
                lastRequest = DateTime.UtcNow;
                tryCount = 0;
                error = Error.None;
                this.callback = callback;
            }

            public void CreateClip()
            {
                status = ClipRequestStep.CreateClip;

                //Send post request
                string data = JsonUtility.ToJson(createRequest);
                UnityWebRequest request = UnityWebRequest.Put(apiUri + "/sounds", data);
                request.method = "POST";
                request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
                request.timeout = 5;
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    CompleteAsyncOperation(asyncOp, request, (string text, Error fail) =>
                    {
                    //Request failed
                    if (fail)
                            ReturnError(fail);

                    //Request success
                    else
                        {
                            lastRequest = DateTime.UtcNow;
                            tryCount = 0;

                        //Get Response
                        createResponse = JsonUtility.FromJson<CreateResponse>(text);

                        //Request rejected
                        if (!createResponse.success)
                            {
                                error = new Error(-1, createResponse.message);
                                status = ClipRequestStep.Error;
                            }
                        //Request accepted
                        else
                            {
                                status = ClipRequestStep.ClipCreated;
                            }
                        }
                    });
                };
            }

            public void GetClipStatus()
            {
                status = ClipRequestStep.GenerateAudio;

                //Send get request
                UnityWebRequest request = UnityWebRequest.Get(apiUri + "/sounds/" + createResponse.id);
                request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
                request.timeout = 5;
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    CompleteAsyncOperation(asyncOp, request, (string text, Error fail) =>
                    {
                    //Request failed
                    if (fail)
                            ReturnError(fail);

                    //Request success
                    else
                        {
                            lastRequest = DateTime.UtcNow;
                            tryCount = 0;

                        //Get response
                        downloadResponse = JsonUtility.FromJson<DownloadResponse>(text);

                            switch (downloadResponse.status)
                            {
                                case "Error":
                                    error = new Error(-1, downloadResponse.message);
                                    status = ClipRequestStep.Error;
                                    break;
                                case "Pending":
                                    break;
                                case "Done":
                                    status = ClipRequestStep.AudioGenerated;
                                    break;
                            }
                        }
                    });
                };
            }

            public void DownloadClip()
            {
                status = ClipRequestStep.DownloadAudio;

                //Send download request
                UnityWebRequest request = UnityWebRequest.Get(downloadResponse.location);
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    CompleteAsyncOperation(asyncOp, request, (string text, Error fail) =>
                    {
                    //Request failed
                    if (fail)
                            ReturnError(fail);

                    //Request success - Download file
                    else
                        {
                            string path = Resources.path.Substring(0, Resources.path.LastIndexOf("Plugins/Resemble/") + "Plugins/Resemble/".Length);
                            path = Application.dataPath.Remove(Application.dataPath.Length - 6) + path + "Temp";
                            string savePath = string.Format("{0}/{1}.mp3", path, new string(createRequest.data.text.Where(char.IsLetter).ToArray()));
                            System.IO.File.WriteAllBytes(savePath, request.downloadHandler.data);
                            savePath = savePath.Remove(0, Application.dataPath.Length - 6);
                            AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
                            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(savePath);
                            callback.Invoke(clip, Error.None);
                            status = ClipRequestStep.AudioDownloaded;
                        }
                    });
                };
            }

            public void ReturnError(Error error)
            {
                this.error = error;
                status = ClipRequestStep.Error;
                callback.Invoke(null, error);
            }
        }

        public enum ClipRequestStep
        {
            WaitInQueue,
            CreateClip,
            ClipCreated,
            GenerateAudio,
            AudioGenerated,
            DownloadAudio,
            AudioDownloaded,
            Error,
        }

        private static void ApplicationUpdate()
        {
            DateTime now = DateTime.UtcNow;
            int count = requests.Count;

            //No more requests - Stop update
            if (count == 0)
            {
                EditorApplication.update -= ApplicationUpdate;
                return;
            }

            //Execute requests
            for (int i = 0; i < Mathf.Min(count, 9); i++)
            {

                void RemoveRequest()
                {
                    requests.RemoveAt(i);
                    count--;
                    i--;
                }

                ClipRequest request = requests[i];
                switch (request.status)
                {
                    case ClipRequestStep.WaitInQueue:
                        request.CreateClip();
                        break;
                    case ClipRequestStep.ClipCreated:
                        request.GetClipStatus();
                        break;
                    case ClipRequestStep.AudioGenerated:
                        request.DownloadClip();
                        break;
                    case ClipRequestStep.AudioDownloaded:
                    case ClipRequestStep.Error:
                        RemoveRequest();
                        break;
                    default:
                        TimeSpan span = now.Subtract(request.lastRequest);
                        if (span.TotalSeconds > 15)
                        {
                            request.ReturnError(Error.Timeout);
                            RemoveRequest();
                        }
                        break;
                }
            }
        }

        public static ClipRequest GetAudioClip(string text, GetClipCallback callback)
        {
            ClipRequest request = new ClipRequest(text, callback);
            requests.Add(request);
            if (requests.Count == 1)
                EditorApplication.update += ApplicationUpdate;
            return request;
        }

        private static void CompleteAsyncOperation(AsyncOperation asyncOp, UnityWebRequest webRequest, GenericCallback callback)
        {
            if (callback == null)
                return;

            //Fail - Network error
            if (webRequest.isNetworkError)
                callback.Invoke(webRequest.downloadHandler.text, Error.NetworkError);
            //Fail - Http error
            else if (webRequest.isHttpError)
                callback.Invoke(webRequest.downloadHandler.text, Error.FromJson(webRequest.responseCode, webRequest.downloadHandler.text));
            else
            {
                //Fail - Empty reponse
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    callback.Invoke(webRequest.downloadHandler.text, Error.EmptyResponse);
                //Succes
                else if (callback != null)
                    callback.Invoke(webRequest.downloadHandler.text, Error.None);
            }
        }

    }

    #region Data structures
    [Serializable]
    public struct CreateRequest
    {
        public string engine;
        public Data data;

        [Serializable]
        public struct Data
        {
            public string text;
            public string voice;
        }
    }

    [Serializable]
    public struct CreateResponse
    {
        public bool success;
        public string id;
        public string message;
    }

    [Serializable]
    public struct DownloadResponse
    {
        public string status;
        public string location;
        public string message;
    }
    #endregion
}