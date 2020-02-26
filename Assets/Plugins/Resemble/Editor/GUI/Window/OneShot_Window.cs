using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public partial class Resemble_Window
    {
        public static AudioPreview preview;
        private static Task task;
        private static Text_Editor drawer = new Text_Editor();
        private static bool drawProgressBar;
        private static Text text
        {
            get
            {
                return Resources.instance.text;
            }
        }

        //GUI stuff
        private Dropdown fileDropDown;
        private Dropdown editDropDown;
        private Dropdown settingsDropDown;

        public void DrawOneShotGUI()
        {
            //Check connection
            if (!Settings.haveProject)
                Utils.ConnectionRequireMessage();
            EditorGUI.BeginDisabledGroup(!Settings.haveProject);

            //Purpose box
            EditorGUILayout.HelpBox("The audio files generated here will not be saved in the Resemble project.", MessageType.None);


            //Update progress bar if exist
            if (drawProgressBar)
            {
                EditorUtility.DisplayProgressBar("Resemble", "Download clip...", preview.download.progress);
                if (preview.download.isDone)
                {
                    drawProgressBar = false;
                    EditorUtility.ClearProgressBar();
                }
            }

            //Tags
            drawer.DrawTagsBtnsLayout(!Settings.haveProject);

            //Draw text area
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 300).Shrink(10);
            drawer.DrawTextArea(rect, Settings.haveProject);
            if (drawer.dirty)
                EditorUtility.SetDirty(Resources.instance);

            //Draw char count progress bar
            rect.Set(rect.x, rect.y + rect.height, rect.width, 16);
            drawer.DrawCharCountBar(rect);
            GUILayout.Space(20);

            if (GUILayout.Button("Generate audio file"))
            {
                CancelCurrentTask();
                Generate();
            }

            if (preview != null && preview.clip != null && GUILayout.Button("Play clip"))
            {
                AudioPreview.PlayClip(preview.clip);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void SaveClipFile()
        {
            //Open file browser
            string path = EditorUtility.SaveFilePanel("Save wav file", Application.dataPath, "OneShotClip", "wav");

            //Cancel by user
            if (string.IsNullOrEmpty(path))
                return;

            //Save and import file
            System.IO.File.WriteAllBytes(path, preview.download.webRequest.downloadHandler.data);
            path = path.Remove(0, Application.dataPath.Length - 6);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            //Ping file
            EditorGUIUtility.PingObject(preview.clip);
        }

        private void SaveAsCharacterPod()
        {
            //Open file browser
            string path = EditorUtility.SaveFilePanelInProject("Save character set", "new CharacterSet", "asset", "Save character set");

            //Cancel by user
            if (string.IsNullOrEmpty(path))
                return;

            //Create instance
            Speech set = ScriptableObject.CreateInstance<Speech>();

            //Save and import file
            AssetDatabase.CreateAsset(set, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            //Ping file
            EditorGUIUtility.PingObject(set);
        }

        /// <summary> Create clip data and send the request. </summary>
        private void Generate()
        {
            CreateClipData pod = new CreateClipData("OneShot", text.BuildResembleString(), "9816e4ee");
            task = APIBridge.CreateClipSync(pod, GetClipCallback);
        }

        /// <summary> Cancel current task if exist. </summary>
        private void CancelCurrentTask()
        {
            if (task != null && task.status != Task.Status.Completed)
            {
                task.status = Task.Status.Completed;
                task.error = new Error(-1, "Cancel by the user.");
            }
        }

        private void Clear()
        {
            ClearTags();
            text.userString = "";
        }

        private void ClearTags()
        {
            text.tags.Clear();
        }

        private void CopyClipBody()
        {
            EditorGUIUtility.systemCopyBuffer = text.BuildResembleString();
        }

        #region Callbacks
        private void GetClipCallback(string audioUri, Error error)
        {
            if (error)
                Debug.LogError("Resemble Error : " + error.code + " - " + error.message);
            else
            {
                Resemble_Window.preview = new AudioPreview(audioUri);
                drawProgressBar = true;
            }
        }

        private void GetAllPodsCallback(ResembleClip[] pods, Error error)
        {
            if (error)
                Debug.LogError("Error : " + error.code + " - " + error.message);

            for (int i = 0; i < pods.Length; i++)
            {
                Debug.Log(pods[i]);
            }
        }

        private void CreateProjectCallback(ProjectStatus status, Error error)
        {
            if (error)
                Debug.LogError("Error : " + error.code + " - " + error.message);
            else
                Debug.Log(status);
        }
        #endregion

    }
}