using UnityEngine;
using UnityEditor;

public static class GUIUtils
{
    public static void DrawErrorBox(this Error error, string message = null)
    {
        if (!error)
            return;

        if (string.IsNullOrEmpty(message))
        {
            message = error.code < 0 ? error.message :
                string.Format("Error {0} : {1}", error.code, error.message);
        }
        else
        {
            message = error.code < 0 ?
                string.Format("{0}\n{1}", message, error.message) :
                string.Format("{0}\nError {1} : {2}", message, error.code, error.message);
        }
        EditorGUILayout.HelpBox(message, UnityEditor.MessageType.Error);
    }

    public static void DrawLinkLabel(string label, string linkLabel, string url, GUIStyle labelStyle, GUIStyle linkStyle)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label(label, labelStyle);
        if (GUILayout.Button(linkLabel, linkStyle))
            Application.OpenURL(url);
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public static void DrawSeparator()
    {
        GUILayout.Space(10);
        Rect rect = GUILayoutUtility.GetRect(1.0f, 1.0f);
        rect.Set(rect.x + 10, rect.y, rect.width - 20, 1.0f);
        EditorGUI.DrawRect(rect, Color.grey);
    }
}
