using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Resemble
{
    public class Resemble_Window : EditorWindow
    {
        public static Resemble_Window window;
        public static AudioPreview preview;
        public PostPod pod = new PostPod();
        private static PodText text = new PodText();
        private static Text clipText = new Text();
        private static APIBridge.Task task;
        private static TextField drawer = new TextField();
        private static string placeHolderText = "";
        private static PlaceHolderAPIBridge.ClipRequest request;
        private static bool drawProgressBar;

        [MenuItem("Window/Audio/Resemble")]
        public static void Open()
        {
            window = (Resemble_Window)EditorWindow.GetWindow(typeof(Resemble_Window));
            window.titleContent = new GUIContent("Resemble");
            window.Show();
        }

        void OnGUI()
        {
            //Init components
            Styles.Load();
            if (drawer.target == null)
                drawer.target = clipText;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("File", EditorStyles.toolbarButton))
                FileDropDown();
            if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                EditDropDown();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Styles.popupBtn, EditorStyles.toolbarButton))
                SettingsDropDown();
            GUILayout.Space(-6);
            GUILayout.EndHorizontal();


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

            //text.OnGUI(request);

            drawer.DrawTagsBtnsLayout();

            //Draw text area
            Rect rect = GUILayoutUtility.GetAspectRect(1.0f).Shrink(10);
            drawer.DrawTextArea(rect);

            //Draw char count progress bar
            rect.Set(rect.x, rect.y + rect.height, rect.width, 16);
            drawer.DrawCharCountBar(rect);
            GUILayout.Space(20);

            Repaint();


            /*
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 150);
            rect.Set(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
            GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
            text.OnGUI(rect, style);*/

            //pod.emotion = "style1";

            if (GUILayout.Button("Generate preview"))
            {
                //Cancel current task if exist
                if (task != null && task.status != APIBridge.Task.Status.completed)
                {
                    task.status = APIBridge.Task.Status.completed;
                    task.error = new Error(-1, "Cancel by the user.");
                }

                pod.title = "OneShot";
                pod.body = "<speak><p>" + clipText.BuildResembleString() + "</p></speak>";
                pod.voice = "9816e4ee";
                Debug.Log(pod.body);
                task = APIBridge.CreateClipSync(pod, GetClipCallback);

                /*
                if (EditorUtility.DisplayCancelableProgressBar("Process Resemble clip...", "Progress...", 0.5f))
                    EditorUtility.ClearProgressBar();*/
            }

            if (GUILayout.Button("Get all pods"))
            {
                APIBridge.GetAllPods(GetAllPodsCallback);
            }

            if (GUILayout.Button("Create project"))
            {
                Project project = new Project();
                project.name = "Watermelon";
                project.description = "Project generated from unity plugin";
                APIBridge.CreateProject(project, CreateProjectCallback);
            }

            if (preview != null && preview.clip != null && GUILayout.Button("Play clip"))
            {
                AudioPreview.PlayClip(preview.clip);
            }

            GUILayout.Space(50);
            placeHolderText = EditorGUILayout.TextField("PlaceHolderText", placeHolderText);
            if (GUILayout.Button(request == null ? "Request placeHolder api" : request.status.ToString()))
            {
                request = PlaceHolderAPIBridge.GetAudioClip(placeHolderText, GetClipCallback);
            }

        }

        private void FileDropDown()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Save as wav..."), false, SaveAsWavFile);
            menu.AddItem(new GUIContent("Save as CharacterSet..."), false, SaveAsCharacterPod);
            menu.ShowAsContext();
        }

        private void EditDropDown()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Generate"), false, Generate);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear tags"), false, Clear);
            menu.AddItem(new GUIContent("Clear"), false, Clear);
            menu.ShowAsContext();
        }

        private void SettingsDropDown()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Settings"), false, Settings.OpenWindow);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Help"), false, () => { WebPage.PluginWindow.Open(); });
            menu.AddItem(new GUIContent("Resemble API"), false, () => { WebPage.ResembleAPIDoc.Open(); });
            menu.ShowAsContext();
        }

        private void SaveAsWavFile()
        {

        }

        private void SaveAsCharacterPod()
        {

        }

        private void Generate()
        {

        }

        private void Clear()
        {

        }

        private void GetClipCallback(AudioClip clip, Error error)
        {
            if (error)
                Debug.LogError("Error : " + error.code + " - " + error.message);
            else
            {
                AudioPreview.PlayClip(clip);
                request = null;
            }
        }

        private void GetClipCallback(AudioPreview preview, Error error)
        {
            if (error)
                Debug.LogError("Error : " + error.code + " - " + error.message);
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
    }
}