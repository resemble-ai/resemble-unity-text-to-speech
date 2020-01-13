using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PodText_Drawer
{
    public static void OnGUI(this PodText pod)
    {
        GUILayout.Space(10);

        //Get toolbar space
        Rect toolbar = GUILayoutUtility.GetRect(Screen.width, 20);
        GUILayout.Space(5);

        //Draw text area
        Rect textArea = GUILayoutUtility.GetRect(Screen.width, 150);
        GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
        style.richText = true;
        TextEditor editor;
        OnGUI(pod, textArea, style, out editor);

        //Debug utils
        GUILayout.Label(editor.cursorIndex + "  " + editor.SelectedText);
        if (editor.hasSelection)
            GUILayout.Label(pod.HighlightedTags(editor.cursorIndex, editor.selectIndex));

        //Draw toolbar
        //GUILayout.BeginArea(toolbar);
        EditorGUI.BeginDisabledGroup(!editor.hasSelection);
        toolbar.width = 100;
        if (GUI.Button(toolbar, "Add angry emotion"))
            pod.SetTagToSelection(editor.selectIndex, editor.cursorIndex, PodText.Emotion.Angry);
        EditorGUI.EndDisabledGroup();
        toolbar.x += 110;
        if (GUI.Button(toolbar, "Remove all tags"))
        {
            pod.RemoveAllTags();
            Debug.Log(pod.tags.Length);
        }
        //GUILayout.EndArea();
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
            if (pod.tags[i].emotion == PodText.Emotion.Neutral)
                continue;
            Vector2 start = style.GetCursorPixelPosition(rect, new GUIContent(pod.userString), pod.tags[i].startIndex);
            Vector2 end = style.GetCursorPixelPosition(rect, new GUIContent(pod.userString), pod.tags[i].endIndex);
            float heigth = style.CalcHeight(new GUIContent("|"), 1) - 4;
            btnRect.Set(start.x - 1, start.y, end.x - start.x + 2, heigth);
            Resemble_Resources.instance.textMat.SetFloat("_Ratio", btnRect.width / btnRect.height);
            EditorGUI.DrawPreviewTexture(btnRect, Texture2D.whiteTexture, Resemble_Resources.instance.textMat);
        }

        //Draw input field
        pod.originString = GUI.TextArea(rect, pod.originString, style);
        editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

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
