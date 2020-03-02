using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace Resemble
{
    public class AudioPreview
    {
        public string url;
        public bool done
        {
            get
            {
                return download.isDone;
            }
        }
        public byte[] data
        {
            get
            {
                if (!done)
                    return null;
                return download.webRequest.downloadHandler.data;
            }
        }
        public float progress
        {
            get
            {
                return download.progress;
            }
        }
        public AudioClip clip;
        public OnCompleted onDowloaded;
        public UnityWebRequestAsyncOperation download;

        public delegate void OnCompleted();

        public AudioPreview(string url)
        {
            this.url = url;
            download = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV).SendWebRequest();
            download.completed += OnWavDownloaded;
        }

        private void OnWavDownloaded(AsyncOperation obj)
        {
            clip = DownloadHandlerAudioClip.GetContent(download.webRequest);
            SceneView.RepaintAll();
            onDowloaded.Invoke();
        }

        public static void PlayClip(AudioClip clip)
        {
            InvokeMethode("PlayClip", clip, 0, false);
        }

        public static void PlayClip(AudioClip clip, int sample)
        {
            InvokeMethode("PlayClip", clip, sample, false);
        }

        public static void SetClipSamplePosition(AudioClip clip, int sample)
        {
            InvokeMethode("SetClipSamplePosition", clip, sample);
        }

        public static int GetClipSamplePosition(AudioClip clip)
        {
            return (int) InvokeMethode("GetClipSamplePosition", clip);
        }

        public static void StopClip(AudioClip clip)
        {
            InvokeMethode("StopClip", clip);
        }

        public static bool IsClipPlaying(AudioClip clip)
        {
            return (bool) InvokeMethode("IsClipPlaying", clip);
        }

        public static object InvokeMethode(string methodeName, params object[] arguments)
        {
            System.Type[] types = new System.Type[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                types[i] = arguments[i].GetType();

            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                methodeName,
                BindingFlags.Static | BindingFlags.Public,
                null, types, null
            );
            return method.Invoke(null, arguments);
        }

    }
}