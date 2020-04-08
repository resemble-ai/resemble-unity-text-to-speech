using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(APIGraph))]
public class APIGraph_Editor : Editor
{

    private APIGraph graph;
    private Event e;
    private GUIStyle phonemePreview;
    private int highlight = -1;
    private int dragContainer = -1;
    private bool releaseDrag;
    private double time;
    private string focusPhoneme;
    private bool addVowelRequest;
    private Vector2 addVowelFieldPos;
    private string addVowelFieldContent;
    private Color graphBgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    private Color selectColor = new Color(1.0f, 1.3f, 2.0f, 1.0f);
    private Vector2[] vowelsGraphPoints = new Vector2[] {
        new Vector2(0.090f, 0.100f), new Vector2(0.180f, 0.325f), new Vector2(0.270f, 0.550f), new Vector2(0.360f, 0.775f),
        new Vector2(0.495f, 0.100f), new Vector2(0.540f, 0.325f), new Vector2(0.585f, 0.550f), new Vector2(0.630f, 0.775f),
        new Vector2(0.900f, 0.100f), new Vector2(0.900f, 0.325f), new Vector2(0.900f, 0.550f), new Vector2(0.900f, 0.775f),
    };
    private int[] vowelsGraphLines = new int[]
    {
        0, 1, 1, 2, 2, 3,
        4, 5, 5, 6, 6, 7,
        8, 9, 9, 10, 10, 11,
        0, 4, 4, 8,
        1, 5, 5, 9,
        2, 6, 6, 10,
        3, 7, 7, 11
    };
    private Vector2[] vowelsFold = new Vector2[28]; 

    public override void OnInspectorGUI()
    {
        graph = target as APIGraph;
        e = Event.current;

        //Vowels
        Rect rect = DrawVowelsGraph();
        DrawPhonemePreview(rect);
        VowelsContextMenu(rect);

        //Consonants
        DrawConsonants();
    }

    private Rect DrawVowelsGraph()
    {
        //Get area & draw background
        GUILayout.Label("Vowels" + focusPhoneme, EditorStyles.largeLabel);
        Rect rect = Shrink(GUILayoutUtility.GetAspectRect(1.0f), 10);
        EditorGUI.DrawRect(rect, Color.black);
        rect = Shrink(rect, 1);
        EditorGUI.DrawRect(rect, graphBgColor);


        //Place points
        Vector3[] points = new Vector3[vowelsGraphPoints.Length];
        for (int i = 0; i < points.Length; i++)
            points[i] = Rect.NormalizedToPoint(rect, vowelsGraphPoints[i]);


        //Draw lines
        Handles.color = Color.gray;
        Handles.DrawLines(points, vowelsGraphLines);


        //Draw labels
        GUI.Label(RectFromPoint(points[0], 0, - 15), "Front");
        GUI.Label(RectFromPoint(points[4], 0, - 15), "Central");
        GUI.Label(RectFromPoint(points[8], 0, - 15), "Back");

        GUI.Label(RectFromPoint(points[0], -45, 0), "Close");
        GUI.Label(RectFromPoint(points[1], -70, 0), "Close-mid");
        GUI.Label(RectFromPoint(points[2], -70, 0), "Open-mid");
        GUI.Label(RectFromPoint(points[3], -45, 0), "Open");


        //Get deltaTime
        float delta = (float)(EditorApplication.timeSinceStartup - time) * 4;
        time = EditorApplication.timeSinceStartup;


        //Drag stuff
        if (highlight >= graph.items.Count)
            highlight = -1;
        if (dragContainer >= graph.items.Count)
            dragContainer = -1;
        if (dragContainer != -1 && dragContainer < graph.items.Count)
            focusPhoneme = graph.items[dragContainer].phonemes;
        else
            focusPhoneme = "";
        if (e.type == EventType.MouseUp && dragContainer != -1)
            releaseDrag = true;


        //Draw containers
        for (int i = 0; i < graph.items.Count; i++)
        {
            DrawContainer(rect, graph.items[i], i, delta);
        }
        if (releaseDrag)
        {
            dragContainer = -1;
            releaseDrag = false;
        }

        //Draw cross cursor
        if (dragContainer != -1)
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);

