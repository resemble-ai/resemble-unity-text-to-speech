using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Resemble
{
    [CustomEditor(typeof(CharacterSet))]
    public class CharacterSet_Editor : Editor
    {
        private CharacterSet characterSet;
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
            characterSet = target as CharacterSet;

            //Bar
            Rect rect = new Rect(0, 0, Screen.width, 46);
            GUI.Box(rect, "", Styles.bigTitle);

            //Icon
            rect.Set(6, 6, 32, 32);
            GUI.DrawTexture(rect, Resources.instance.icon);

            //Title
            rect.Set(44, 4, Screen.width - 100, 20);
            GUI.Label(rect, characterSet.name, EditorStyles.largeLabel);

            //Pod count
            rect.Set(44, 23, Screen.width - 130, 16);
            GUI.Label(rect, "Pod count : " + characterSet.pods.Count);

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
                menu.AddItem(new GUIContent("Reset"), false, ResetCharacterSet);
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
                list = new ReorderableList(characterSet.pods, typeof(Clip), false, true, false, false);
                list.onSelectCallback += List_OnSelect;
                list.elementHeight = 28;
                list.drawElementCallback = List_DrawElement;
                list.drawHeaderCallback = List_DrawHeader;
            }
            mp = Event.current.mousePosition;
            list.DoLayoutList();
            Repaint();

            //Add pod btn
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add new pod"))
            {
                Clip pod = CreateInstance<Clip>();
                pod.name = "New Pod";
                pod.set = characterSet;
                pod.autoRename = true;
                AssetDatabase.AddObjectToAsset(pod, characterSet);
                EditorUtility.SetDirty(characterSet);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(characterSet), ImportAssetOptions.ForceUpdate);
                characterSet.pods.Add(pod);
                characterSet.pods = characterSet.pods.OrderBy(x => x.name).ToList();
                Selection.activeObject = pod;
            }
            GUILayout.EndHorizontal();

            //Draw connected error
            GUIUtils.ConnectionRequireMessage();
        }

        private void OnEnable()
        {
            characterSet = target as CharacterSet;
            int count = characterSet.pods.Count;
            for (int i = 0; i < count; i++)
            {
                if (!AssetDatabase.IsSubAsset(characterSet.pods[i]))
                {
                    characterSet.pods.RemoveAt(i);
                    count--;
                    i--;
                }
            }
        }

        private void List_OnSelect(ReorderableList list)
        {
            EditorGUIUtility.PingObject(characterSet.pods[list.index]);
        }

        private void List_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect titleRect = new Rect(rect.x + 20, rect.y + 2, rect.width - 100, rect.height);
            GUI.Label(titleRect, characterSet.pods[index].name, EditorStyles.largeLabel);
            titleRect.Set(rect.x, rect.y + rect.height - 1, rect.width, 1.0f);
            EditorGUI.DrawRect(titleRect, Color.grey * 0.3f);
            bool haveClip = characterSet.pods[index].clipCopy != null;

            float width = rect.width;
            rect.Set(width - 90, rect.y + 2, 50, rect.height - 4);
            if (GUIUtils.FlatButton(rect, "Pod", Styles.podColor, 1.0f, rect.Contains(mp) ? 0.5f : 0.2f))
                Selection.activeObject = characterSet.pods[index];
            rect.Set(width - 35, rect.y, 50, rect.height);
            GUIUtils.DragArea(rect, characterSet.pods[index].clip);
            if (GUIUtils.FlatButton(rect, "Clip", Styles.clipColor, 1.0f, haveClip ? (rect.Contains(mp) ? 0.5f : 0.2f) : 1.0f) && haveClip)
                EditorGUIUtility.PingObject(characterSet.pods[index].clipCopy);
        }

        private void List_DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Pods");
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.Popup("Voice", 0, new string[] { "Lucie", "Jhon", "OldMen" });
            EditorGUI.indentLevel++;
            characterSet.pitch = (CharacterSet.Tuning)EditorGUILayout.EnumPopup("Pitch", characterSet.pitch);
            characterSet.speed = (CharacterSet.Tuning)EditorGUILayout.EnumPopup("Speed", characterSet.speed);
            EditorGUI.indentLevel--;

            GUILayout.Space(5);
            GUIUtils.DrawSeparator();
        }

        public void RegenerateAllPods()
        {

        }

        public void ExportAllPodsInWav()
        {
            EditorUtility.SaveFolderPanel("Export wav files", "", "");
        }

        public void ResetCharacterSet()
        {
            if (EditorUtility.DisplayDialog("Reset Character set", "This action will destroy all pods under \"" + characterSet.name + "\" CharacterSet and can't be undo !", "Ok", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(characterSet);
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