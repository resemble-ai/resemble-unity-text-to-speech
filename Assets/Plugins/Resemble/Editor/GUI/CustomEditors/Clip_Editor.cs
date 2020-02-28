using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    [CustomEditor(typeof(Clip)), CanEditMultipleObjects]
    public class Clip_Editor : Editor
    {
        private Clip clip;
        private bool rename;
        private string renameLabel;
        private int renameControlID;
        private Editor clipEditor;
        private AudioImporter importer;
        private bool dirtySettings;
        private Task task;
        private Error error = Error.None;
        public Text_Editor drawer = new Text_Editor();

        protected override void OnHeaderGUI()
        {
            //Init resources
            Styles.Load();
            clip = target as Clip;
            if (drawer.target == null)
                drawer.target = clip.text;

            //Bar
            Rect rect = new Rect(0, 0, Screen.width, 46);
            GUI.Box(rect, "", Styles.bigTitle);

            //Icon
            rect.Set(6, 6, 32, 32);
            GUI.DrawTexture(rect, Resources.instance.icon);

            //Name & Rename field
            float width = Styles.header.CalcSize(new GUIContent(clip.speech.name + " > ")).x;
            rect.Set(44, 4, width, 22);
            if (GUI.Button(rect, clip.speech.name + " > ", Styles.header))
                Selection.activeObject = clip.speech;
            rect.Set(rect.x + rect.width, rect.y, Screen.width - (rect.x + rect.width + 50), rect.height);

            //Autorename
            if (clip.autoRename)
            {
                clip.autoRename = false;
                rename = true;
                renameLabel = clip.name;
            }

            if (RenameableField(rect, ref rename, ref renameLabel, clip.name, out renameControlID))
            {
                clip.name = string.IsNullOrWhiteSpace(renameLabel) ? "Untitled" : renameLabel;
                if (clip.clip != null)
                    clip.clip.name = clip.name;
                if (clip.clipCopy != null)
                    clip.clipCopy.name = clip.name;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
                Repaint();
            }

            //Resemble.ai link
            rect.Set(Screen.width - 140, rect.y + 24, 135, 16);
            if (GUI.Button(rect, "Show in Resemble.ai", EditorStyles.linkLabel))
                WebPage.ResembleProjects.Open(Settings.projectUUID + "/clips/" + clip.UUID);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            //Help button
            rect.Set(Screen.width - 37, 6, 16, 16);
            if (GUI.Button(rect, Styles.characterSetHelpBtn, GUIStyle.none))
                WebPage.PluginDoc.Open();

            //Options button
            rect.x += 18;
            if (GUI.Button(rect, Styles.popupBtn, GUIStyle.none))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Print target path"), false, PrintTargetPath);
                menu.AddItem(new GUIContent("Regenerate pod"), false, RegeneratePod);
                menu.AddItem(new GUIContent("Export in wav"), false, ExportPod);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset"), false, Reset);
                menu.AddItem(new GUIContent("Delete"), false, Delete);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Help"), false, () => { WebPage.PluginClip.Open(); });
                menu.AddItem(new GUIContent("Settings"), false, Settings.OpenWindow);
                menu.DropDown(rect);
            }

            GUILayout.Space(50);
        }

        public override void OnInspectorGUI()
        {
            //Draw text layout
            drawer.DoLayout(true, OnEditText, OnGenerate);

            //Show pending request button
            if (task != null && task.status != Task.Status.Completed)
            {
                if (GUILayout.Button("Show pending request"))
                    Resemble_Window.Open(Resemble_Window.Tab.Pool);
            }

            if (error)
                error.DrawErrorBox();

            if (task != null && task.preview != null && task.status == Task.Status.Downloading)
            {
                if (!task.preview.done)
                {
                    Rect rect = GUILayoutUtility.GetRect(Screen.width, 16);
                    float progress = task.preview.download.progress;
                    EditorGUI.ProgressBar(rect, progress, "Download : " + Mathf.RoundToInt(progress * 100) + "%");
                }
                else
                {

                }
            }


            if (clip.clip != null)
            {
                //Audio preview
                GUILayout.Space(10);
                Utils.DrawSeparator();
                GUILayout.Space(10);
                GUILayout.Label("Preview", EditorStyles.largeLabel);

                DrawAudioPlayer();

                //Import settings
                GUILayout.Space(10);
                Utils.DrawSeparator();
                GUILayout.Label("Import settings", EditorStyles.largeLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                importer.loadInBackground = EditorGUILayout.Toggle(new GUIContent("Load In Background",
                    "When the flag is set, the loading of the clip will happen delayed without blocking the main thread."),
                    importer.loadInBackground);
                dirtySettings |= EditorGUI.EndChangeCheck();

                //Apply button
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUI.BeginDisabledGroup(!dirtySettings);
                if (GUILayout.Button("Apply"))
                {
                    dirtySettings = false;
                    importer.SaveAndReimport();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            Utils.ConnectionRequireMessage();
        }

        private void OnEditText()
        {
            EditorUtility.SetDirty(clip);
        }

        private void OnGenerate()
        {
            //Get and create directory
            string folderPath = clip.GetSaveFolder();
            string savePath = folderPath + clip.name + ".wav";
            Directory.CreateDirectory(folderPath);

            //Generate a place-holder to the save path (Will be replaced by the clip generated by Resemble)
            byte[] file = File.ReadAllBytes(AssetDatabase.GetAssetPath(Resources.instance.processClip));
            File.WriteAllBytes(savePath, file);

            //Import placeholder
            AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);

            //Send request to the API
            CreateClipData pod = new CreateClipData(clip.name, clip.text.BuildResembleString(), "9816e4ee");
            task = APIBridge.CreateClipSync(pod, (string audioUri, Error error) => 
            {
                if (!error)
                {
                    this.error = Error.None;
                    task.DownloadResult(audioUri, OnDownloaded);
                }
                else
                {
                    this.error = error;
                }
            });
            Pool.AddTask(task);
        }

        private void OnDownloaded(byte[] data, Error error)
        {
            if (error)
                return;

            //Write file
            string savePath = clip.GetSavePath();
            File.WriteAllBytes(savePath, data);

            //Import asset
            AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
        }

        public void OnEnable()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value += ApplicationUpdate;
            info.SetValue(null, value);
            Clip clip = target as Clip;
            if (clip != null && clip.text != null && drawer != null && drawer.target != null)
                drawer.Refresh();
        }

        public void OnDisable()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value -= ApplicationUpdate;
            info.SetValue(null, value);
        }

        private void ApplicationUpdate()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                Object[] pods = Selection.objects.Where(x => x is Clip).ToArray();
                if (pods.Length == 1 && pods[0] == clip)
                    Delete();
                else if (pods.Length > 1)
                    DeleteMultiple(pods);
            }
        }

        public static bool RenameableField(Rect rect, ref bool rename, ref string renameLabel, string originName, out int controlID)
        {
            KeyCode keycode = KeyCode.A;
            controlID = GUIUtility.GetControlID("renameLabel".GetHashCode(), FocusType.Passive, rect);
            if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
                keycode = Event.current.keyCode;
            renameLabel = GUI.TextField(rect, rename ? renameLabel : originName, rename ? Styles.headerField : Styles.header);
            TextEditor textEdit = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            switch (keycode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (rename)
                    {
                        controlID = 0;
                        GUI.FocusControl("None");
                    }
                    break;
                case KeyCode.Escape:
                    if (rename)
                    {
                        controlID = 0;
                        renameLabel = originName;
                        GUI.FocusControl("None");
                    }
                    break;
            }

            if (controlID != textEdit.controlID - 1)
            {
                if (rename)
                {
                    rename = false;
                    return true;
                }
            }
            else
            {
                if (!rename)
                {
                    rename = true;
                    renameLabel = originName;
                }
            }
            return false;
        }

        public void DrawAudioPlayer()
        {
            if (clipEditor == null || clipEditor.target != clip.clip)
            {
                clipEditor = Editor.CreateEditor(clip.clip);
            }

            if (importer == null)
            {
                importer = AudioImporter.GetAtPath(AssetDatabase.GetAssetPath(clip.clip)) as AudioImporter;
            }


            //Preview toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Play", EditorStyles.toolbarButton))
            { AudioPreview.PlayClip(clip.clip); }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetRect(Screen.width, 100);
            clipEditor.OnPreviewGUI(rect, GUI.skin.box);
        }

        public void PrintTargetPath()
        {
            Debug.Log(clip.GetSavePath());
        }

        public void RegeneratePod()
        {

        }

        public void ExportPod()
        {

        }

        public void Reset()
        {

        }

        public void Delete()
        {
            clip = target as Clip;
            string path = AssetDatabase.GetAssetPath(clip);
            if (!EditorUtility.DisplayDialog("Delete pod ?", path + "\nYou cannot undo this action.", "Delete", "Cancel"))
                return;
            AssetDatabase.RemoveObjectFromAsset(clip);
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Speech>(path));
            AssetDatabase.ImportAsset(path);
        }

        public void DeleteMultiple(Object[] targets)
        {
            string[] paths = targets.Select(x => AssetDatabase.GetAssetPath(x)).ToArray();
            string allPath = "";
            for (int i = 0; i < paths.Length; i++)
            {
                if (i == 3)
                {
                    allPath += "...";
                    break;
                }
                allPath += paths[i] + "/" + targets[i].name + "\n";
            }
            if (!EditorUtility.DisplayDialog("Delete pods ?", allPath + "\nYou cannot undo this action.", "Delete", "Cancel"))
                return;

            List<string> sets = new List<string>();
            for (int i = 0; i < targets.Length; i++)
            {
                AssetDatabase.RemoveObjectFromAsset(targets[i]);
                if (!sets.Contains(paths[i]))
                    sets.Add(paths[i]);
            }

            for (int i = 0; i < sets.Count; i++)
            {
                EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Speech>(sets[i]));
                AssetDatabase.ImportAsset(sets[i]);
            }
        }

    }
}