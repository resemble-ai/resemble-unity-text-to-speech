using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Resemble;

namespace Resemble.GUIEditor
{
    [CustomEditor(typeof(Speech))]
    public class Speech_Editor : Editor
    {
        private Speech speech;
        private ReorderableList list;
        public AudioClip clip;
        private Vector2 mp;

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
            GUI.Label(rect, "Pod count : " + speech.clips.Count);

            //Resemble.ai link
            rect.Set(Screen.width - 117, rect.y + 5, 125, 16);
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
                menu.AddItem(new GUIContent("Regenerate all pods"), false, RegenerateAllPods);
                menu.AddItem(new GUIContent("Export all pods in wav"), false, ExportAllPodsInWav);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset"), false, ResetSpeech);
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
            if (GUILayout.Button("Add new clip"))
            {
                Clip clip = CreateInstance<Clip>();
                clip.name = "New clip";
                clip.speech = speech;
                clip.autoRename = true;
                AssetDatabase.AddObjectToAsset(clip, speech);
                EditorUtility.SetDirty(speech);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(speech), ImportAssetOptions.ForceUpdate);
                speech.clips.Add(clip);
                speech.clips = speech.clips.OrderBy(x => x.name).ToList();
                Selection.activeObject = clip;
            }
            GUILayout.EndHorizontal();

            //Draw connected error
            Utils.ConnectionRequireMessage();
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
            bool haveClip = speech.clips[index].clipCopy != null;

            float width = rect.width;
            rect.Set(width - 90, rect.y + 2, 50, rect.height - 4);
            if (Utils.FlatButton(rect, "Edit", Styles.clipGreenColor, 1.0f, rect.Contains(mp) ? 0.5f : 0.2f))
                Selection.activeObject = speech.clips[index];
            rect.Set(width - 35, rect.y, 50, rect.height);
            Utils.DragArea(rect, speech.clips[index].clip);
            if (Utils.FlatButton(rect, "AudioClip", Styles.clipOrangeColor, 1.0f, haveClip ? (rect.Contains(mp) ? 0.5f : 0.2f) : 1.0f) && haveClip)
                EditorGUIUtility.PingObject(speech.clips[index].clipCopy);
        }

        private void List_DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Clips");
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Voice");
            EditorGUILayout.Space(EditorGUIUtility.labelWidth);
            if (EditorGUILayout.DropdownButton(new GUIContent("Current"), FocusType.Passive))
            {
                //Settings.voic
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Popup("Voice", 0, new string[] { "Lucie", "Jhon", "OldMen" });

            EditorGUI.indentLevel++;
            speech.pitch = (Speech.Tuning)EditorGUILayout.EnumPopup("Pitch", speech.pitch);
            speech.speed = (Speech.Tuning)EditorGUILayout.EnumPopup("Speed", speech.speed);
            EditorGUI.indentLevel--;

            GUILayout.Space(5);
            Utils.DrawSeparator();
        }

        public void RegenerateAllPods()
        {

        }

        public void ExportAllPodsInWav()
        {
            EditorUtility.SaveFolderPanel("Export wav files", "", "");
        }

        public void ResetSpeech()
        {
            if (EditorUtility.DisplayDialog("Reset Character set", "This action will destroy all pods under \"" + speech.name + "\" CharacterSet and can't be undo !", "Ok", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(speech);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null)
                {
                    AssetDatabase.RemoveObjectFromAsset(clip);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }

    }
}