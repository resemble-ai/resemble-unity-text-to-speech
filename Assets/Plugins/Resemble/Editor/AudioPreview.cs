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
#if UNITY_2020_1_OR_NEWER
            InvokeMethode("PlayPreviewClip", clip, 0, false);
#else
            InvokeMethode("PlayClip", clip, 0, false);
#endif
        }

        public static void PlayClip(AudioClip clip, int sample)
        {
#if UNITY_2020_1_OR_NEWER
            InvokeMethode("PlayPreviewClip", clip, sample, false);
#else
            InvokeMethode("PlayClip", clip, sample, false);
#endif
        }

        public static void SetClipSamplePosition(AudioClip clip, int sample)
        {
#if UNITY_2020_1_OR_NEWER
            InvokeMethode("SetPreviewClipSamplePosition", clip, sample);
#else
            InvokeMethode("SetClipSamplePosition", clip, sample);
#endif
        }

        public static int GetClipSamplePosition(AudioClip clip)
        {
#if UNITY_2020_1_OR_NEWER
            return (int) InvokeMethode("GetPreviewClipSamplePosition");
#else
            return (int) InvokeMethode("GetClipSamplePosition", clip);
#endif
        }

        public static void StopClip(AudioClip clip)
        {
#if UNITY_2020_1_OR_NEWER
            InvokeMethode("StopAllPreviewClips");
#else
            InvokeMethode("StopClip", clip);
#endif
        }

        public static bool IsClipPlaying(AudioClip clip)
        {
#if UNITY_2020_1_OR_NEWER
            return (bool) InvokeMethode("IsPreviewClipPlaying");
#else
            return (bool) InvokeMethode("IsClipPlaying", clip);
#endif
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