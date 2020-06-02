using System.IO;
using UnityEngine;
using UnityEditor;
using Resemble;
using Resemble.GUIEditor;
using Resemble.Structs;
using Resources = Resemble.Resources;

[System.Serializable]
public class AsyncRequest
{
    private const string tempFileName = "UnityProject - Temp ";
    private const float checkCooldown = 1.5f;       //Time in seconds between each request to know if the clip is ready to be downloaded.
    private const int waitClipTimout = 600;         //Maximum time a clip can take to be generated, after it's considered a timout.

    public static bool refreshing;

    public Task currentTask;
    public Object notificationLink;
    private PhonemesCallback phonemeCallback;
    public string saveDirectory;
    public string requestName;
    public string fileName;
    public string clipUUID;
    public double lastCheckTime;
    public double lastStateTime;
    public bool deleteClipAtEnd;
    public bool needToBeRefresh;
    public Error error;
    
    public Status status
    {
        get
        {
            return _status;
        }
        set
        {
            if (value == _status)
                return;

            switch (value)
            {
                case Status.NeedNewClipStatusRequest:
                    if (_status == Status.WaitClipStatusRequest)
                        break;
                    else
                    lastStateTime = EditorApplication.timeSinceStartup;
                    break;
                case Status.WaitClipStatusRequest:
                    if (_status == Status.NeedNewClipStatusRequest)
                        break;
                    else
                        lastStateTime = EditorApplication.timeSinceStartup;
                    break;
                default:
                    lastStateTime = EditorApplication.timeSinceStartup;
                    break;
            }
            _status = value;
        }
    }
    private Status _status;

    public bool isDone
    {
        get
        {
            switch (status)
            {
                case Status.Completed:
                case Status.Error:
                    return true;
                default:
                    return false;
            }
        }
    }
    public float downloadProgress
    {
        get
        {
            if (status != Status.Downloading)
                return 0.0f;
            return currentTask.preview.download.progress;
        }
    }

    private delegate void PhonemesCallback(PhonemesTimeStamps phonemes);


    /// <summary> Build a async request for a clip. This request handles patching, downloading and notifications. </summary>
    public static AsyncRequest Make(Clip clip)
    {
        //Build request
        AsyncRequest request = new AsyncRequest();
        request.status = Status.BuildRequest;
        string savePath = clip.GetSavePath();
        
        request.saveDirectory = Path.GetDirectoryName(savePath);
        request.fileName = Path.GetFileName(savePath);
        request.requestName = clip.speech.name + " > " + clip.clipName;
        request.clipUUID = clip.uuid;
        request.notificationLink = clip;


        //Generate place holder
        request.status = Status.GeneratePlaceHolder;
        clip.clip = request.GeneratePlaceHolder();


        //Phonemes stuff
        bool includePhonemes = clip.speech.includePhonemes;
        string voiceUUID = clip.speech.voiceUUID;
        if (includePhonemes)
            request.phonemeCallback = clip.SetPhonemesRaw;


        //Add request to the pending pool
        Resources.instance.requests.Add(request);
        EditorUtility.SetDirty(Resources.instance);
        Resemble_Window.RefreshPoolList();


        //No UUID - Create new clip        
        request.status = Status.SendDataToAPI;
        if (string.IsNullOrEmpty(request.clipUUID))
        {
            //Create new clip
            ClipPatch.Data data = new ClipPatch.Data(clip.clipName, clip.text.BuildResembleString(), voiceUUID);
            request.currentTask = APIBridge.CreateClip(data, includePhonemes, (ClipStatus status, Error error) =>
            { request.clipUUID = clip.uuid = status.id;  RegisterRequestToPool(request); });

        }
        else
        {
            ClipPatch patch = new ClipPatch(clip.clipName, clip.text.BuildResembleString(), voiceUUID, clip.speech.includePhonemes);
            //Bypass the api check for similarities.
            if (Settings.forceGeneration)
            {
                //Patch clip
                request.currentTask = APIBridge.UpdateClip(request.clipUUID, patch, (string content, Error patchError) =>
                { RegisterRequestToPool(request); });
            }

            //Check the api for similarities
            else
            {
                //Get existing clip
                APIBridge.GetClip(clip.uuid, (ResembleClip apiClip, Error error) =>
                {
                //Handle error
                if (error)
                        request.SetError(error);

                    else
                    {
                    //No changes - Download existing clip
                    if (apiClip.finished && patch.CompareContent(apiClip))
                        {
                            APIBridge.DownloadClip(apiClip.link, (byte[] data, Error downloadError) =>
                            { RegisterRequestToPool(request); });
                        }

                    //Changes - Patch existing clip
                    else
                        {
                            request.currentTask = APIBridge.UpdateClip(request.clipUUID, patch, (string content, Error patchError) =>
                            { RegisterRequestToPool(request); });
                        }
                    }
                });
            }
        }

        //Return the request
        return request;
    }

