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
                SetRect(GUILayoutUtility.GetLastRect());
        }

        private void SetRect(Rect rect)
        {
            this.rect = GUIUtility.GUIToScreenRect(rect).Offset(-size.x + rect.width, 18, 0, 0);
            this.rect.size = size;
        }
    }
}