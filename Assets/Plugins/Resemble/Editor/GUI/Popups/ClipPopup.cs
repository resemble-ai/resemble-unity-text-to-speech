using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public class ClipPopup : EditorWindow
    {
        //GUI
        private static ClipPopup window;
        private static Vector2 scroll;
        private static Error error;

        //Search
        private string search;
        private SearchField searchField;

        //Others
        private static string voiceUUID;
        private static ValidateCallback callback;
        private static ResembleClip[] clips;
        public delegate void ValidateCallback(ResembleClip clip);

        public static void Show(Rect rect, Speech speech, ValidateCallback callback)
        {
            //Close window if already open
            if (window != null)
            {
                Hide();
                return;
            }

            //Open window
            ClipPopup.callback = callback;
            ClipPopup.voiceUUID = speech.voiceUUID;
            clips = null;
            error = Error.None;
            window = CreateInstance<ClipPopup>();
            window.ShowPopup();
            window.minSize = rect.size;
            window.titleContent = new GUIContent("Voices");
            window.position = rect;

            //Request clips
            List<string> existingClips = speech.clips.Select(x => x.uuid).ToList();
            APIBridge.GetClips((ResembleClip[] clips, Error error) =>
            {
                ClipPopup.clips = clips.Where(x => x.voice == voiceUUID && !existingClips.Contains(x.uuid)).ToArray();
                ClipPopup.error = error;
                window.Repaint();
            });
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
            Hide();
        }

        public static void Hide()
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
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
            //Draw error if any
            if (error)
                error.DrawErrorBox();

            //Loading message - Wait API response
            if (clips == null)
            {
                EditorGUILayout.HelpBox("Load clips...", MessageType.None);
                return;
            }

            //No clip available
            if (clips.Length == 0)
            {
                EditorGUILayout.HelpBox("There are no more clips available with this voice.", MessageType.Warning);
                if (GUILayout.Button("Close"))
                    Hide();
                return;
            }

            //Draw clip list
            scroll = GUILayout.BeginScrollView(scroll, false, true);
            for (int i = 0; i < clips.Length; i++)
                DrawClip(clips[i]);
            GUILayout.EndScrollView();
        }

        /// <summary> Draw an asset and its children. </summary>
        private void DrawClip(ResembleClip clip)
        {
            //Apply search filter
            if (!string.IsNullOrEmpty(search) && !clip.title.Contains(search))
                return;

            //Draw button
            if (GUILayout.Button(clip.title))
            {
                callback.Invoke(clip);
                Hide();
            }
        }
    }
}