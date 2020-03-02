using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    [CustomEditor(typeof(Speech))]
    public class Speech_Editor : Editor
    {
        private Speech speech;
        private ReorderableList list;
        public AudioClip clip;
        private Vector2 mp;
        private Rect voiceRect;
        private Rect createClipRect;
        private Rect importClipRect;

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        protected override void OnHeaderGUI()
        {
            //Init resources
            Styles.Load();
            speech = target as Speech;

            //Bar
            Rect rect = new Rect(0, 0, Screen.width, 46);
            GUI.Box(rect, "", Styles.bigTitle);

            //Icon
            rect.Set(6, 6, 32, 32);
            GUI.DrawTexture(rect, Resources.instance.icon);

            //Title
            rect.Set(44, 4, Screen.width - 100, 20);
            GUI.Label(rect, speech.name, EditorStyles.largeLabel);

            //Pod count
            rect.Set(44, 23, Screen.width - 130, 16);
            GUI.Label(rect, "Clip count : " + speech.clips.Count);

            //Resemble.ai link
            rect.Set(Screen.width - 122, rect.y + 5, 125, 16);
            if (GUI.Button(rect, "Open Resemble.ai", EditorStyles.linkLabel))
                WebPage.ResembleHome.Open();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            //Help button
            rect.Set(Screen.width - 37, 6, 16, 16);
            if (GUI.Button(rect, Styles.characterSetHelpBtn, GUIStyle.none))
                WebPage.PluginCharacterSet.Open();

            //Options button
            rect.x += 18;
            if (GUI.Button(rect, Styles.popupBtn, GUIStyle.none))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Export all clips in wav"), false, ExportAllClipsInWav);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Help"), false, () => { WebPage.PluginCharacterSet.Open(); });
                menu.AddItem(new GUIContent("Settings"), false, Settings.OpenWindow);
                menu.DropDown(rect);
            }

            GUILayout.Space(50);
        }

        public override void OnInspectorGUI()
        {
            //Header
            GUILayout.Space(10);
            DrawContentHeader();
            GUILayout.Space(10);

            bool noVoice = string.IsNullOrEmpty(speech.voiceUUID);
            if (noVoice)
            {
                EditorGUILayout.HelpBox("Please select a voice.", MessageType.Info);
            }
            EditorGUI.BeginDisabledGroup(noVoice);

            //Rebuild list
            if (list == null)
            {
                list = new ReorderableList(speech.clips, typeof(Clip), false, true, false, false);
                list.onSelectCallback += List_OnSelect;
                list.elementHeight = 28;
                list.drawElementCallback = List_DrawElement;
                list.drawHeaderCallback = List_DrawHeader;
            }
            mp = Event.current.mousePosition;
            list.DoLayoutList();
            Repaint();

            //Add clip btn
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            //Import existing clip button
            if (GUILayout.Button("Import existing clip"))
            {
                StringPopup.Hide();
                ClipPopup.Show(importClipRect.Offset(-200, 18, 200, 120),
                    speech, (ResembleClip clip) =>
                    {
                        if (clip != null)
                            ImportClip(clip);
                    });
            }

            //Get btn rect
            if (Event.current.type == EventType.Repaint)
                importClipRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());

            //Create new clip button
            if (GUILayout.Button("Create new clip"))
            {
                ClipPopup.Hide();
                StringPopup.Show(createClipRect.Offset(-200, 18, 200, 51),
                    "New clip name", (string value) =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        APIBridge.CreateClip(new CreateClipData(value, "", speech.voiceUUID), (ClipStatus status, Error error) =>
                        {
                            if (error)
                                error.Log();
                            else
                            {
                                if (status.status == "OK")
                                    AddClip(value, status.id);
                                else
                                    Debug.LogError("Cannot create the clip, please try again later.");
                            }
                        });
                    }
                });
            }
            //Get btn rect
            if (Event.current.type == EventType.Repaint)
                createClipRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());

            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            //Draw connected error
            Utils.ConnectionRequireMessage();
        }

        public void AddClip(string name, string uuid)
        {
            Clip clip = CreateInstance<Clip>();
            clip.name = name;
            clip.uuid = uuid;
            clip.text = new Text();
            AddClipToAsset(clip);
        }

        public void ImportClip(ResembleClip source)
        {
            Clip clip = CreateInstance<Clip>();
            clip.text = new Text();
            clip.uuid = source.uuid;
            clip.name = source.title;
            clip.text.ParseResembleString(source.body);
            AddClipToAsset(clip);
        }

        public void AddClipToAsset(Clip clip)
        {
            //Add clip to list
            clip.speech = speech;
            speech.clips.Add(clip);
            speech.clips = speech.clips.OrderBy(x => x.name).ToList();

            //Add clip to assets
            AssetDatabase.AddObjectToAsset(clip, speech);
            EditorUtility.SetDirty(speech);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(speech), ImportAssetOptions.ForceUpdate);

            //Focus clip
            Selection.activeObject = clip;
        }

        private void OnEnable()
        {
            speech = target as Speech;
            int count = speech.clips.Count;
            for (int i = 0; i < count; i++)
            {
                if (!AssetDatabase.IsSubAsset(speech.clips[i]))
                {
                    speech.clips.RemoveAt(i);
                    count--;
                    i--;
                }
            }
        }

        private void List_OnSelect(ReorderableList list)
        {
            EditorGUIUtility.PingObject(speech.clips[list.index]);
        }

        private void List_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect titleRect = new Rect(rect.x + 20, rect.y + 2, rect.width - 100, rect.height);
            GUI.Label(titleRect, speech.clips[index].name, EditorStyles.largeLabel);
            titleRect.Set(rect.x, rect.y + rect.height - 1, rect.width, 1.0f);
            EditorGUI.DrawRect(titleRect, Color.grey * 0.3f);
            bool haveClip = speech.clips[index].clip != null;

            float width = rect.width;
            rect.Set(width - 90, rect.y + 2, 50, rect.height - 4);
            if (Utils.FlatButton(rect, "Edit", Styles.clipGreenColor, 1.0f, rect.Contains(mp) ? 0.5f : 0.2f))
                Selection.activeObject = speech.clips[index];
            rect.Set(width - 35, rect.y, 50, rect.height);
            Utils.DragArea(rect, speech.clips[index].clip);
            if (Utils.FlatButton(rect, "Clip", Styles.clipOrangeColor, 1.0f, haveClip ? (rect.Contains(mp) ? 0.5f : 0.2f) : 1.0f) && haveClip)
                Selection.activeObject = speech.clips[index].clip;
        }

        private void List_DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Clips");
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Voice", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(speech.voiceName) ? "None" : speech.voiceName), FocusType.Passive))
            {
                VoicePopup.Show(voiceRect, (Voice voice) =>
                {
                    speech.voiceName = voice.name;
                    speech.voiceUUID = voice.uuid;
                    EditorUtility.SetDirty(speech);
                });
            }
            if (Event.current.type == EventType.Repaint)
                voiceRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect().Offset(0, 18, 0, 100));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            Utils.DrawSeparator();
        }

        public void ExportAllClipsInWav()
        {
            EditorUtility.SaveFolderPanel("Export wav files", "", "");
        }
    }
}