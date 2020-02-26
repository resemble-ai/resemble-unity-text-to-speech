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

        public void OnGUI()
        {
            if (clip != null)
            {
                if (GUILayout.Button("Play clip"))
                {
                    PlayClip(clip);
                }
            }
        }

        public static void PlayClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] {
                typeof(AudioClip),
                typeof(int),
                typeof(bool)
                },
                null
            );

            method.Invoke(null, new object[] { clip, 0, false });
        }
    }
}