using System.IO;
using UnityEngine;
using UnityEditor;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public partial class Resemble_Window
    {
        private const string oneShotLabel = "Unity - OneShotClip";
        public static AudioPreview preview;
        private static Text_Editor drawer;
        private static bool drawProgressBar;

        private const float checkCooldown = 1.5f;  //Time in seconds between 2 checks
        private double lastCheckTime;

        //Saved stuff
        private static Text text
        {
            get
            {
                return Resources.instance.oneShotText;
            }
        }
        private static string voiceName
        {
            get
            {
                return Resources.instance.oneShotVoiceName;
            }
            set
            {
                if (value != voiceName)
                {
                    Resources.instance.oneShotVoiceName = value;
                    EditorUtility.SetDirty(Resources.instance);
                }
            }
        }
        private static string voiceUUID
        {
            get
            {
                return Resources.instance.oneShotVoiceUUID;
            }
            set
            {
                if (value != voiceUUID)
                {
                    Resources.instance.oneShotVoiceUUID = value;
                    EditorUtility.SetDirty(Resources.instance);
                }
            }
        }
        private static string savePath
        {
            get
            {
                return Resources.instance.oneShotPath;
            }
            set
            {
                if (value != savePath)
                {
                    Resources.instance.oneShotPath = value;
                    EditorUtility.SetDirty(Resources.instance);
                }
            }
        }

        //GUI stuff
        private static Error error;
        private Dropdown fileDropDown;
        private Dropdown editDropDown;
        private Dropdown settingsDropDown;
        private Rect voiceRect;

        /// <summary> Called when the window is show. </summary>
        private static void EnableOneShot()
        {
            drawer.Refresh();
        }

        public void DrawOneShotGUI()
        {
            //Check connection
            if (!Settings.haveProject)
                Utils.ConnectionRequireMessage();
            EditorGUI.BeginDisabledGroup(!Settings.haveProject);

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

            //Init components
            if (Resources.instance.oneShotText == null)
                Resources.instance.oneShotText = new Text();
            if (drawer == null)
                drawer = new Text_Editor(Resources.instance.oneShotText, SetDirty, Repaint);

            //Tags
            drawer.DrawTagsBtnsLayout(!Settings.haveProject);

            //Draw text area
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 300).Shrink(10);
            drawer.DrawTextArea(rect, Settings.haveProject);

            //Draw char count progress bar
            rect.Set(rect.x, rect.y + rect.height, rect.width, 16);
            drawer.DrawCharCountBar(rect);
            GUILayout.Space(20);


            //Bot commands
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            //Voice field
            DrawVoiceField();

            //Generate button
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(voiceUUID) || string.IsNullOrEmpty(text.userString));
            if (GUILayout.Button("Generate audio", GUILayout.ExpandWidth(false)))
                Generate();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(7);
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();


            //Footer
            GUILayout.FlexibleSpace();
            Utils.DrawSeparator();
            GUILayout.Label("The audio files generated here will not be saved in the Resemble project.", Styles.settingsBody);
        }

        private void DrawVoiceField()
        {
            if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(voiceName) ? "None" : voiceName), FocusType.Passive))
            {
                VoicePopup.Show(voiceRect, (Voice voice) =>
                {
                    voiceName = voice.name;
                    voiceUUID = voice.uuid;
                });
            }
            if (Event.current.type == EventType.Repaint)
                voiceRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect().Offset(0, 18, 0, 100));
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
            string path = EditorUtility.SaveFilePanel("Save OneShot clip", savePath, "", "wav");
            if (string.IsNullOrEmpty(path))
                return;
            savePath = Path.GetDirectoryName(path);
            AsyncRequest.Make(text.BuildResembleString(), voiceUUID, path);
            string name = Path.GetFileName(path);
            name = name.Remove(name.Length - 4);
            NotificationsPopup.Add("OneShot request send : " + name, MessageType.Info, null);
            text.SelectAll();
            text.Delete();
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

        public new void SetDirty()
        {
            EditorUtility.SetDirty(Resources.instance);
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