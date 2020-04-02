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
        //Data
        private Speech speech;
        private ReorderableList list;

        //GUI
        private Vector2 mousePosition;
        private Rect voiceRect;
        private PopupButton deleteButton = new PopupButton("Delete", "Delete the clip selected in the list.", new Vector2(300, 51));
        private PopupButton importButton = new PopupButton("Import", "Import a clip that already exists in the project with this voice.", new Vector2(200, 120));
        private PopupButton CreateButton = new PopupButton("Create", "Create a new clip with this voice.", new Vector2(200, 51));
        private GUIContent phonemeTableLabel = new GUIContent("Phoneme table", "A phoneme table allows you to transform the raw phoneme data into a usable version for your project.\nLeave this field empty if you want to keep the raw data.");

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
            //Init vars
            Event e = Event.current;
            mousePosition = e.mousePosition;

            //Speech properties
            GUILayout.Space(10);
            DrawSpeechProperties();
            GUILayout.Space(10);


            //Select a voice box
            bool noVoice = string.IsNullOrEmpty(speech.voiceUUID);
            if (noVoice)
                EditorGUILayout.HelpBox("Please select a voice.", MessageType.Info);


            //Disable ui if there is no voice selecteds
            EditorGUI.BeginDisabledGroup(noVoice);


            //Rebuild list if needed and draw it
            if (list == null)
                RebuildClipList();
            list.DoLayoutList();


            //Draw commands buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(list.index < 0);
            deleteButton.DoLayout((Rect r) => { HidePopups(); DeletePopup.Show(r, DeleteClip); });
            EditorGUI.EndDisabledGroup();
            importButton.DoLayout((Rect r) => { HidePopups(); ClipPopup.Show(r, speech, ImportClip); });
            CreateButton.DoLayout((Rect r) => { HidePopups(); StringPopup.Show(r, "New clip name", CreateClip); });
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();


            //Draw connected error
            Utils.ConnectionRequireMessage();


            //Keep a constant refresh
            Repaint();
        }

        /// <summary> Ensures that all popup windows that the editor can open are closed. </summary>
        private void HidePopups()
        {
            DeletePopup.Hide();
            ClipPopup.Hide();
            StringPopup.Hide();
        }

        /// <summary> Delete the clip selected in the list from speech. </summary>
        private void DeleteClip(bool deleteOnAPI, bool deleteAudioClip)
        {
            if (list.index < 0)
                return;
            DeleteClip(speech.clips[list.index], deleteOnAPI, deleteAudioClip);
        }

        /// <summary> Delete a clip from speech. </summary>
        public void DeleteClip(Clip clip, bool deleteOnAPI, bool deleteAudioClip)
        {
            //Delete audioclip if needed
            if (deleteAudioClip && clip.clip != null)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(clip.clip));

            //Delete on APIif needed
            if (deleteOnAPI)
                APIBridge.DeleteClip(clip.uuid);

            //Delete clip from speech
            AssetDatabase.RemoveObjectFromAsset(clip);
            speech.clips.Remove(clip);

            //Refresh speech
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetOrScenePath(speech), ImportAssetOptions.ForceSynchronousImport);
        }

        public void CreateClip(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                APIBridge.CreateClip(new ClipPatch.Data(name, "", speech.voiceUUID), false, (ClipStatus status, Error error) =>
                {
                    if (error)
                        error.Log();
                    else
                    {
                        if (status.status == "OK")
                            AddClip(name, status.id);
                        else
                            Debug.LogError("Cannot create the clip, please try again later.");
                    }
                });
            }
        }

        private void RebuildClipList()
        {
            list = new ReorderableList(speech.clips, typeof(Clip), false, true, false, false);
            list.onSelectCallback += List_OnSelect;
            list.elementHeight = 28;
            list.drawElementCallback = List_DrawElement;
            list.drawHeaderCallback = List_DrawHeader;
        }

        public void AddClip(string name, string uuid)
        {
            Clip clip = CreateInstance<Clip>();
            clip.name = uuid + "-" + name;
            clip.uuid = uuid;
            clip.clipName = name;
            clip.text = new Text();
            AddClipToAsset(clip);
        }

        public void ImportClip(ResembleClip source)
        {
            Clip clip = CreateInstance<Clip>();
            clip.text = new Text();
            clip.uuid = source.uuid;
            clip.name = source.uuid + "-" + source.title;
            clip.clipName = source.title;
            clip.text.ParseResembleString(source.body);
            AddClipToAsset(clip);
        }

        public void AddClipToAsset(Clip clip)
        {
            //Add clip to list
            clip.speech = speech;
            speech.clips.Add(clip);
            speech.clips = speech.clips.OrderBy(x => x.clipName).ToList();

            //Add clip to assets
            AssetDatabase.AddObjectToAsset(clip, speech);
            EditorUtility.SetDirty(speech);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));

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
            GUI.Label(titleRect, speech.clips[index].clipName, EditorStyles.largeLabel);
            titleRect.Set(rect.x, rect.y + rect.height - 1, rect.width, 1.0f);
            EditorGUI.DrawRect(titleRect, Color.grey * 0.3f);
            bool haveClip = speech.clips[index].clip != null;

            float width = rect.width;
            rect.Set(width - 90, rect.y + 2, 50, rect.height - 4);
            if (Utils.FlatButton(rect, "Edit", Styles.clipGreenColor, 1.0f, rect.Contains(mousePosition) ? 0.5f : 0.2f))
                Selection.activeObject = speech.clips[index];
            rect.Set(width - 35, rect.y, 50, rect.height);
            Utils.DragArea(rect, speech.clips[index].clip);
            if (Utils.FlatButton(rect, "Clip", Styles.clipOrangeColor, 1.0f, haveClip ? (rect.Contains(mousePosition) ? 0.5f : 0.2f) : 1.0f) && haveClip)
                Selection.activeObject = speech.clips[index].clip;
        }

        private void List_DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Clips");
        }

        private void DrawSpeechProperties()
        {
            //Voice field
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Voice", GUILayout.Width(EditorGUIUtility.labelWidth - 2));
            if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(speech.voiceName) ? "None" : speech.voiceName), FocusType.Passive))
            {
                VoicePopup.Show(voiceRect, (Voice voice) =>
                {
                    speech.voiceName = voice.name;
                    speech.voiceUUID = voice.uuid;
                    EditorUtility.SetDirty(speech);
                });
            }

            //Set rect for popup
            if (Event.current.type == EventType.Repaint)
                voiceRect = GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect().Offset(0, 18, 0, 100));
            EditorGUILayout.EndHorizontal();

            //Begin Change Check
            EditorGUI.BeginChangeCheck();

            //Include phonemes
            speech.includePhonemes = EditorGUILayout.Toggle("Include phonemes", speech.includePhonemes);

            //Phoneme table
            if (speech.includePhonemes)
            {
                PhonemeTable temp = EditorGUILayout.ObjectField(phonemeTableLabel, speech.phonemeTable, typeof(PhonemeTable), false) as PhonemeTable;

                //Update phoneme table on clip when change
                if (temp != speech.phonemeTable)
                {
                    speech.phonemeTable = temp;
                    for (int i = 0; i < speech.clips.Count; i++)
                    {
                        if (speech.clips[i].havePhonemes)
                            speech.clips[i].phonemes.UpdateTable(temp);
                    }
                }
            }

            //Apply change if any
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(speech);

            GUILayout.Space(5);
            Utils.DrawSeparator();
        }

        public void ExportAllClipsInWav()
        {
            EditorUtility.SaveFolderPanel("Export wav files", "", "");
        }
    }
}