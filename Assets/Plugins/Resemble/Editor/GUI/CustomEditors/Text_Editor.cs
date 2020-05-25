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
        public Text text;
        public bool dirty;

        //Text edition stuff
        private string userString
        {
            get
            {
                return text.userString;
            }
            set
            {
                text.userString = value;
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
        private int carretID
        {
            get
            {
                return text.carretID;
            }
            set
            {
                text.carretID = value;
            }
        }
        private int selectID
        {
            get
            {
                return text.selectID;
            }
            set
            {
                text.selectID = value;
            }
        }
        private bool focus;

        //GUI stuff
        private bool fromOneShootWindow;
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
        Utils.ButtonState editBtn;
        Utils.ButtonState removeBtn;

        //Repaint stuff
        public delegate void Callback();
        private bool needTagRefresh;
        private bool containsTag;
        private Tag tagToEdit;
        Callback requestRepaint;

        //Constructor
        public Text_Editor(Text text, Callback editCallback, Callback repaintCallback)
        {
            this.text = text;
            text.onEdit += () => { needTagRefresh = true; repaintCallback.Invoke(); };
            text.onChangeSelect += () => { CheckContains(); RefreshLines(); RefreshSelectionRects(); repaintCallback.Invoke(); };
            requestRepaint += repaintCallback;
        }

        #region Draw functions

        private void RefreshLines()
        {
            lines = LinesPack.FromText(userString, textRect, Styles.textStyle);
        }

        private void CheckContains()
        {
            if (text.haveSelection)
                containsTag = text.GetTags().Count > 0;
            else
                containsTag = text.GetTag() != null;
        }

        public void Refresh()
        {
            //Postpone the next refresh to the next event.
            if (Event.current == null)
            {
                needTagRefresh = true;
                SceneView.RepaintAll();
            }

            //Refresh now
            else
            {
                RefreshTagsRects();
                RefreshSelectionRects();
                requestRepaint.Invoke();
            }
        }

        public void DoLayout(bool enable, bool fromOneShootWindow)
        {
            this.fromOneShootWindow = fromOneShootWindow;
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 300).Shrink(10);
            DrawTextArea(rect, Settings.haveProject);
        }

        public void DrawTagsBtnsLayout(bool disabled)
        {
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 50).Shrink(10);
            Rect btnRect = rect;

            //Update disable state of buttons based on selection
            breakBtn = DisableIf(haveSelection, breakBtn);
            emotionBtn = DisableIf(!haveSelection, emotionBtn);
            editBtn = DisableIf(haveSelection || !containsTag, editBtn);
            removeBtn = DisableIf(!containsTag, removeBtn);

            //Break button
            btnRect.width = 60;
            if (Utils.FlatButton(btnRect, new GUIContent(Resources.instance.breakIco), new Color(0.956f, 0.361f, 0.259f), ref breakBtn))
            {
                tagToEdit = text.AddBreak();
            }

            //Emotion button
            btnRect.Set(btnRect.x + btnRect.width + 5, btnRect.y, 130, btnRect.height);
            if (Utils.FlatButton(btnRect, new GUIContent("Add Emotion"), new Color(0.259f, 0.6f, 0.956f), ref emotionBtn))
            {
                tagToEdit = text.AddTag();
            }

            btnRect.Set(btnRect.x + btnRect.width + 5, btnRect.y, 70, btnRect.height);
            if (Utils.FlatButton(btnRect, new GUIContent("Edit"), new Color(0.259f, 0.6f, 0.956f), ref editBtn))
            {
                tagToEdit = text.GetTag();
            }

            btnRect.Set(btnRect.x + btnRect.width + 5, btnRect.y, 100, btnRect.height);
            if (Utils.FlatButton(btnRect, new GUIContent("Remove"), new Color(1.0f, 0.6f, 0.1f), ref removeBtn))
            {
                if (text.haveSelection)
                    text.RemoveTag();
                else
                    text.RemoveTag(text.GetTag());
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
            if (text == null)
                return;
            Styles.Load();
            Event e = Event.current;

            //Refresh stuff
            if (e.type == EventType.Repaint)
            {
                if (needTagRefresh || tagToEdit != null)
                {
                    textRect = new Rect(5, 5 - scroll.y, rect.width - 26, rect.height);
                    needTagRefresh = false;
                    RefreshLines();
                    RefreshTagsRects();
                    requestRepaint.Invoke();
                }
                if (tagToEdit != null)
                {
                    ShowEditPopupOntag(tagToEdit);
                    tagToEdit = null;
                }
            }

            //Draw background and set clip area
            Rect rectBox = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.Box(rectBox, "", GUI.skin.textField);
            GUI.BeginClip(rectBox);
            if (e.type == EventType.Repaint)
                clipRect = new Rect(rectBox.x, rectBox.y, rectBox.width - 20, rectBox.height);

            //Draw tags rects
            for (int i = 0; i < text.tags.Count; i++)
                DrawRects(text.tags[i].rects, true, text.tags[i].color);

            //Draw selection rects
            DrawRects(rects, false, Styles.selectColor);

            //Draw text
            float contentHeight = Styles.textStyle.CalcHeight(userStringGUIContent, rectBox.width - 30);
            textRect.height = contentHeight;
            GUI.Label(textRect, userString, Styles.textStyle);

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
            {
                scroll.y += e.delta.y * 5;
                needTagRefresh = true;
                Refresh();
            }

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
                    Utils.FlatRect(r, color.Alpha(0.7f), 0.2f, 0.0f);
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
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? Color.white : Color.black);
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
                        //Have selection
                        if (text.haveSelection)
                        {
                            text.Delete();
                            break;
                        }

                        //Can't remove more characters
                        if (carretID == 0)
                            break;

                        //Classic
                        if (!e.control)
                        {
                            text.RemoveChars(-1);
                            break;
                        }

                        //Control modificator
                        text.RemoveChars(-WordLenght(carretID, true));
                        break;

                    case KeyCode.Delete:
                        //Have selection
                        if (text.haveSelection)
                        {
                            text.Delete();
                            break;
                        }

                        //Can't remove more characters
                        if (carretID == text.userString.Length)
                            break;

                        //Classic
                        if (!e.control)
                        {
                            text.RemoveChars(1);
                            break;
                        }

                        //Control modificator
                        text.RemoveChars(WordLenght(carretID, false));
                        break;

                    case KeyCode.LeftArrow:
                    case KeyCode.RightArrow:
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                        ArrowMove(e);
                        break;
                    default:
                        //Debug.Log(e.keyCode);
                        break;
                }
            }
            else
            {
                //Type simple character
                if (!char.IsControl(e.character))
                    text.TypeChar(e.character);

                //Control + character
                else
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            text.Delete();
                            text.TypeChar('\n');
                            break;
                        case KeyCode.A:
                            if (e.control)
                                text.SelectAll();
                            break;
                        case KeyCode.X:
                            if (e.control)
                                text.Cut();
                            break;
                        case KeyCode.C:
                            if (e.control)
                                text.Copy();
                            break;
                        case KeyCode.V:
                            if (e.control)
                                text.Paste();
                            break;
                    }
                }
            }

            e.Use();
            lastInputTime = EditorApplication.timeSinceStartup;
        }

        private void ReceiveMouseEvent(Event e)
        {
            //Remove drag flag after lose it in out of screen
            if (drag && e.type == EventType.MouseDown && e.button == 0)
                drag = false;

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
                            selectID = id;
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

            //Right clic - Open a context menu
            if ((e.type == EventType.MouseDown && e.button == 1) || e.type == EventType.ContextClick)
            {
                //Set carret on clic if there is no selection or the clic is not on the selection.
                int mouseID = GetCarretAt(e.mousePosition);
                if (!text.haveSelection || !text.SelectionContains(mouseID))
                    carretID = selectID = mouseID;

                //Get flags
                bool haveClipboard = !string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
                bool onTag = !text.haveSelection && text.GetTag() != null;

                //If shift and on tag - Open directly the tag editor
                if (e.shift && onTag)
                {
                    tagToEdit = text.GetTag();
                }
                //Build context menu
                else
                {
                    GenericMenu menu = new GenericMenu();
                    if (haveSelection)
                    {
                        menu.AddItem(new GUIContent("Copy"), false, () => { text.Copy(); });
                        menu.AddItem(new GUIContent("Cut"), false, () => { text.Cut(); });
                        if (haveClipboard)
                            menu.AddItem(new GUIContent("Paste"), false, () => { text.Paste(); });
                        else
                            menu.AddDisabledItem(new GUIContent("Paste"), false);
                        menu.AddSeparator("");
                        menu.AddDisabledItem(new GUIContent("Add break"), false);
                        menu.AddItem(new GUIContent("Add Emotion"), false, () => { tagToEdit = text.AddTag(); });
                        menu.AddSeparator("");
                        menu.AddDisabledItem(new GUIContent("Edit"), false);
                        menu.AddItem(new GUIContent("Remove"), false, () => { text.RemoveTag(); });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Copy"), false);
                        menu.AddDisabledItem(new GUIContent("Cut"), false);
                        if (haveClipboard)
                            menu.AddItem(new GUIContent("Paste"), false, () => { text.Paste(); });
                        else
                            menu.AddDisabledItem(new GUIContent("Paste"), false);
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Add break"), false, () => { tagToEdit = text.AddBreak(); });
                        menu.AddDisabledItem(new GUIContent("Add Emotion"), false);
                        menu.AddSeparator("");
                        if (onTag)
                        {
                            menu.AddItem(new GUIContent("Edit"), false, () => { tagToEdit = text.GetTag(); });
                            menu.AddItem(new GUIContent("Remove"), false, () => { text.RemoveTag(text.GetTag()); });
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Edit Emotion"), false);
                            menu.AddDisabledItem(new GUIContent("Remove Emotion"), false);
                        }
                    }
                    menu.ShowAsContext();
                }
                e.Use();
            }

            if (drag)
                lastInputTime = EditorApplication.timeSinceStartup;
            RefreshSelectionRects();
            CheckContains();
        }

        private void ShowEditPopupOntag(Tag tag)
        {
            TagPopup.Show(GetTagPos(tag), tag, fromOneShootWindow);
        }

        private void ManageFocus(Event e)
        {
            if (e.clickCount != clickCount)
            {
                clickCount = e.clickCount;
                focus = clipRect.Shrink(-10).Contains(e.mousePosition) || drag;
            }
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
                        selectID = carretID = min;
                        rects = null;
                        return;
                    case KeyCode.RightArrow:
                        selectID = carretID = max;
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

                RefreshSelectionRects();
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
                for (int i = 0; i < text.tags.Count; i++)
                {
                    text.tags[i].AddCharacters(id, length);
                }
            }
            else
            {
                int count = text.tags.Count;
                for (int i = 0; i < count; i++)
                {
                    if (text.tags[i].RemoveCharacters(id, length))
                    {
                        text.tags.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }


        #endregion

        #region Refresh stuff
        private void RefreshSelectionRects()
        {
            if (text.haveSelection)
                rects = GetRects(carretID, selectID, textRect.min + scroll);
            else
                rects = new Rect[0];
        }

        private void RefreshTagsRects()
        {
            for (int i = 0; i < text.tags.Count; i++)
                text.tags[i].rects = GetRects(text.tags[i].start, text.tags[i].end, textRect.min + scroll);
        }

        class LinesPack
        {
            public string[] lines;
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
        #endregion
    }
}