        Repaint();
        return rect;
    }

    private void DrawPhonemePreview(Rect rect)
    {
        rect.Set(rect.x + 5, rect.y + rect.height - 34, rect.width - 10, 34);
        if (phonemePreview == null)
        {
            phonemePreview = new GUIStyle(EditorStyles.largeLabel);
            phonemePreview.fontSize = 24;
        }
        GUI.Label(rect, focusPhoneme, phonemePreview);
    }

    private void VowelsContextMenu(Rect rect)
    {
        if (e.type == EventType.ContextClick)
        {
            GenericMenu menu = new GenericMenu();
            if (highlight == -1)
            {
                menu.AddItem(new GUIContent("Add characters"), false, () => {
                    addVowelRequest = true;
                    addVowelFieldPos = e.mousePosition - rect.min;
                });
                menu.AddDisabledItem(new GUIContent("Remove characters"));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Add characters"));
                menu.AddItem(new GUIContent("Remove characters"), false, () => {
                    Undo.RecordObject(target, "Remove character");
                    graph.items.RemoveAt(highlight);
                    highlight = -1;
                    EditorUtility.SetDirty(target);
                });
            }
            menu.ShowAsContext();
            e.Use();
        }


        //Add field
        if (addVowelRequest)
        {
            Rect r = new Rect(addVowelFieldPos.x + 50, addVowelFieldPos.y - 20, 100, 20);
            GUI.Box(r, "", GUI.skin.window);
            r.Set(r.x + 3, r.y + 2, 70, 16);
            addVowelFieldContent = GUI.TextField(r, addVowelFieldContent);
            r.Set(r.x + 72, r.y, 24, 16);
            if (GUI.Button(r, "Ok"))
            {
                Undo.RecordObject(target, "Add characters");
                addVowelRequest = false;
                if (addVowelFieldContent.Length > 0)
                    graph.items.Add(new APIGraph.PhoItem() {
                        phonemes = addVowelFieldContent,
                        vowelPos = Rect.PointToNormalized(rect, addVowelFieldPos) });
                addVowelFieldContent = "";
                EditorUtility.SetDirty(target);
            }
        }

    }

    private void DrawConsonants()
    {
        GUILayout.Label("Consonants", EditorStyles.largeLabel);

    }

    private void DrawContainer(Rect rect, APIGraph.PhoItem item, int id, float delta)
    {
        int count = item.phonemes.Length;
        float itemsHeight = (count - 1) * 16;
        Rect r = new Rect(Rect.NormalizedToPoint(rect, item.vowelPos), Vector2.one * 40);
        Rect ro = r; if (id == highlight) ro.height += itemsHeight;
        bool over = (e.type == EventType.Repaint ? ro.Contains(e.mousePosition) : id == highlight) &&
            dragContainer != id;
        if (highlight == id && !over)
                highlight = -1;

        bool dragOver = over && dragContainer != -1 && dragContainer != id;
        if (dragOver)
        {
            if (releaseDrag)
            {
                Undo.RecordObject(target, "Merge characters");
                string phoneme = graph.items[dragContainer].phonemes;
                graph.items.RemoveAt(dragContainer);
                item.phonemes += phoneme;
                dragContainer = -1;
                highlight = -1;
                EditorUtility.SetDirty(target);
                return;
            }
        }

        vowelsFold[id].y += ((id == highlight ? 1.0f : 0.0f) - vowelsFold[id].x) * 0.01f;
        vowelsFold[id].y *= 0.9f;
        vowelsFold[id].x += vowelsFold[id].y;

        //Stay
        if (id == highlight)
        {
        }

        //Enter
        if (over && highlight == -1)
        {
            highlight = id;
        }

        //Exit
        if (!over && id == highlight)
        {
            highlight = -1;
        }

        //Drag
        if (dragContainer == id && e.type == EventType.Repaint)
        {
            Undo.RecordObject(target, "Move characters");
            item.vowelPos = Rect.PointToNormalized(rect, e.mousePosition);
            EditorUtility.SetDirty(target);
        }

        float t = vowelsFold[id].x;
        r.height += t * itemsHeight;
        GUI.Box(r, "", GUI.skin.window);
        GUI.color = Color.white;

        //Draw phonemes buttons
        GUI.BeginClip(r);
        r.Set(1, 1, r.width - 2, 10);
        if (GUI.RepeatButton(r, dragOver ? "---" : "") && e.button == 0)
        {
            dragContainer = id;
        }

        r.Set(0, 16, r.width - 8, 16);
        for (int j = 0; j < count; j++)
        {
            r.Set(5, j * 16 + 17, r.width, 16);
            string label = item.phonemes[j].ToString();
            if (GUI.RepeatButton(r, label, GUI.skin.button) && dragContainer == -1 && e.button == 0)
            {
                if (count > 1)
                {
                    Undo.RecordObject(target, "Extract character");
                    item.phonemes = item.phonemes.Replace(label, "");
                    graph.items.Add(new APIGraph.PhoItem() { vowelPos = item.vowelPos, phonemes = label });
                    dragContainer = graph.items.Count - 1;
                    EditorUtility.SetDirty(target);
                    break;
                }
                else
                {
                    dragContainer = id;
                }
            }
        }
        GUI.EndClip();
    }

    #region Utils
    private Rect Shrink(Rect rect, float size)
    {
        rect.Set(rect.x + size, rect.y + size, rect.width - size * 2, rect.height - size * 2);
        return rect;
    }

    private Rect RectFromPoint(Vector2 point, float offsetX = 0.0f, float offsetY = 0.0f)
    {
        return new Rect(point.x + offsetX, point.y - 8 + offsetY, 100, 16);
    }
    #endregion
}
