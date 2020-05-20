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
        private Editor clipEditor;
        private AsyncRequest request;
        private const float checkCooldown = 1.5f;  //Time in seconds between 2 checks

        //GUI
        private Rect renameRect;
        private bool clipPlaying;
        private bool haveUserData;
        private bool showRawPhonemes;
        public Text_Editor drawer;
        private GUIContent userData = new GUIContent("UserData", "This area is available to make your life easier. Put whatever you want in it. You can retrieve it in game via YourClip.userData.");
        private GUIContent phonemes = new GUIContent("Phonemes", "Phonemes pronounced by the generated voice.");
        private double lastCheckTime;
        private float clipTime;

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
            if (GUI.Button(rect, clip.clipName, Styles.header))
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
                menu.AddItem(new GUIContent("Update from API"), false, UpdateFromAPI);
                menu.AddItem(new GUIContent("Generate Audio"), false, () => { request = AsyncRequest.Make(clip); });
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
            if (request != null && request.status == AsyncRequest.Status.Error)
            {
                request.error.DrawErrorBox();
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.Set(rect.x + rect.width - 70, rect.y + rect.height - 20, 68, 18);
                if (GUI.Button(rect, "Close"))
                {
                    request.status = AsyncRequest.Status.Completed;
                    AsyncRequest.RegisterRefreshEvent();
                }
            }

            //Draw audio area
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawAudioArea();
            GUILayout.EndVertical();

            //Draw phonemes area
            if (clip.speech.includePhonemes)
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawPhonemesArea();
                GUILayout.EndVertical();
            }

            //Draw userdata area
            GUILayout.Space(10);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawUserDataArea();
            GUILayout.EndVertical();

            //Drax connection message
            Utils.ConnectionRequireMessage();
            GUILayout.Space(30);

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
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (request == null)
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(clip.text.userString));
                if (GUILayout.Button("Generate Audio"))
                {
                    request = AsyncRequest.Make(clip);
                    clip.phonemes = null;
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (GUILayout.Button("Select parent"))
                    Selection.activeObject = clip.speech;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawAudioArea()
        {
            //Remove request if completed
            if (request != null && request.status == AsyncRequest.Status.Completed)
                request = null;

            //Draw pending message or dowload progress bar
            if (request != null && !request.isDone)
            {
                if (request.status != AsyncRequest.Status.Downloading)
                {
                    GUILayout.Space(4);
                    Utils.DrawPendingLabel("Pending...");
                    GUILayout.Space(-10);
                    Utils.DrawSeparator();
                    GUILayout.Space(-5);
                }
                else
                {
                    GUILayout.Space(10);
                    Rect rect = GUILayoutUtility.GetRect(Screen.width, 30);
                    float progress = request.downloadProgress;
                    EditorGUI.ProgressBar(rect, progress, "Download : " + Mathf.RoundToInt(progress * 100) + "%");
                }
            }

            //Draw Audio preview
            else if (clip.clip != null)
            {
                DrawAudioPlayer();
            }

            //Draw commands
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (request != null)
            {
                if (GUILayout.Button("Cancel"))
                {
                    AsyncRequest.Cancel(clip.uuid);
                    request = null;
                }
                if (GUILayout.Button("Show pending list"))
                {
                    Resemble_Window.Open(Resemble_Window.Tab.Pool);
                }
                if (clip.clip != null && GUILayout.Button("Select placeholder"))
                {
                    EditorGUIUtility.PingObject(clip.clip);
                    Selection.activeObject = clip.clip;
                }
            }
            else if (clip.clip != null)
            {
                if (GUILayout.Button("Select file"))
                {
                    EditorGUIUtility.PingObject(clip.clip);
                    Selection.activeObject = clip.clip;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawPhonemesArea()
        {
            //Toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(-4);
            GUILayout.Label(phonemes, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            bool haveTable = clip.speech.phonemeTable != null;
            bool havePhonemes = clip.phonemes != null && !clip.phonemes.raw.isEmpty;

            EditorGUI.BeginDisabledGroup(!havePhonemes);
            if (GUILayout.Toggle(showRawPhonemes, "Raw", EditorStyles.toolbarButton) || !haveTable)
                showRawPhonemes = true;

            EditorGUI.BeginDisabledGroup(!haveTable);
            if (GUILayout.Toggle(!showRawPhonemes, "Refined", EditorStyles.toolbarButton))
                showRawPhonemes = false;

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(-4);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            //No phonemes
            if (!havePhonemes)
            {
                if (request == null)
                    EditorGUILayout.HelpBox("This clip does not contain any phoneme data. Regenerate your clip to get new phonemes.", MessageType.Warning);
                return;
            }

            //Get preview area rect
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 100);

            //Set time by click or drag in preview area
            Event e = Event.current;
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && rect.Offset(0, 0, 0, -20).Contains(e.mousePosition))
            {
                Vector2 mousePos = Event.current.mousePosition;
                clipTime = Rect.PointToNormalized(rect, mousePos).x;
                int sample = Mathf.RoundToInt(clipTime * clip.clip.samples);
                Repaint();
            }

            //Draw area background
            EditorGUI.DrawRect(rect, Color.black);
            rect = rect.Shrink(1);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1.0f));

            //Draw progress bar
            Rect barRect = new Rect(rect.x + rect.width * clipTime, rect.y, 2, rect.height);
            EditorGUI.DrawRect(barRect, Color.grey);

            //Preview
            if (showRawPhonemes)
                DrawRawPhonemePreview(rect);
            else
                DrawPhonemePreview(rect, clip.phonemes.refined);
        }

        private void DrawRawPhonemePreview(Rect rect)
        {
            char[] phonemes = clip.phonemes.raw.phonemesChars;
            float[] times = clip.phonemes.raw.end_times;

            int count = phonemes.Length;
            int id = Utils.FindPreviousIndex(clipTime * clip.clip.length, times) + 1;
            id = Mathf.Clamp(id, 0, count - 1);

            //Duration
            float start = id > 0 ? times[id - 1] : 0.0f;
            float end = times[id];
            float duration = end - start;

            //Draw infos
            string info = "Index :\t" + id + " / " + count + "\n";
            info += "Phoneme :\t[" + phonemes[id] + "]\n";
            info += "Start :\t" + start.ToString("0.000") + "\n";
            info += "End :\t" + end.ToString("0.000") + "\n";
            info += "Duration :\t" + duration.ToString("0.000") + "\n";
            GUI.Label(rect, info, Styles.phonemesInfos);

            //Commands
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<"))
                clipTime = Mathf.Max(start / clip.clip.length - 0.0001f, 0);
            if (GUILayout.Button(">"))
                clipTime = Mathf.Min(end / clip.clip.length + 0.0001f);
            if (GUILayout.Button("Copy"))
                EditorGUIUtility.systemCopyBuffer = phonemes[id].ToString();
            GUILayout.EndHorizontal();
        }

        private void DrawPhonemePreview(Rect rect, Phonemes phonemes)
        {
            //No phonemes
            if (phonemes == null)
            {
                if (Utils.BoxWithLink("There is no refined phonemes.", "Rebuild", MessageType.Error))
                    clip.phonemes.UpdateTable(clip.speech.phonemeTable);
                return;
            }

            //Draw top rect
            Rect topRect = new Rect(rect.x, rect.y, rect.width, 18);
            EditorGUI.DrawRect(topRect, Phonemes_Editor.graphBgColor);

            //Draw curves and fields
            float maxValue = 0.0f;
            string maxValueName = "";
            Color maxValueColor = Color.white;
            for (int i = 0; i < phonemes.curves.Length; i++)
            {
                Color curveColor = Color.HSVToRGB((i * 0.13f) % 1.0f, 0.8f, 1.0f);
                EditorGUIUtility.DrawCurveSwatch(rect, phonemes.curves[i].curve, null, curveColor, Phonemes_Editor.transparent, new Rect(0, 0, 1, 1));

                float value = phonemes.curves[i].curve.Evaluate(clipTime);

                if (value > maxValue)
                {
                    maxValue = value;
                    maxValueName = phonemes.curves[i].name;
                    maxValueColor = curveColor;
                }
            }

            //Draw maxValue top label
            if (maxValue > 0.0001f)
            {
                topRect.x = rect.x + rect.width * clipTime;
                Color guiColor = GUI.color;
                GUI.color = maxValueColor;
                GUI.Label(topRect, maxValueName, Styles.whiteLabel);
                GUI.color = guiColor;
            }

            //Commands
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Rebuild"))
                clip.phonemes.UpdateTable(clip.speech.phonemeTable);
            GUILayout.EndHorizontal();
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
                clip.userdata = GUILayout.TextArea(clip.userdata, GUILayout.MinHeight(100));
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

            else if (!haveUserData)
                GUILayout.Space(10);
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

        public void OnEnable()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value += HandleDeleteEvent;
            info.SetValue(null, value);
            InitComponents();
            request = AsyncRequest.Get(clip.uuid);
            if (request != null && !AsyncRequest.refreshing)
                AsyncRequest.RegisterRefreshEvent();
            haveUserData = !string.IsNullOrEmpty(clip.userdata);
            drawer.Refresh();
            Repaint();
        }

        public void OnDisable()
        {
            System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value -= HandleDeleteEvent;
            info.SetValue(null, value);
        }

        private void HandleDeleteEvent()
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

        public void DrawAudioPlayer()
        {
            if (clipEditor == null || clipEditor.target != clip.clip)
            {
                clipEditor = Editor.CreateEditor(clip.clip);
            }

            //Get playing clip data
            if (clipPlaying)
            {
                clipPlaying = AudioPreview.IsClipPlaying(clip.clip);
                int sample = AudioPreview.GetClipSamplePosition(clip.clip);
                clipTime = sample / (float)clip.clip.samples;
            }

            //Preview toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(-4);

            if (!clipPlaying)
            {
                if (GUILayout.Button("Play", EditorStyles.toolbarButton))
                {
                    int sample = Mathf.RoundToInt(clipTime * clip.clip.samples);
                    AudioPreview.PlayClip(clip.clip, sample);
                    AudioPreview.SetClipSamplePosition(clip.clip, sample);
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
            float clipLength = clip.clip.length * 1000;
            GUILayout.Label((clipTime * clipLength).ToString("0:00:000") + " / " + clipLength.ToString("0:00:000"));

            GUILayout.EndHorizontal();
            GUILayout.Space(10);


            //Get preview area rect
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 100);


            //Set time by click or drag in preview area
            Event e = Event.current;
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && rect.Contains(e.mousePosition))
            {
                //Snap
                Vector2 mousePos = Event.current.mousePosition;
                if (mousePos.x - rect.x < 10)
                    mousePos.x = rect.x;

                clipTime = Rect.PointToNormalized(rect, mousePos).x;
                int sample = Mathf.RoundToInt(clipTime * clip.clip.samples);
                Repaint();
            }


            //Play-Pause by space
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                if (clipPlaying)
                {
                    AudioPreview.StopClip(clip.clip);
                    clipPlaying = false;
                }
                else
                {
                    int sample = Mathf.RoundToInt(clipTime * clip.clip.samples);
                    AudioPreview.PlayClip(clip.clip, sample);
                    AudioPreview.SetClipSamplePosition(clip.clip, sample);
                    clipPlaying = true;
                }
                e.Use();
            }


            //Block native preview interactions
            if (e.type != EventType.Repaint && e.type != EventType.Layout && rect.Contains(e.mousePosition))
                e.Use();


            //Draw clip spectrum
            EditorGUI.DrawRect(rect, Color.black);
            rect = rect.Shrink(1);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1.0f));
            clipEditor.OnPreviewGUI(rect, GUIStyle.none);


            //Draw progress bar
            rect.Set(rect.x + rect.width * clipTime, rect.y, 2, rect.height);
            EditorGUI.DrawRect(rect, Color.white);
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
                    this.clip.clipName = clip.title;
                    this.clip.name = clip.uuid + "-" + clip.title;
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
            "Rename clip", clip.clipName, (string value) => {
                if (!string.IsNullOrEmpty(clip.clipName) && value != clip.clipName)
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

        public void Rename(string value)
        {
            clip.clipName = value;
            clip.name = clip.uuid + "-" + value;
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