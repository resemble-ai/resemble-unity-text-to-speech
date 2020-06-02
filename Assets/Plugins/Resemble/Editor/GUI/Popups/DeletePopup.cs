using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    public class DeletePopup : EditorWindow
    {
        //GUI
        private static DeletePopup window;

        //Others
        private static string[] choices = new string[] { "Delete the asset only.", "Delete completely the clip from the project." };
        private static int selected;
        private static bool deleteAudioClip;
        private static ValidateCallback callback;
        public delegate void ValidateCallback(bool deleteOnAPI, bool deletedAudioClips);

        public static void Show(Rect rect, ValidateCallback callback)
        {
            //Close window if already open
            if (window != null)
            {
                Hide();
                return;
            }

            //Open window
            DeletePopup.callback = callback;
            window = CreateInstance<DeletePopup>();
            window.ShowPopup();
            window.minSize = new Vector2(100, 90);
            rect.height = 90;
            window.titleContent = new GUIContent("ChoicePopup");
            window.position = rect;
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
            GUILayout.Label("Delete clip");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            selected = EditorGUILayout.Popup(selected, choices);
            deleteAudioClip = EditorGUILayout.Toggle("Delete generated file", deleteAudioClip);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            if (GUILayout.Button("Confirm"))
            {
                callback.Invoke(selected == 1, deleteAudioClip);
                Close();
            }
            GUILayout.EndHorizontal();     
        }
    }
}