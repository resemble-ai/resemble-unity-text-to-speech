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
        //Data
        private Clip clip;
        private Task task;
        private Editor clipEditor;
        private bool clipFinished;
        private Error error = Error.None;


        //GUI
        private Rect renameRect;
        private bool clipPlaying;
        private bool requestDownload;
        private bool haveUserData;
        public Text_Editor drawer;
        private GUIContent userData = new GUIContent("UserData", "This area is available to make your life easier. Put whatever you want in it. You can retrieve it in game via YourClip.userData;");

        

        protected override void OnHeaderGUI()
        {
            //Init resources
            InitComponents();

            //Bar
            Rect rect = new Rect(0, 0, Screen.width, 46);
            GUI.Box(rect, "", Styles.bigTitle);

            //Icon
            rect.Set(6, 6, 32, 32);
            GUI.DrawTexture(rect, Resources.instance.icon);

            //Speech name and shorcut
            float width = Styles.header.CalcSize(new GUIContent(clip.speech.name + " > ")).x;
            rect.Set(44, 4, width, 22);
            if (GUI.Button(rect, clip.speech.name + " > ", Styles.header))
                Selection.activeObject = clip.speech;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            //Clip name and rename
            rect.Set(rect.x + rect.width, rect.y, Screen.width - (rect.x + rect.width + 50), rect.height);
            renameRect = rect;
            if (GUI.Button(rect, clip.name, Styles.header))
                Rename();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            //Resemble.ai link
            rect.Set(Screen.width - 140, rect.y + 24, 135, 16);
            if (GUI.Button(rect, "Show in Resemble.ai", EditorStyles.linkLabel))
                WebPage.ResembleProjects.Open(Settings.projectUUID + "/clips/" + clip.uuid);
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
                menu.AddItem(new GUIContent("Update from API"), false, UpdateFromAPI);
                menu.AddItem(new GUIContent("Generate Audio"), false, PatchClip);
                menu.AddItem(new GUIContent("Download wav as..."), false, ExportClip);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rename"), false, Rename);
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
            //Draw text area
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawTextArea();
            GUILayout.EndVertical();

            //Error box
            if (error)
                error.DrawErrorBox();

            //Draw audio area
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawAudioArea();
            GUILayout.EndVertical();

            //Draw userdata area
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawUserDataArea();
            GUILayout.EndVertical();

            //Drax connection message
            Utils.ConnectionRequireMessage();

            //TEMP (still necessary?) Keep refreshing this window 
            Repaint();
        }

        private void DrawTextArea()
        {
            //Tags buttons
            drawer.DrawTagsBtnsLayout(false);

            //Separator
            Rect rect = GUILayoutUtility.GetRect(1.0f, 1.0f, GUILayout.ExpandWidth(true));
            rect.Set(rect.x, rect.y, rect.width, 1.0f);
            EditorGUI.DrawRect(rect, Color.grey * 0.2f);

            //Draw text field
            drawer.DoLayout(true, false);

            //Draw character count bar
            rect = GUILayoutUtility.GetRect(Screen.width, 25).Shrink(10);
            rect.Set(rect.x, rect.y - 10, rect.width, rect.height + 10);
            drawer.DrawCharCountBar(rect);

            //Draw bottom text buttons
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Audio"))
                PatchClip();
            if (!clipFinished && (GUILayout.Button("Refresh")))
                CheckClipFinished();
            if (clipFinished && (GUILayout.Button("Download File")))
                DownloadClip();
            GUILayout.EndHorizontal();
        }

        private void DrawAudioArea()
        {
            if (requestDownload || task != null)
            {
                GUILayout.Space(10);
                Rect rect = GUILayoutUtility.GetRect(Screen.width, 30);
                if (task == null)
                {
                    //EditorGUI.ProgressBar(rect, 0, "Sending request...");
                    DrawPendingLabel();
                }
                else if (task.preview == null)
                {
                    //EditorGUI.ProgressBar(rect, 0, "Generate audio file...");
                    DrawPendingLabel();
                }
                else if (task.status == Task.Status.Downloading && !task.preview.done)
                {
                    float progress = task.preview.download.progress;
                    EditorGUI.ProgressBar(rect, progress, "Download : " + Mathf.RoundToInt(progress * 100) + "%");
                }
            }

            //Draw Audio preview
            else if (clip.clip != null)
            {
                DrawAudioPlayer();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open file"))
                {
                    EditorGUIUtility.PingObject(clip.clip);
                    Selection.activeObject = clip.clip;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawUserDataArea()
        {
            //Toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(-4);
            GUILayout.Label(userData, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            if (haveUserData && GUILayout.Button("Remove text", EditorStyles.toolbarButton))
            {
                haveUserData = false;
                clip.userdata = "";
                EditorUtility.SetDirty(clip);
            }
            if (!haveUserData && GUILayout.Button("Add text", EditorStyles.toolbarButton))
                haveUserData = true;
            int id = 0;
            if (GUILayout.Button("Add Label", EditorStyles.toolbarButton))
            {
                string name = "New label";
                while (clip.ContainsLabel(name))
                {
                    id++;
                    name = "New Label " + id;
                }
                ArrayUtility.Add(ref clip.labels, new Label(name, 0));
                EditorUtility.SetDirty(clip);
            }
            GUILayout.Space(-4);
            GUILayout.EndHorizontal();


            //User data string
            if (haveUserData)
            {
                GUILayout.Space(10);
                clip.userdata = GUILayout.TextArea(clip.userdata, GUILayout.MinHeight(50));
            }

            //Draw labels
            if (clip.labels != null && clip.labels.Length > 0)
            {
                GUILayout.Space(10);
                int count = clip.labels.Length;
                for (int i = 0; i < count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    string labelText = GUILayout.TextField(clip.labels[i].text);
                    int labelValue = EditorGUILayout.IntField(clip.labels[i].value, GUILayout.Width(50));
                    bool delete = GUILayout.Button("X", GUILayout.Width(20));
                    GUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (delete)
                        {
                            ArrayUtility.RemoveAt(ref clip.labels, i);
                            count--;
                            i--;
                        }
                        else
                        {
                            clip.labels[i] = new Label(labelText, labelValue);
                            EditorUtility.SetDirty(clip);
                        }
                    }
                }
                GUILayout.Space(10);
            }
        }

        private void DrawPendingLabel()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Pending...", Styles.centredLabel);
            Rect rect = GUILayoutUtility.GetRect(25, 25);
            Material mat = Resources.instance.loadingMat;
            mat.SetFloat("_Progress", (float)(EditorApplication.timeSinceStartup % 1.0f));
            mat.SetColor("_Color", new Color(0.0f, 0.0f, 0.0f, 1.0f));
            EditorGUI.DrawPreviewTexture(rect, Resources.instance.loadingTex, mat);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void InitComponents()
        {
            Styles.Load();
            clip = target as Clip;
            if (clip.text == null)
                clip.text = new Text();
            if (drawer == null)
                drawer = new Text_Editor(clip.text, SetDirty, Repaint);
        }

        private void OnEditText()
        {
            EditorUtility.SetDirty(clip);
        }

        [System.Obsolete("Generate clip by sync methode. This is deprecated, use async patch methodes instead.")]
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
            CreateClipData pod = new CreateClipData(clip.name, clip.text.BuildResembleString(), clip.speech.voiceUUID);    //"9816e4ee"
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
            {
                NotificationsPopup.Add("Error on clip " + clip.name + "\n" + error.message, MessageType.Error, clip);
                return;
            }
            else
            {
                NotificationsPopup.Add("Download complete\n" + clip.name, MessageType.Info, clip);
            }

            //Write file
            string savePath = clip.GetSavePath();
            File.WriteAllBytes(savePath, data);

            //Import asset
            AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
            clip.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(savePath);

            task = null;
            Repaint();
        }

        public void OnEnable()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value += ApplicationUpdate;
            info.SetValue(null, value);
            InitComponents();
            haveUserData = !string.IsNullOrEmpty(clip.userdata);
            drawer.Refresh();
            Repaint();
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

            //Get playing clip data
            int sample = 0;
            float time = 0.0f;
            if (clipPlaying)
            {
                clipPlaying = AudioPreview.IsClipPlaying(clip.clip);
                sample = AudioPreview.GetClipSamplePosition(clip.clip);
                time = sample / (float)clip.clip.samples;
            }

            //Preview toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(-4);

            if (!clipPlaying)
            {
                if (GUILayout.Button("Play", EditorStyles.toolbarButton))
                {
                    AudioPreview.PlayClip(clip.clip);
                    clipPlaying = true;
                }
            }
            else
            {
                if (GUILayout.Button("Stop", EditorStyles.toolbarButton))
                {
                    AudioPreview.StopClip(clip.clip);
                    clipPlaying = false;
                }
            }

            GUILayout.FlexibleSpace();

            //Draw clip length label
            float clipLength = clip.clip.length;
            GUILayout.Label((time * clipLength).ToString("0:00") + "/" + clipLength.ToString("0:00"));

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            //Draw preview
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 100);
            
            //Allows you to launch the clip at a given timming.
            if (GUI.Button(rect, "", GUIStyle.none))
            {
                time = Rect.PointToNormalized(rect, Event.current.mousePosition).x;
                sample = Mathf.RoundToInt(time * clip.clip.samples);
                if (clipPlaying)
                    AudioPreview.StopClip(clip.clip);
                AudioPreview.PlayClip(clip.clip, sample);
                AudioPreview.SetClipSamplePosition(clip.clip, sample);
                clipPlaying = true;
            }

            //Draw clip spectrum
            clipEditor.OnPreviewGUI(rect, GUI.skin.box);

            //Draw progress bar
            if (clipPlaying)
            {
                rect.Set(rect.x + rect.width * time, rect.y, 2, rect.height);
                EditorGUI.DrawRect(rect, Color.white);
                Repaint();
            }
        }

        public void PrintTargetPath()
        {
            Debug.Log(clip.GetSavePath());
        }

        public void UpdateFromAPI()
        {
            if (!EditorUtility.DisplayDialog("Update from API", "This operation will overwrite existing " +
                "information with information from the Resemble.ai website.", "Ok", "Cancel"))
                return;

            APIBridge.GetClip(clip.uuid, (ResembleClip clip, Error error) =>
            {
                if (error)
                    error.Log();
                else
                {
                    this.clip.text.ParseResembleString(clip.body);
                    this.clip.name = clip.title;
                    drawer.Refresh();
                    Repaint();
                }
            });
        }

        public void ExportClip()
        {

        }

        public void Rename()
        {
            StringPopup.Show(GUIUtility.GUIToScreenRect(renameRect.Offset(0, 20, 0, 0)),
            "Rename clip", clip.name, (string value) => {
                if (!string.IsNullOrEmpty(clip.name) && value != clip.name)
                    Rename(value);
            });
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

        public void PatchClip()
        {
            if (string.IsNullOrEmpty(clip.uuid))
            {
                Debug.LogError("This clip have no UUID");
                return;
            }

            ClipPatch patch = new ClipPatch(clip.name, clip.text.BuildResembleString(), clip.speech.voiceUUID);
            APIBridge.UpdateClip(clip.uuid, patch, (string content, Error error) =>
            {
                if (error)
                {
                    NotificationsPopup.Add(error.message, MessageType.Error, clip);
                }
                else
                {
                    //Debug.Log(content);
                    //this.goBackToParent = true;
                }

            });
        }

        public void DownloadClip()
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

            requestDownload = true;
            APIBridge.GetClip(clip.uuid, (ResembleClip result, Error error) =>
            {
                requestDownload = false;
                if (error)
                {
                    NotificationsPopup.Add(error.message, MessageType.Error, clip);
                }
                else
                {
                    task = APIBridge.DownloadClip(result.link, OnDownloaded);
                }
                Repaint();
            });
        }

        public void CheckClipFinished()
        {
            APIBridge.GetClip(clip.uuid, (ResembleClip clip, Error error) =>
            {
                if (error)
                    error.Log();
                else
                {
                    Debug.Log("Finished : " + clip.finished);
                    if (clip.finished)
                    {
                        this.clipFinished = true;
                        Repaint();
                    }
                }
            });
        }

        public void Rename(string value)
        {
            clip.name = value;
            SetDirty();
            ReImport();
        }

        public new void SetDirty()
        {
            EditorUtility.SetDirty(clip);
        }

        public void ReImport()
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip), ImportAssetOptions.ForceUpdate);
        }

    }
}