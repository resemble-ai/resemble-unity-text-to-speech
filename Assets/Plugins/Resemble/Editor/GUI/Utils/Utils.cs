using UnityEngine;
using UnityEditor;
using Resemble;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public static class Utils
    {
        public static void DrawPendingLabel(string label)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            GUILayout.Label(label, Styles.centredLabel);
            Rect rect = GUILayoutUtility.GetRect(25, 25);
            Material mat = Resources.instance.loadingMat;
            if (Event.current.type == EventType.Repaint)
            {
                System.DateTime now = System.DateTime.Now;
                float time = now.Millisecond / 1000.0f + now.Second;
                mat.SetFloat("_Progress", time);
                if (EditorGUIUtility.isProSkin)
                    mat.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f, 1.0f));
                else
                    mat.SetColor("_Color", new Color(0.0f, 0.0f, 0.0f, 1.0f));
            }
            EditorGUI.DrawPreviewTexture(rect, Resources.instance.loadingTex, mat);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void DrawErrorBox(this Error error, string message = null)
        {
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

        public static bool DrawLinkLabel(string label, string linkLabel, GUIStyle labelStyle, GUIStyle linkStyle)
        {
            return DrawLinkLabel(new GUIContent(label), new GUIContent(linkLabel), labelStyle, linkStyle);
        }

        public static bool DrawLinkLabel(string label, GUIContent linkLabel, GUIStyle labelStyle, GUIStyle linkStyle)
        {
            return DrawLinkLabel(new GUIContent(label), linkLabel, labelStyle, linkStyle);
        }

        public static bool DrawLinkLabel(GUIContent label, GUIContent linkLabel, GUIStyle labelStyle, GUIStyle linkStyle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(label, labelStyle);
            bool clic = GUILayout.Button(linkLabel, linkStyle);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return clic;
        }

        public static void DrawSeparator()
        {
            GUILayout.Space(10);
            Rect rect = GUILayoutUtility.GetRect(1.0f, 1.0f);
            rect.Set(rect.x + 10, rect.y, rect.width - 20, 1.0f);
            EditorGUI.DrawRect(rect, Color.grey);
        }

        public static void FlatBox(Rect rect, Color headerColor, Color backColor, float cornerSize, float headerHeight)
        {
            Material mat = Resources.instance.boxMat;
            mat.SetColor("_BackColor", backColor);
            mat.SetColor("_HeaderColor", headerColor);
            mat.SetVector("_Sizes", new Vector4(rect.width, rect.height, cornerSize, headerHeight));


            //mat.SetVector("_Clip", new Vector4(rect.width, rect.height, cornerSize, headerHeight));

            EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, mat);
        }

        public static void FlatRect(Rect rect, Color color, float roundness, float borderOnly)
        {
            Material mat = Resources.instance.textMat;
            mat.SetColor("_Color", color);
            mat.SetFloat("_Roundness", roundness);
            mat.SetFloat("_Ratio", rect.width / rect.height);
            mat.SetFloat("_BorderOnly", borderOnly);
            EditorGUI.DrawPreviewTexture(rect, Texture2D.whiteTexture, mat);
        }

        public static bool FlatButton(Rect rect, string label, Color color, float roundness, float borderOnly)
        {
            FlatRect(rect, color, roundness, borderOnly);
            return GUI.Button(rect, label, Styles.centredLabel);
        }

        public static bool FlatButton(Rect rect, GUIContent content, Color color, float roundness, float borderOnly)
        {
            FlatRect(rect, color, roundness, borderOnly);
            return GUI.Button(rect, content, Styles.centredLabel);
        }

        public static bool FlatButton(Rect rect, GUIContent content, Color color, bool enable)
        {
            bool mouseOver = rect.Contains(Event.current.mousePosition);
            FlatRect(rect, color, 1.0f, enable ? 0.0f : (mouseOver ? 1.0f : 0.8f));
            GUI.Button(rect, content, Styles.centredLabel);
            return mouseOver;
        }

        public static bool FlatButton(Rect rect, GUIContent content, Color color, ref ButtonState state)
        {
            Event e = Event.current;
            bool contains = (state == ButtonState.Over || state == ButtonState.Press);
            if(e.type == EventType.Repaint)
                contains = rect.Contains(e.mousePosition);

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (contains)
                        state = ButtonState.Press;
                    break;
                case EventType.MouseUp:
                        state = contains ? ButtonState.Over : ButtonState.None;
                    break;
                case EventType.Repaint:
                    if (contains && state == ButtonState.None)
                        state = ButtonState.Over;
                    else if (!contains && state == ButtonState.Over)
                        state = ButtonState.None;
                    break;
            }

            switch (state)
            {
                case ButtonState.None:
                    FlatRect(rect, color, 1.0f, 0.0f);
                    break;
                case ButtonState.Over:
                    FlatRect(rect, color, 1.0f, 0.2f);
                    break;
                case ButtonState.Press:
                    FlatRect(rect, color, 0.5f, 0.5f);
                    break;
                case ButtonState.Disable:
                    color.a = 0.6f;
                    FlatRect(rect, color, 1.0f, 0.2f);
                    break;
            }

            rect = rect.Shrink(3);
            if (state == ButtonState.Disable)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Button(rect, content, Styles.centredLabel);
                EditorGUI.EndDisabledGroup();
                return false;
            }
            return GUI.Button(rect, content, Styles.centredLabel);
        }

        public static bool FlatButton(Rect rect, string label, Color color)
        {
            FlatRect(rect, color, 1.0f, rect.Contains(Event.current.mousePosition) ? 0.8f : 1.0f);
            return GUI.Button(rect, label, Styles.centredLabel);
        }

        public static bool FlatButton(GUIContent label, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(label, Styles.centredLabel);
            FlatRect(rect, color, 1.0f, rect.Contains(Event.current.mousePosition) ? 0.8f : 1.0f);
            return GUI.Button(rect, label, Styles.centredLabel);
        }

        public static bool FlatButtonLayout(string label, Color color, float roundness, float borderOnly)
        {
            return FlatButtonLayout(new GUIContent(label), color, roundness, borderOnly);
        }

        public static bool FlatButtonLayout(GUIContent content, Color color, ref ButtonState state)
        {
            Rect rect = GUILayoutUtility.GetRect(Styles.centredLabel.CalcSize(content).x, 30);
            return FlatButton(rect, content, color, ref state);
        }

        public static bool FlatButtonLayout(Texture2D texture, Color color, float roundness, float borderOnly)
        {
            float width = texture.width;
            Rect rect = GUILayoutUtility.GetRect(width, 30);
            rect.Set(rect.x, rect.y, width, 30);
            FlatRect(rect, color, roundness, borderOnly);
            rect.y += 3;
            rect.height -= 6;
            return GUI.Button(rect, texture, Styles.centredLabel);
        }

        public static bool FlatButtonLayout(GUIContent content, Color color, float roundness, float borderOnly)
        {
            float width = Styles.centredLabel.CalcSize(content).x;
            Rect rect = GUILayoutUtility.GetRect(width, 30);
            rect.Set(rect.x, rect.y, width, 30);
            FlatRect(rect, color, roundness, borderOnly);
            return GUI.Button(rect, content, Styles.centredLabel);
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
            if (Settings.haveProject)
                return;
            if (BoxWithLink("You are not connected to any Resemble project.", "Go to Resemble settings", MessageType.Error))
            {
                RessembleSettingsProvider.pageID = 0;
                Settings.OpenWindow();
            }
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

        public static void BoxWithLink(string message, string linkLabel, WebPage page, MessageType boxType)
        {
            EditorGUILayout.HelpBox(message + " \n", boxType);
            Rect rect = GUILayoutUtility.GetLastRect();
            int offset = EditorGUI.indentLevel * 16 + 42;
            rect.Set(rect.x + offset, rect.y + rect.height - 17, rect.width - offset, 16);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, linkLabel, Styles.linkStyleSmall))
                page.Open();
        }

        public static Rect Shrink(this Rect rect, float size)
        {
            rect.Set(rect.x + size, rect.y + size, rect.width - size * 2, rect.height - size * 2);
            return rect;
        }

        public static Rect Offset(this Rect rect, float x, float y, float width, float height)
        {
            rect.Set(rect.x + x, rect.y + y, rect.width + width, rect.height + height);
            return rect;
        }

        public static Rect Drop(this Rect rect)
        {
            rect.Set(rect.x, rect.y + rect.height, 1, 1);
            return rect;
        }

        public static void OpenProjectInBrowser(string projectID)
        {
            throw new System.Exception();
        }

        public static string LocalPath(string absolutePath)
        {
            absolutePath = absolutePath.Replace("\\", "/");
            if (absolutePath.Contains(Application.dataPath))
                return absolutePath.Remove(0, Application.dataPath.Length - 6);
            return absolutePath;
        }

        public static bool LocalPath(ref string path)
        {
            if (!path.Contains(Application.dataPath))
                return false;
            path = path.Remove(0, Application.dataPath.Length - 6);
            return true;
        }

        public static Color Alpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static int FindPreviousIndex(float value, float[] array)
        {
            if (value < array[0])
                return -1;

            int start = 0;
            int end = array.Length;
            int mid;

            while (end - start > 1)
            {
                mid = (start + end) / 2;
                if (value > array[mid])
                    start = mid;
                else
                    end = mid;
            }

            mid = (start + end) / 2;
            return mid;
        }

        public enum ButtonState
        {
            None,
            Over,
            Press,
            Disable,
        }

    }
}