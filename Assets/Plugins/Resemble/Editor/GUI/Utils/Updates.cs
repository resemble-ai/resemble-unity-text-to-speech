using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Resemble
{
    public static class Updates
    {

        public static Status status = Status.unknown;
        private static UnityWebRequest downloadRequest;
        private static float version;
        private static ulong fileSize;
        private static bool abort;

        public enum Status
        {
            unknown,
            checking,
            error,
            updated,
            outdated,
        }

        public static void CheckStatus()
        {
            status = Status.checking;
            try
            {
                UnityWebRequest request;
                request = UnityWebRequest.Get("https://api.github.com/repos/resemble-ai/resemble-unity-text-to-speech/contents/Output/Version");
                request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
                request.SendWebRequest().completed += (asyncOp) =>
                {
                    try
                    {
                        string jsonFile = request.downloadHandler.text;
                        GithubFileInfos response = JsonUtility.FromJson<GithubFileInfos>(jsonFile);
                        string sVersion = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(response.content));
                        version = float.Parse(sVersion, System.Globalization.CultureInfo.InvariantCulture);

                        if (version > Settings.version)
                            status = Status.outdated;
                        else if (version == Settings.version)
                            status = Status.updated;
                    }
                    catch
                    {
                        status = Status.error;
                    }
                };
            }
            catch
            {
                status = Status.error;
            }
        }

        public static void Update()
        {
            GetSHAKey();
        }

        public static void GetLatestPluginVersion()
        {
            //Get version
            UnityWebRequest request;
            request = UnityWebRequest.Get("https://api.github.com/repos/resemble-ai/resemble-unity-text-to-speech/contents/Output/Version");
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SendWebRequest().completed += (asyncOp) => 
            {
                string jsonFile = request.downloadHandler.text;
                GithubFileInfos response = JsonUtility.FromJson<GithubFileInfos>(jsonFile);
                string sVersion = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(response.content));
                version = float.Parse(sVersion, System.Globalization.CultureInfo.InvariantCulture);

                if (version > Settings.version)
                {
                    Debug.Log("Your plugin is outdated, click below to update it.");
                    GetSHAKey();
                }
                else if (version == Settings.version)
                    Debug.Log("Your plugin is up to date.");
            };
        }

        private static void GetSHAKey()
        {
            //Get sha
            UnityWebRequest request;
            request = UnityWebRequest.Get("https://api.github.com/repos/resemble-ai/resemble-unity-text-to-speech/contents/Output/");
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SendWebRequest().completed += (asyncOp) =>
            {
                GithubDirectoryInfos directoryInfos = JsonUtility.FromJson<GithubDirectoryInfos>("{\"files\":" + request.downloadHandler.text + "}");
                for (int i = 0; i < directoryInfos.files.Length; i++)
                {
                    if (directoryInfos.files[i].name == "ResemblePlugin.unitypackage")
                    {
                        fileSize = directoryInfos.files[i].size;
                        DownloadLastPackage(directoryInfos.files[i].sha);
                    }
                }

            };
        }

        private static void DownloadLastPackage(string shaKey)
        {
            //Create path
            string url = "https://api.github.com/repos/resemble-ai/resemble-unity-text-to-speech/git/blobs/" + shaKey;
            string dir = new DirectoryInfo(Application.dataPath).Parent.FullName + "/ResemblePackages/";
            string name = "ResemblePlugin-" + version.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
            string path = dir + name + ".unitypackage";

            
            Directory.CreateDirectory(dir);

            //Make request
            downloadRequest = new UnityWebRequest(url);
            downloadRequest.method = UnityWebRequest.kHttpVerbGET;
            var handler = new DownloadHandlerFile(path, false);
            handler.removeFileOnAbort = true;
            downloadRequest.downloadHandler = handler;
            downloadRequest.SetRequestHeader("Accept", "application/vnd.github.v3.raw");
            downloadRequest.SendWebRequest().completed += (asyncOp) => { OpenFile(path); };
            abort = false;
            if (EditorUtility.DisplayCancelableProgressBar("Resemble plugin update", "Preparing to download...", 0.0f))
            {
                abort = true;
                downloadRequest.Abort();
            }
            EditorApplication.update += DownloadCallback;
        }

        private static void OpenFile(string path)
        {
            if (!abort)
                Application.OpenURL(path);
        }

        private static void DownloadCallback()
        {
            //Draw progressbar
            float t = (float) ((double)downloadRequest.downloadedBytes / (double)fileSize);
            if (EditorUtility.DisplayCancelableProgressBar("Resemble plugin update", "Downloading...   " + Mathf.RoundToInt(t * 100) + "%", t))
            {
                abort = true;
                downloadRequest.Abort();
            }

            //Unbind callback
            if (downloadRequest.isDone)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= DownloadCallback;
            }
        }

        [Serializable]
        private struct GithubDirectoryInfos
        {
            #pragma warning disable 0649
            public GithubFileInfos[] files;
        }

        [Serializable]
        private struct GithubFileInfos
        {
            #pragma warning disable 0649
            public string name;
            #pragma warning disable 0649
            public string path;
            #pragma warning disable 0649
            public string sha;
            #pragma warning disable 0649
            public ulong size;
            #pragma warning disable 0649
            public string url;
            #pragma warning disable 0649
            public string type;
            #pragma warning disable 0649
            public string content;
            #pragma warning disable 0649
            public string encoding;
        }

        [Serializable]
        private struct GithubFile
        {
            public string sha;
            public string node_id;
            public ulong size;
            public string url;

        }
    }
}