using UnityEngine;
using UnityEditor;
using Resemble.GUIEditor;
using Resemble;

public class Phonemes_Editor : Editor
{
    //private Phonemes phonemes;
    public static Color graphBgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public static Color transparent = new Color(0.0f, 0.0f, 0.0f, 0.0f);

    public delegate void Callback();
    public delegate void CallbackIndex(Rect rect, int index);

    public static void DrawGraph(Phonemes phonemes, ref float time, Callback repaint, CallbackIndex onDrawItem)
    {
        //Get area space
        Rect rect = GUILayoutUtility.GetRect(Screen.width, 300).Shrink(10);
        GUI.Box(rect, "", Styles.whiteFrame);
        rect = rect.Shrink(2);
        Event e = Event.current;
        Styles.Load();


        //Draw top rect
        Rect topRect = new Rect(rect.x, rect.y, rect.width, 18);
        EditorGUI.DrawRect(topRect, graphBgColor);

        //Draw background
        rect = rect.Offset(0, 20, 0, -20);
        EditorGUI.DrawRect(rect, graphBgColor);


        //Handle setTime events
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && rect.Contains(e.mousePosition))
        {
            time = Rect.PointToNormalized(rect, e.mousePosition).x;
            repaint.Invoke();
        }


        //Draw cursor
        Rect cursorRect = new Rect(rect.x + rect.width * time, rect.y, 2, rect.height);
        EditorGUI.DrawRect(cursorRect, Color.white);


        //Draw curves and fields
        bool haveCustomItem = onDrawItem != null;
        float width = Screen.width - EditorGUIUtility.labelWidth - (haveCustomItem ? 180 : 50);
        float maxValue = 0.0f;
        string maxValueName = "";
        Color maxValueColor = Color.white;
        for (int i = 0; i < phonemes.curves.Length; i++)
        {
            Color curveColor = Color.HSVToRGB((i * 0.13f) % 1.0f, 0.8f, 1.0f);
            EditorGUIUtility.DrawCurveSwatch(rect, phonemes.curves[i].curve, null, curveColor, transparent, new Rect(0, 0, 1, 1));

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(phonemes.curves[i].name, GUILayout.Width(EditorGUIUtility.labelWidth));

            Rect barRect = GUILayoutUtility.GetRect(width, 16);
            barRect.x -= 10;
            barRect.width = width;
            GUI.Box(barRect, "", Styles.whiteFrame);
            float value = phonemes.curves[i].curve.Evaluate(time);
            Rect fillRect = barRect;
            fillRect.width *= Mathf.Clamp01(value);

            if (value > maxValue)
            {
                maxValue = value;
                maxValueName = phonemes.curves[i].name;
                maxValueColor = curveColor;
            }

            fillRect = fillRect.Shrink(1);
            EditorGUI.DrawRect(fillRect, curveColor);

            if (haveCustomItem)
            {
                barRect.Set(barRect.x + barRect.width + 10, barRect.y, 120, barRect.height);
                onDrawItem.Invoke(barRect, i);
            }


            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }


        //Draw maxValue top label
        if (maxValue > 0.0001f)
        {
            topRect.x = cursorRect.x - 10;
            Color guiColor = GUI.color;
            GUI.color = maxValueColor;
            GUI.Label(topRect, maxValueName, Styles.whiteLabel);
            GUI.color = guiColor;
        }
    }
}
