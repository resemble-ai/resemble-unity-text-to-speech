using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public class VoicePopup : EditorWindow
    {
        //GUI
        private static VoicePopup window;
        private static Vector2 scroll;

        //Search
        private string search;
        private SearchField searchField;

        //Others
        private static ValidateCallback callback;
        private static Voice[] voices;
        public delegate void ValidateCallback(Voice voice);

        public static void Show(Rect rect, ValidateCallback callback)
        {
            //Close window if already open
            if (window != null)
            {
                window.Close();
                return;
            }

            //Open window
            VoicePopup.callback = callback;
            Settings.OnRefreshVoices += OnRefreshVoices;
            Settings.RefreshVoices();
            voices = Settings.voices;
            window = CreateInstance<VoicePopup>();
            window.ShowPopup();
            window.titleContent = new GUIContent("Voices");
            window.position = rect;
        }

        private static void OnRefreshVoices()
        {
            voices = Settings.voices;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.window);
            DrawHeader();
            DrawContent();
            GUILayout.EndVertical();
        }

        private void OnLostFocus()
        {
            Close();
        }

        private new void Close()
        {
            window = null;
            Settings.OnRefreshVoices -= OnRefreshVoices;
            base.Close();
        }

        private void DrawHeader()
        {
            GUILayout.Space(-19);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //search bar
            if (searchField == null)
                searchField = new SearchField();
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 16).Offset(5, 2, -13, 0);
            search = searchField.OnToolbarGUI(rect, search);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true);

            for (int i = 0; i < voices.Length; i++)
                DrawVoice(voices[i]);

            GUILayout.EndScrollView();
        }

        /// <summary> Draw an asset and its children. </summary>
        private void DrawVoice(Voice voice)
        {
            //Apply search filter
            if (!string.IsNullOrEmpty(search) && !voice.name.Contains(search))
                return;

            //Draw button
            if (GUILayout.Button(voice.name))
            {
                callback.Invoke(voice);
                Close();
            }
        }
    }
}