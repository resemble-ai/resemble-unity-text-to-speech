using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public static class PodText_Drawer
    {
        public static bool OnGUI(this PodText pod, PlaceHolderAPIBridge.ClipRequest request)
        {
            //Init
            Styles.Load();
            GUILayout.Space(10);

            //Draw text area
            Rect textArea = GUILayoutUtility.GetRect(Screen.width, 190);
            textArea.Set(textArea.x, textArea.y + 40, textArea.width, 150);
            GUILayout.Space(-190);
            GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
            style.richText = true;
            TextEditor editor;
            OnGUI(pod, textArea, style, out editor);

            //Get cursor data
            int rawCursorIndex = editor.cursorIndex;
            int rawSelectIndex = editor.selectIndex;
            textArea.Set(-2, -2, textArea.width, textArea.height);
            int userCursorIndex = style.GetCursorStringIndex(textArea, new GUIContent(pod.userString), editor.graphicalCursorPos);
            int selectUserLength = PodText.RemoveTags(editor.SelectedText).Length;
            int userSelectIndex = userCursorIndex + (rawCursorIndex > rawSelectIndex ? -selectUserLength : selectUserLength);

            //Draw Tags
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 5; i++)
            {
                Emotion e = (Emotion)i;
                if (GUIUtils.FlatButtonLayout(e.ToString(), e.Color(), 1.0f, i == 0 ? 1.0f : 0.0f))
                    pod.SetTagToSelection(rawCursorIndex, rawSelectIndex, userCursorIndex, userSelectIndex, e);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(160);

            //Character count - Progress bar
            Rect rect = GUILayoutUtility.GetRect(1.0f, 16.0f);
            int length = pod.userString.Length;
            EditorGUI.ProgressBar(rect, length / 1000.0f, "Characters : " + length + "/1000");

            //Debug utils
            /*
            GUILayout.Label(pod.originString);
            textArea.Set(0, 0, textArea.width, textArea.height);
            GUILayout.Label(rawCursorIndex + "  " + rawSelectIndex);
            GUILayout.Label(userCursorIndex + "  " + userSelectIndex);
            GUILayout.Space(20);
            GUILayout.Label(editor.cursorIndex + "  " + editor.SelectedText);
            if (editor.hasSelection)
                GUILayout.Label(pod.HighlightedTags(editor.cursorIndex, editor.selectIndex));
                */
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(request != null || Settings.project == null);
            if (GUILayout.Button("Reset tags"))
            {
                pod.RemoveAllTags();
            }
            if (GUILayout.Button(request == null ? "Generate" : request.status.ToString()))
            {
                GUILayout.EndHorizontal();
                return true;
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            return false;
        }

        private static void OnGUI(this PodText pod, Rect rect, GUIStyle style, out TextEditor editor)
        {
            //Draw background
            GUI.Box(rect, "");
            rect.Set(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);

            //Get events
            /*
            Event e = Event.current;

            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.LeftArrow:
                        Debug.Log(pod.baseString[])
                        break;
                }
                Debug.Log(e.keyCode);
            }*/

            //Draw emotion rects
            Rect btnRect = new Rect();
            for (int i = 0; i < pod.tags.Length; i++)
            {
                if (pod.tags[i].emotion == Emotion.None)
                    continue;
                Vector2 start = style.GetCursorPixelPosition(rect, new GUIContent(pod.userString), pod.tags[i].startIndex);
                Vector2 end = style.GetCursorPixelPosition(rect, new GUIContent(pod.userString), pod.tags[i].endIndex);
                float heigth = style.CalcHeight(new GUIContent("|"), 1) - 4;
                btnRect.Set(start.x - 1, start.y, end.x - start.x + 2, heigth);
                Resources.instance.textMat.SetFloat("_Ratio", btnRect.width / btnRect.height);
                Resources.instance.textMat.SetFloat("_Roundness", 0.95f);
                Resources.instance.textMat.SetColor("_Color", pod.tags[i].emotion.Color());
                EditorGUI.DrawPreviewTexture(btnRect, Texture2D.whiteTexture, Resources.instance.textMat);
            }

            //Draw input field
            int controlID = GUIUtility.GetControlID("PodTextEditor".GetHashCode(), FocusType.Keyboard, rect);
            pod.originString = GUI.TextArea(rect, pod.originString, style);
            editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlID + 1);

            if (pod.carretPosition != editor.cursorIndex)
            {
                bool right = editor.cursorIndex > pod.carretPosition;

                if (editor.cursorIndex != pod.originString.Length && !editor.hasSelection)
                {
                    if (pod.originString[editor.cursorIndex] == '<' && right)
                    {
                        int dif = pod.originString.IndexOf('>', editor.cursorIndex) - editor.cursorIndex;
                        for (int i = 0; i <= dif; i++)
                            editor.MoveRight();
                    }
                    else if (pod.originString[editor.cursorIndex] == '>' && !right)
                    {
                        int id = pod.originString.LastIndexOf('<', editor.cursorIndex - 1, editor.cursorIndex - 1);
                        int dif = editor.cursorIndex - id;
                        for (int i = 0; i <= dif; i++)
                            editor.MoveLeft();
                    }
                }
            }

            pod.carretPosition = editor.cursorIndex;
        }
    }
}