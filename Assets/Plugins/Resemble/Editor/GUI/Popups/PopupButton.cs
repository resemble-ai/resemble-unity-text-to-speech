using UnityEngine;

namespace Resemble.GUIEditor
{
    /// <summary> Useful class to make a button that will display a popup window. </summary>
    public class PopupButton
    {
        public GUIContent content;
        public delegate void OnValidate(Rect rect);
        public Vector2 size;
        private Rect rect;

        public PopupButton(string label, string tooltip, Vector2 size)
        {
            this.size = size;
            content = new GUIContent(label, tooltip);
        }

        public void DoLayout(OnValidate onValidate)
        {
            if (GUILayout.Button(content))
                onValidate.Invoke(rect);
            if (Event.current.type == EventType.Repaint)
                SetRect(GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect()));
        }

        public void Draw(Rect rect, OnValidate onClic)
        {
            SetRect(rect);
            if (GUI.Button(rect, content))
                onClic.Invoke(GUIUtility.ScreenToGUIRect(rect));
        }

        private void SetRect(Rect rect)
        {
            this.rect = rect.Offset(Mathf.Min(rect.x, rect.x + rect.width - size.x) , rect.height - 2, size.x, size.y);
        }
    }
}