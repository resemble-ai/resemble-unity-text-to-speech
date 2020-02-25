using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    public class Text_Editor
    {

        //Target
        public Text target;
        public bool dirty;

        //Text edition stuff
        private string userString
        {
            get
            {
                if (target == null || target.userString == null)
                    return "";
                return target.userString;
            }
            set
            {
                target.userString = value;
                lines = null;
            }
        }
        private int length
        {
            get
            {
                return userString.Length;
            }
        }
        private Rect[] rects;
        private LinesPack lines;
        private double lastInputTime;
        private bool drag;
        private Vector2 lastClicPos;
        private double lastClicTime;
        private bool haveSelection
        {
            get
            {
                return carretID != selectID;
            }
        }
        private int carretID;
        private int selectID
        {
            get
            {
                return _selectID;
            }
            set
            {
                if (_selectID != value)
                {
                    _selectID = value;
                    if (selectID != carretID)
                    {
                        RefreshSelectionRects();
                    }
                    else
                        rects = null;
                }
            }
        }
        private int _selectID = 0;
        private bool focus;

        //GUI stuff
        private int clickCount;
        private Rect textRect;
        private Rect clipRect;
        private Vector2 scroll;
        private GUIContent userStringGUIContent
        {
            get
            {
                return new GUIContent(userString);
            }
        }

        Utils.ButtonState breakBtn;
        Utils.ButtonState emotionBtn;


        public delegate void DrawerEvent();


        #region Draw functions

        public void DoLayout(bool enable, DrawerEvent onEditCallback, DrawerEvent onGenerateCallback)
        {
            DrawTagsBtnsLayout(!enable);

            Rect rect = GUILayoutUtility.GetRect(Screen.width, 300).Shrink(10);
            DrawTextArea(rect, Settings.haveProject);
            if (dirty)
                onEditCallback.Invoke();

            if (GUILayout.Button("Generate"))
                onGenerateCallback.Invoke();
        }

        public void DrawTagsBtnsLayout(bool disabled)
        {
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 50).Shrink(10);
            Rect btnRect = rect;

            //Update disable state of buttons based on selection
            breakBtn = DisableIf(haveSelection, breakBtn);
            emotionBtn = DisableIf(!haveSelection, emotionBtn);

            //Break button
            btnRect.width = 60;
            Utils.FlatButton(btnRect, new GUIContent(Resources.instance.breakIco), new Color(0.956f, 0.361f, 0.259f), ref breakBtn);

            //Emotion button
            btnRect.Set(btnRect.x + btnRect.width + 5, btnRect.y, 130, btnRect.height);
            if (Utils.FlatButton(btnRect, new GUIContent("Add Emotion"), new Color(0.259f, 0.6f, 0.956f), ref emotionBtn))
            {
                Tag tag = ApplyTag(Emotion.Angry);
                selectID = carretID;
                TagPopup.Show(GetTagPos(tag), tag, true);
            }
        }

        private Utils.ButtonState DisableIf(bool value, Utils.ButtonState state)
        {
            if (value)
                return Utils.ButtonState.Disable;
            else if (state == Utils.ButtonState.Disable)
                return Utils.ButtonState.None;
            return state;
        }

        public void DrawTextArea(Rect rect, bool interactable)
        {
            //Init components
            if (target == null)
                return;
            Styles.Load();
            Event e = Event.current;
            dirty = false;

            //Draw background and set clip area
            Rect rectBox = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.Box(rectBox, "");
            GUI.BeginClip(rectBox);
            if (e.type == EventType.Repaint)
                clipRect = new Rect(rectBox.x, rectBox.y, rectBox.width - 20, rectBox.height);

            //Draw tags rects
            for (int i = 0; i < target.tags.Count; i++)
                DrawRects(target.tags[i].rects, true, target.tags[i].color);

            //Draw selection rects
            DrawRects(rects, false, Styles.selectColor);

            //Draw text
            float contentHeight = Styles.textStyle.CalcHeight(userStringGUIContent, rectBox.width - 30);
            textRect = new Rect(5, 5 - scroll.y, rectBox.width - 26, contentHeight);
            GUI.Label(textRect, userString, Styles.textStyle);
            //EditorGUI.DrawRect(textRect, Color.red * 0.1f);

            //Draw carret
            if (interactable)
                DrawCarret();

            //Handle events

            ManageFocus(e);

            if (focus && interactable)
            {
                if (e.type == EventType.KeyDown)
                    ReceiveKeyEvent(e);
                if (e.isMouse)
                    ReceiveMouseEvent(e);
            }

            //Get scroll by mouse scrollwheel
            if (e.type == EventType.ScrollWheel && interactable)
                scroll.y += e.delta.y * 5;

            //Clamp scroll to content area
            float dif = Mathf.Max(0.0f, contentHeight - rectBox.height + 64);
            scroll.y = Mathf.Clamp(scroll.y, 0.0f, dif);

            //End clip area
            GUI.EndClip();

            //Draw scroll bar
            Rect scrollRect = new Rect(rectBox.x + rectBox.width - 16, rectBox.y + 1, 15, rectBox.height - 2);
            float size = Mathf.Clamp(contentHeight - dif, 0.0f, dif);
            if (dif > 0.0f)
                scroll.y = GUI.VerticalScrollbar(scrollRect, scroll.y, size, 0.0f, dif + size);
            else
                GUI.VerticalScrollbar(scrollRect, 0.0f, 1.0f, 0.0f, 1.0f);

            //Add text cursor area
            if (interactable)
                EditorGUIUtility.AddCursorRect(rectBox.Offset(0, 0, -20, 0), MouseCursor.Text);
        }

        public void DrawCharCountBar(Rect rect)
        {
            EditorGUI.ProgressBar(rect, length / 1000.0f, length + "/1000");
        }

        public void Refresh()
        {
            lines = LinesPack.FromText(userString, textRect, Styles.textStyle);
            RefreshSelectionRects();
            RefreshTagsRects();
        }

        /// <summary> 
        /// Draw a group of rects in text area. Like selection rects or tag rects.
        /// </summary>
        private void DrawRects(Rect[] rects, bool flatStyle, Color color)
        {
            if (rects == null)
                return;

            for (int i = 0; i < rects.Length; i++)
            {
                Rect r = rects[i];
                r.y -= scroll.y;

                if (flatStyle)
                    Utils.FlatRect(r, color, 0.95f, 0.0f);
                else
                    EditorGUI.DrawRect(r, color);
            }
        }

        /// <summary>
        /// Draw a blinking carret at selectID
        /// </summary>
        private void DrawCarret()
        {
            if (!focus)
                return;

            double delta = EditorApplication.timeSinceStartup - lastInputTime;
            if ((delta % 1.0f) > 0.5)
                return;

            Vector2 pos = GetCarretAt(selectID);
            Rect rect = new Rect(pos.x, pos.y, 1, 16);
            EditorGUI.DrawRect(rect, Color.black);
        }
        #endregion

        #region Text utils functions
        /// <summary>
        /// Return pixel position from character index in userString.
        /// </summary>
        private Vector2 GetCarretAt(int index)
        {
            return Styles.textStyle.GetCursorPixelPosition(textRect, userStringGUIContent, index);
        }

        /// <summary>
        /// Return character index from pixel position in userString.
        /// </summary>
        private int GetCarretAt(Vector2 position)
        {
            return Styles.textStyle.GetCursorStringIndex(textRect, userStringGUIContent, position);
        }

        /// <summary>
        /// Generate rects between startID and endID on userString in textRect
        /// </summary>
        private Rect[] GetRects(int startID, int endID, Vector2 offset)
        {
            //Reorder select IDs
            int min = Mathf.Min(startID, endID);
            int max = Mathf.Max(startID, endID);

            //Rebuild lines if needed
            if (lines == null)
                lines = LinesPack.FromText(userString, textRect, Styles.textStyle);

            //Return rects generated between start - end
            return lines.GetRects(min, max, Styles.textStyle, offset + new Vector2(-1, 2));
        }

        /// <summary>
        /// Give the character count between the index and the start/end of the word
        /// </summary>
        private int WordLenght(int index, bool back)
        {
            if (!back)
            {
                int result = userString.IndexOfAny(new char[] { ' ', '\n' }, index + 1);
                return result == -1 ? index : (result + 1) - index;
            }
            else
            {
                int result = userString.LastIndexOfAny(new char[] { ' ', '\n' }, index - 2);
                return result == -1 ? index : index - (result + 1);
            }
        }

        /// <summary>
        /// Return true if all ids are in the userString
        /// </summary>
        private bool IsInString(params int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] < 0)
                    return false;
                if (ids[i] > length)
                    return false;
            }
            return true;
        }

        /// <summary> Return the bottom left corner in screenPos of a tag. </summary>
        private Vector2 GetTagPos(Tag tag)
        {
            Vector2 min = tag.rects[0].min;
            for (int i = 1; i < tag.rects.Length; i++)
            {
                if (min.y > tag.rects[i].min.y)
                    min = tag.rects[i].min;
            }

            min += clipRect.min + new Vector2(2, -scroll.y + Styles.textStyle.lineHeight);
            return GUIUtility.GUIToScreenPoint(min);
        }

        #endregion

        #region User Events

        private void ReceiveKeyEvent(Event e)
        {
            if (e.functionKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Backspace:
                        {
                            if (haveSelection)
                            {
                                ClearSelection();
                                break;
                            }

                            int offset = e.control ? WordLenght(carretID, true) : 1;
                            if (IsInString(carretID - offset, carretID))
                            {
                                OnEditText(carretID, offset, false);
                                userString = userString.Remove(carretID - offset, offset);
                                carretID -= offset;
                                selectID -= offset;
                            }
                            break;
                        }
                    case KeyCode.Delete:
                        {
                            if (haveSelection)
                            {
                                ClearSelection();
                                break;
                            }

                            if (carretID == length)
                                break;
                            int offset = e.control ? WordLenght(carretID, false) : 1;
                            OnEditText(carretID + offset, offset, false);
                            offset = Mathf.Min(length - carretID, offset);
                            userString = userString.Remove(carretID, offset);
                        }
                        break;
                    case KeyCode.LeftArrow:
                    case KeyCode.RightArrow:
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                        ArrowMove(e);
                        break;
                    default:
                        Debug.Log(e.keyCode);
                        break;
                }
            }
            else
            {
                //Insert character directly in userString
                void InsertChar(char c)
                {
                    ClearSelection();
                    OnEditText(carretID, 1, true);
                    userString = userString.Insert(carretID, c.ToString());
                    carretID++;
                    selectID++;
                }

                if (!char.IsControl(e.character))
                {
                    InsertChar(e.character);
                }
                else
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            ClearSelection();
                            InsertChar('\n');
                            break;


                        case KeyCode.A:
                            if (e.control)
                            {
                                carretID = length;
                                selectID = 0;
                            }
                            break;
                        case KeyCode.X:
                            if (e.control)
                            {
                                int min = Mathf.Min(carretID, selectID);
                                int max = Mathf.Max(carretID, selectID);
                                OnEditText(min, max - min, false);
                                GUIUtility.systemCopyBuffer = userString.Substring(min, max - min);
                                ClearSelection();
                            }
                            break;
                        case KeyCode.C:
                            if (e.control)
                            {
                                int min = Mathf.Min(carretID, selectID);
                                int max = Mathf.Max(carretID, selectID);
                                GUIUtility.systemCopyBuffer = userString.Substring(min, max - min);
                            }
                            break;
                        case KeyCode.V:
                            if (e.control)
                            {
                                ClearSelection();
                                string pastString = GUIUtility.systemCopyBuffer.
                                    Replace("\r", "").
                                    Replace(System.Environment.NewLine, "");
                                OnEditText(carretID, pastString.Length, true);
                                userString = userString.Insert(carretID, pastString);
                                selectID = carretID = carretID + pastString.Length;
                            }
                            break;
                    }
                }
            }

            e.Use();
            RefreshTagsRects();
            lastInputTime = EditorApplication.timeSinceStartup;
            dirty = true;
        }

        private void ReceiveMouseEvent(Event e)
        {
            if (!drag)
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    double time = EditorApplication.timeSinceStartup;
                    //Double clic
                    if ((time - lastClicTime) < 0.25f && Vector2.Distance(lastClicPos, e.mousePosition) < 5.0f)
                    {
                        SelectWordAt(e.mousePosition);
                    }

                    //Simple clic
                    else
                    {
                        int id = GetCarretAt(e.mousePosition);
                        carretID = id;
                        if (!e.shift)
                        {
                            _selectID = id;
                            rects = null;
                        }
                        else
                        {
                            RefreshSelectionRects();
                        }
                    }

                    drag = true;
                    lastClicTime = time;
                    lastClicPos = e.mousePosition;
                    e.Use();
                }
            }
            else
            {
                if (e.type == EventType.MouseDrag)
                {
                    selectID = GetCarretAt(e.mousePosition);
                    e.Use();
                }
                if (e.type == EventType.MouseUp && e.button == 0)
                {
                    drag = false;
                    e.Use();
                }
            }

            if (drag)
                lastInputTime = EditorApplication.timeSinceStartup;
        }

        private Tag ApplyTag(Emotion emotion)
        {
            //No selection
            if (!haveSelection)
                return null;

            //Remove tags in this area
            int start = Mathf.Min(selectID, carretID);
            int end = Mathf.Max(selectID, carretID);
            int count = target.tags.Count;
            for (int i = 0; i < count; i++)
            {
                Tag otherTag = null;
                if (target.tags[i].ClearCharacters(start, end - start, out otherTag))
                {
                    target.tags.RemoveAt(i);
                    i--;
                    count--;
                    if (otherTag != null)
                        target.tags.Add(otherTag);
                }
            }

            //Just remove old tags and stop process herefor neutral emotion
            if (emotion == Emotion.Neutral)
            {
                RefreshTagsRects();
                return null;
            }

            //Add the new tag
            Tag tag = new Tag(Tag.Type.Emotion, emotion, selectID, carretID);
            target.tags.Add(tag);
            RefreshTagsRects();
            dirty = true;
            return tag;
        }

        private void AddBreak()
        {
            userString = userString.Insert(carretID, "      ");
            OnEditText(carretID, 6, true);
            target.tags.Add(new Tag(Tag.Type.Wait, Emotion.Neutral, carretID, carretID + 6));
            RefreshTagsRects();
            carretID++;
            selectID++;
            dirty = true;
        }

        private void ClearTags()
        {
            target.tags.Clear();
            RefreshTagsRects();
            dirty = true;
        }

        private void ManageFocus(Event e)
        {
            if (e.clickCount != clickCount)
            {
                clickCount = e.clickCount;
                focus = clipRect.Shrink(-10).Contains(e.mousePosition) || drag;
            }
        }

        private void ClearSelection()
        {
            if (!haveSelection)
                return;

            int min = Mathf.Min(carretID, selectID);
            int max = Mathf.Max(carretID, selectID);

            OnEditText(min, max - min, false);
            userString = userString.Remove(min, max - min);
            selectID = carretID = min;
            rects = null;
        }

        private void ArrowMove(Event e)
        {
            bool shift = e.shift;

            if (haveSelection && !shift)
            {
                int min = Mathf.Min(carretID, selectID);
                int max = Mathf.Max(carretID, selectID);

                switch (e.keyCode)
                {
                    case KeyCode.LeftArrow:
                        _selectID = carretID = min;
                        rects = null;
                        return;
                    case KeyCode.RightArrow:
                        _selectID = carretID = max;
                        rects = null;
                        return;
                    case KeyCode.UpArrow:
                        carretID = selectID;
                        shift = false;
                        break;
                    case KeyCode.DownArrow:
                        carretID = selectID;
                        shift = false;
                        break;
                }
            }


            switch (e.keyCode)
            {
                case KeyCode.LeftArrow:
                    if (carretID > 0)
                    {
                        if (e.control)
                            carretID = Mathf.Max(carretID - WordLenght(carretID, true), 0);
                        else
                            carretID--;
                    }
                    break;
                case KeyCode.RightArrow:
                    if (carretID < userString.Length)
                    {
                        if (e.control)
                            carretID = Mathf.Min(carretID + WordLenght(carretID, false), length);
                        else
                            carretID++;
                    }
                    break;
                case KeyCode.UpArrow:
                    {
                        Vector2 pos = GetCarretAt(carretID);
                        pos.y -= Styles.textStyle.lineHeight;
                        carretID = GetCarretAt(pos);
                    }
                    break;
                case KeyCode.DownArrow:
                    {
                        Vector2 pos = GetCarretAt(carretID);
                        pos.y += Styles.textStyle.lineHeight;
                        carretID = GetCarretAt(pos);
                    }
                    break;
            }

            if (!shift)
                selectID = carretID;
            else
            {
                RefreshSelectionRects();
            }
        }

        private void SelectWordAt(Vector2 position)
        {
            int id = GetCarretAt(position);
            int min = WordLenght(id + 1, true) - 1;
            int max = WordLenght(id - 1, false) - 2;

            carretID = id - min;
            selectID = Mathf.Min(id + max, length);
        }

        private void OnEditText(int id, int length, bool add)
        {
            if (add)
            {
                for (int i = 0; i < target.tags.Count; i++)
                {
                    target.tags[i].AddCharacters(id, length);
                }
            }
            else
            {
                int count = target.tags.Count;
                for (int i = 0; i < count; i++)
                {
                    if (target.tags[i].RemoveCharacters(id, length))
                    {
                        target.tags.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }


        #endregion

        private void RefreshSelectionRects()
        {
            rects = GetRects(carretID, selectID, textRect.min + scroll);
        }

        private void RefreshTagsRects()
        {
            for (int i = 0; i < target.tags.Count; i++)
                target.tags[i].rects = GetRects(target.tags[i].start, target.tags[i].end, textRect.min + scroll);
        }

        class LinesPack
        {
            string[] lines;
            int[] length;
            int[] startID;

            public LinesPack(string[] lines)
            {
                this.lines = lines;
                startID = new int[lines.Length];
                length = new int[lines.Length];
                int total = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    startID[i] = total;
                    length[i] = lines[i].Length;
                    total += length[i];
                }
            }

            public Rect[] GetRects(int start, int end, GUIStyle style, Vector2 offset)
            {
                int firstLine = lines.Length - 1;
                int lastLine = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (start >= startID[i])
                        firstLine = i;
                    if (end >= startID[i])
                        lastLine = i;
                }

                Rect[] rects = new Rect[lastLine - firstLine + 1];
                float lineHeight = style.lineHeight;
                for (int i = firstLine; i <= lastLine; i++)
                {
                    float x;
                    float width;

                    if (i == firstLine && start - startID[i] != 0)
                    {
                        x = style.CalcSize(new GUIContent(lines[i].Remove(start - startID[i]))).x;
                    }
                    else
                    {
                        x = 2;
                    }

                    if (i == lastLine && end - startID[i] != length[i])
                    {
                        width = style.CalcSize(new GUIContent(lines[i].Remove(end - startID[i]))).x;
                    }
                    else
                    {
                        width = style.CalcSize(new GUIContent(lines[i])).x;
                    }
                    rects[i - firstLine] = new Rect(x + offset.x, i * lineHeight + offset.y, width - x, lineHeight);
                }
                return rects;
            }

            public static LinesPack FromText(string text, Rect rect, GUIStyle style)
            {
                return new LinesPack(LinesFromString(text, rect, style));
            }

            private static string[] LinesFromString(string value, Rect rect, GUIStyle style)
            {
                //Split by line return
                string[] splitString = value.Split('\n');
                for (int i = 0; i < splitString.Length; i++)
                    splitString[i] = splitString[i] + "\n";

                //Insert split by word warp
                Rect virtualRect = new Rect(0, 0, rect.width, 100);
                List<string> linesList = new List<string>();
                for (int i = 0; i < splitString.Length; i++)
                {
                    string sub = splitString[i];
                    int cutID = style.GetCursorStringIndex(virtualRect, new GUIContent(sub), new Vector2(virtualRect.width, 8)) + 1;
                    int a = 0;
                    while (cutID != sub.Length && a < 20)
                    {
                        a++;
                        linesList.Add(sub.Substring(0, cutID));
                        sub = sub.Remove(0, cutID);
                        cutID = style.GetCursorStringIndex(virtualRect, new GUIContent(sub), new Vector2(virtualRect.width, 8)) + 1;
                    }
                    linesList.Add(sub);
                }

                //Return result
                return linesList.ToArray();
            }
        }
    }
}