    /// <summary> Build a async request for a one-shot clip. This request handles creation, downloading, notifications and deletion. </summary>
    public static AsyncRequest Make(string body, string voice, string savePath)
    {
        //Build request
        AsyncRequest request = new AsyncRequest();
        request.status = Status.BuildRequest;
        request.saveDirectory = Path.GetDirectoryName(savePath);
        request.fileName = Path.GetFileName(savePath);
        request.deleteClipAtEnd = true;
        request.requestName = "OneShot > " + request.fileName.Remove(request.fileName.Length - 4);

        //Generate placeholder
        request.status = Status.GeneratePlaceHolder;
        request.GeneratePlaceHolder();

        //Send request
        request.status = Status.SendDataToAPI;
        ClipPatch.Data data = new ClipPatch.Data(GetTemporaryName(), body, voice);
        request.currentTask = APIBridge.CreateClip(data, false, (ClipStatus status, Error error) =>
        { request.clipUUID = status.id; RegisterRequestToPool(request);});

        //Add request to the pending pool
        Resources.instance.requests.Add(request);
        EditorUtility.SetDirty(Resources.instance);
        Resemble_Window.RefreshPoolList();

        //Return the request
        return request;
    }

    /// <summary> Build a async request for import a clip. This request handles downloading and notifications. </summary>
    public static AsyncRequest Make(string url, string savePath)
    {
        //Build request
        AsyncRequest request = new AsyncRequest();
        request.status = Status.BuildRequest;
        request.saveDirectory = Path.GetDirectoryName(savePath);
        request.fileName = Path.GetFileName(savePath);
        request.deleteClipAtEnd = false;
        request.requestName = "Import > " + request.fileName.Remove(request.fileName.Length - 4);

        //Generate placeholder
        request.status = Status.GeneratePlaceHolder;
        request.GeneratePlaceHolder();

        //Send request
        DownloadClip(request, url);

        //Register request into pool
        Resources.instance.requests.Add(request);
        EditorUtility.SetDirty(Resources.instance);
        Resemble_Window.RefreshPoolList();

        //Return the request
        return request;
    }

    /// <summary> Returns the current request running on the given UUID if it exists. </summary>
    public static AsyncRequest Get(string clipUUID)
    {
        for (int i = 0; i < Resources.instance.requests.Count; i++)
        {
            AsyncRequest request = Resources.instance.requests[i];
            if (request.clipUUID == clipUUID)
                return request;
        }
        return null;
    }


    /// <summary> Cancel the request running on the given UUID if it exists. </summary>
    public static void Cancel(string clipUUID)
    {
        for (int i = 0; i < Resources.instance.requests.Count; i++)
        {
            AsyncRequest request = Resources.instance.requests[i];
            if (request.clipUUID == clipUUID)
            {
                Resources.instance.requests.RemoveAt(i);
                EditorUtility.SetDirty(Resources.instance);
            }
        }
    }

    /// <summary> Add a request to the execution pool. </summary>
    private static void RegisterRequestToPool(AsyncRequest request)
    {
        request.status = Status.NeedNewClipStatusRequest;
        EditorUtility.SetDirty(Resources.instance);
        if (!refreshing)
        {
            EditorApplication.update += ExecutePoolRequests;
            refreshing = true;
        }
    }

