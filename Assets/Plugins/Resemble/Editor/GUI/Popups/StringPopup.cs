using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    public class StringPopup : EditorWindow
    {
        //GUI
        private static StringPopup window;

        //Others
        private static string value;
        private static string fieldTitle;
        private static ValidateCallback callback;
        public delegate void ValidateCallback(string value);

        public static void Show(Rect rect, string title, string value, ValidateCallback callback)
        {
            //Close window if already open
            if (window != null)
            {
                Hide();
                return;
            }

            //Open window
            StringPopup.value = value;
            StringPopup.fieldTitle = title;
            StringPopup.callback = callback;
            window = CreateInstance<StringPopup>();
            window.ShowPopup();
            window.minSize = new Vector2(100, 70);
            rect.height = 70;
            window.titleContent = new GUIContent("StringFieldPopup");
            window.position = rect;
        }

        public static void Show(Rect rect, string title, ValidateCallback callback)
        {
            Show(rect, title, "", callback);
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
            GUILayout.Label(fieldTitle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            value = GUILayout.TextField(value);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            if (GUILayout.Button("Confirm"))
            {
                callback.Invoke(value);
                Close();
            }
            GUILayout.EndHorizontal();     
        }
    }
}