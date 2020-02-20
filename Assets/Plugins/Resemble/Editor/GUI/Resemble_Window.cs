using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble.Structs;

namespace Resemble
{
    public class Resemble_Window : EditorWindow
    {
        public static Resemble_Window window;
        public static AudioPreview preview;
        private static APIBridge.Task task;
        private static TextField drawer = new TextField();
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


        /// <summary> Open the Resemble window. This window is used to generate one-shot audioclips. </summary>
        [MenuItem("Window/Audio/Resemble")]
        public static void Open()
        {
            window = (Resemble_Window)EditorWindow.GetWindow(typeof(Resemble_Window));
            window.minSize = new Vector2(300, 400);
            window.titleContent = new GUIContent("Resemble");
            window.Show();
        }

        void OnGUI()
        {
            //Init components
            Styles.Load();
            if (drawer.target == null)
                drawer.target = text;

            //Toolbar
            EditorGUI.BeginDisabledGroup(!Settings.haveProject);
            DrawToolbar();
            EditorGUI.EndDisabledGroup();

            //Check connection
            if (!Settings.haveProject)
                GUIUtils.ConnectionRequireMessage();
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
            Repaint();
        }

        #region Context menu functions
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //File button
            fileDropDown.DoLayout(new GUIContent("File"),
                new Dropdown.Item("Save audioclip file...", preview != null && preview.clip != null, SaveClipFile),
                new Dropdown.Item("Save as CharacterSet...", SaveAsCharacterPod)
            );

            //Edit button
            editDropDown.DoLayout(new GUIContent("Edit"),
                new Dropdown.Item("Generate", !string.IsNullOrEmpty(text.userString), Generate),
                new Dropdown.Item(""),
                new Dropdown.Item("Clear tags", ClearTags),
                new Dropdown.Item("Clear", Clear)
            );

            GUILayout.FlexibleSpace();

            //Settings button
            settingsDropDown.DoLayout(new GUIContent(Styles.popupBtn),
                new Dropdown.Item("Copy clip body", CopyClipBody),
                new Dropdown.Item("Settings", Settings.OpenWindow),
                new Dropdown.Item(""),
                new Dropdown.Item("Help", () => { WebPage.PluginWindow.Open(); }),
                new Dropdown.Item("Resemble API", () => { WebPage.ResembleAPIDoc.Open(); })
            );

            GUILayout.Space(-6);
            GUILayout.EndHorizontal();
        }

        private void SaveClipFile()
        {
            //Open file browser
            string path = EditorUtility.SaveFilePanel("Save clip file", Application.dataPath, "new audioclip", "wav");

            //Cancel by user
            if (string.IsNullOrEmpty(path))
                return;

            //Save and import file
            AssetDatabase.CreateAsset(preview.clip, path);
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
            CharacterSet set = ScriptableObject.CreateInstance<CharacterSet>();

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
            if (task != null && task.status != APIBridge.Task.Status.completed)
            {
                task.status = APIBridge.Task.Status.completed;
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
        #endregion

        #region Callbacks
        private void GetClipCallback(AudioPreview preview, Error error)
        {
            if (error)
                Debug.LogError("Resemble Error : " + error.code + " - " + error.message);
            else
            {
                Resemble_Window.preview = preview;
                drawProgressBar = true;
            }
        }

        private void GetAllPodsCallback(ResemblePod[] pods, Error error)
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