    [InitializeOnLoadMethod]
    public static void RegisterRefreshEvent()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !Resources.instanceReferenced || Resources.instance.requests.Count == 0)
            return;
        refreshing = true;
        EditorApplication.update += ExecutePoolRequests;
    }

    private static void DisposeRefreshEvent()
    {
        refreshing = false;
        EditorApplication.update -= ExecutePoolRequests;
    }

    private static void ExecutePoolRequests()
    {
        double time = EditorApplication.timeSinceStartup;
        int count = Resources.instance.requests.Count;
        for (int i = 0; i < count; i++)
        {
            AsyncRequest request = Resources.instance.requests[i];
            switch (request.status)
            {
                case Status.NeedNewClipStatusRequest:
                case Status.WaitClipStatusRequest:
                    if (time - request.lastStateTime > waitClipTimout)
                    {
                        request.SetError(Error.Timeout);
                        continue;
                    }
                    else
                    {
                        ExecuteRequest(time, request);
                        break;
                    }
                case Status.Completed:
                    if (time - request.lastStateTime > 2.0)
                    {
                        Resources.instance.requests.RemoveAt(i);
                        EditorUtility.SetDirty(Resources.instance);
                        i--;
                        count--;
                    }
                    continue;
                case Status.Error:
                    continue;
                default:
                    if (time - request.lastStateTime > waitClipTimout)
                    {
                        request.SetError(Error.Timeout);
                        continue;
                    }
                    break;
            }
            ExecuteRequest(time, Resources.instance.requests[i]);
        }

        if (Resources.instance.requests.Count == 0)
            DisposeRefreshEvent();
    }

    private static void ExecuteRequest(double time, AsyncRequest request)
    {
        //Do nothing if request is waiting for API response
        if (request.status == Status.WaitClipStatusRequest)
            return;

        //The next steps are only used to send status request
        if (request.status != Status.NeedNewClipStatusRequest)
            return;

        //Force a delay in requests to avoid flooding the api
        double delta = time - request.lastCheckTime;
        if (delta < 0.0f || delta > checkCooldown)
            request.lastCheckTime = time;
        else
            return;

        //Send GetClip request
        request.currentTask = APIBridge.GetClip(request.clipUUID, (ResembleClip clip, Error error) =>
        {
            //Error
            if (error)
                request.SetError(error);

            //Receive an response
            else
            {
                //Clip is ready - Start downloading
                if (clip.finished)
                {
                    DownloadClip(request, clip.link);

                    //Get phonemes
                    if (request.phonemeCallback != null)
                        request.phonemeCallback.Invoke(clip.phoneme_timestamps);
                }

                //Clip is not ready - Mark to create a request next time
                else
                    request.status = Status.NeedNewClipStatusRequest;
            }
        });
        request.status = Status.WaitClipStatusRequest;
    }

    private static void DownloadClip(AsyncRequest request, string url)
    {
        request.status = Status.Downloading;
        request.currentTask = APIBridge.DownloadClip(url, (byte[] data, Error error) => 
        { OnDownloaded(request, data, error); });
    }

    private static void OnDownloaded(AsyncRequest request, byte[] data, Error error)
    {
        //Handle error
        if (error)
        {
            request.SetError(error);
            return;
        }

        //Download completed
        else
        {
            request.status = Status.WritingAsset;
        }

        //Write file
        string savePath = request.saveDirectory + "/" + request.fileName;
        File.WriteAllBytes(savePath, data);

        //Import asset
        savePath = Utils.LocalPath(savePath);
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
        if (request.notificationLink == null)
            request.notificationLink = AssetDatabase.LoadAssetAtPath<AudioClip>(savePath);

        //Send notification
        NotificationsPopup.Add("Download completed\n" + request.requestName, MessageType.Info, request.notificationLink);

        //Delete clip if needed
        if (!request.deleteClipAtEnd)
        {
            request.status = Status.Completed;
        }
        else
        {
            request.status = Status.SendRequestDeletion;
            request.currentTask = APIBridge.DeleteClip(request.clipUUID, (string content, Error deleteError) => 
            {
                if (deleteError)
                    request.SetError(deleteError);
                else
                    request.status = Status.Completed;
            });
        }
    }

    private static string GetTemporaryName()
    {
        return string.Concat(tempFileName, System.DateTime.UtcNow.ToBinary().ToString());
    }

    /// <summary> Generate a placeHolder wav file at saveDirectory and return it. </summary>
    public AudioClip GeneratePlaceHolder()
    {
        //Get and create directory
        string savePath = saveDirectory + "/" + fileName;
        Directory.CreateDirectory(saveDirectory);

        //Copy placeholder file
        byte[] file = File.ReadAllBytes(AssetDatabase.GetAssetPath(Resources.instance.processClip));
        File.WriteAllBytes(savePath, file);

        //Import placeholder
        savePath = Utils.LocalPath(savePath);
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(savePath);
    }

    /// <summary> Marks the request with the error status. The request will be removed from the pool. </summary>
    public void SetError(Error error)
    {
        this.error = error;
        if (error)
        {
            status = Status.Error;
            NotificationsPopup.Add(error.ToString(), MessageType.Error, notificationLink);
        }
    }

    public enum Status
    {
        BuildRequest,
        GeneratePlaceHolder,
        SendDataToAPI,
        NeedNewClipStatusRequest,
        WaitClipStatusRequest,
        Downloading,
        WritingAsset,
        SendRequestDeletion,
        Completed,
        Error,
    }

}
