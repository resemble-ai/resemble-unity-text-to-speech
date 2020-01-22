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
        EditorGUILayout.HelpBox(message, MessageType.Error);
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

    public static bool FlatButton(Rect rect, string label, Color color, float roundness, float borderOnly)
    {
        Material mat = Resemble_Resources.instance.textMat;
        mat.SetColor("_Color", color);
        mat.SetFloat("_Roundness", roundness);
        mat.SetFloat("_Ratio", rect.width / rect.height);
        mat.SetFloat("_BorderOnly", borderOnly);
        EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, mat);
        return GUI.Button(rect, label, Styles.centredLabel);
    }

    public static bool FlatButtonLayout(string label, Color color, float roundness, float borderOnly)
    {
        GUIContent content = new GUIContent(label);
        float width = Styles.centredLabel.CalcSize(content).x;
        Rect rect = GUILayoutUtility.GetRect(width, 30);
        rect.Set(rect.x, rect.y, width, 30);
        Material mat = Resemble_Resources.instance.textMat;
        mat.SetColor("_Color", color);
        mat.SetFloat("_Roundness", roundness);
        mat.SetFloat("_Ratio", rect.width / rect.height);
        mat.SetFloat("_BorderOnly", borderOnly);
        EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, mat);
        return GUI.Button(rect, label, Styles.centredLabel);
    }

    public static void DragArea(Rect rect, Object dragAsset)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition))
        {
            DragAndDrop.PrepareStartDrag();

            DragAndDrop.paths = new string[] { AssetDatabase.GetAssetPath(dragAsset) };
            DragAndDrop.objectReferences = new Object[] { dragAsset };
            
            DragAndDrop.StartDrag(dragAsset.name);
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            DragAndDrop.activeControlID = GUIUtility.hotControl;
            Debug.Log("Begin drag " + DragAndDrop.objectReferences[0]);
            e.Use();
        }
        /*
        GUI.Box(rect, "Add Trigger");
        
        switch (e.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!rect.Contains(e.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();


                    foreach (Object dragged_object in DragAndDrop.objectReferences)
                    {
                        Debug.Log(AssetDatabase.GetAssetPath(dragged_object));
                    }
                }
                break;
        }*/
    }

    public static void ConnectionRequireMessage()
    {
        if (Resemble_Settings.haveProject)
            return;
        if (BoxWithLink("You are not connected to any Resemble project.", "Go to Resemble settings", MessageType.Error))
            OpenResembleSettings();
    }

    public static bool BoxWithLink(string message, string linkLabel, MessageType boxType)
    {
        EditorGUILayout.HelpBox(message + " \n", boxType);
        Rect rect = GUILayoutUtility.GetLastRect();
        int offset = EditorGUI.indentLevel * 16 + 42;
        rect.Set(rect.x + offset, rect.y + rect.height - 17, rect.width - offset, 16);
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        return GUI.Button(rect, linkLabel, Styles.linkStyleSmall);
    }

    public static void OpenResembleWebSite()
    {
        Application.OpenURL("https://www.resemble.ai");
    }

    public static void OpenResembleSettings()
    {
        Resemble_Settings.OpenWindow();
    }
}
