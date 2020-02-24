using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public class TagPopup : EditorWindow
    {

        private static bool releaseFocusOnclose;
        private static Tag tag;

        private static GUIUtils.ButtonState[] btns;

        public static void Show(Vector2 pos, Tag tag, bool fromResembleWindow)
        {
            TagPopup window = ScriptableObject.CreateInstance<TagPopup>();
            window.position = new Rect(pos.x, pos.y, 150, 250);
            window.ShowPopup();
            releaseFocusOnclose = fromResembleWindow;
            TagPopup.tag = tag;
            btns = new GUIUtils.ButtonState[(int)Emotion.COUNT];
        }

        void OnGUI()
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

            for (int i = 0; i < btns.Length; i++)
            {
                Emotion em = (Emotion)i;
                if (GUIUtils.FlatButtonLayout(new GUIContent(em.ToString()), em.Color(), ref btns[i]))
                {
                    tag.emotion = em;
                    tag.color = em.Color();
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Ok"))
            {
                Close();
            }

            Repaint();
        }

        public void OnLostFocus()
        {
            Close();
        }

        public new void Close()
        {
            base.Close();
            if (releaseFocusOnclose)
                Resemble_Window.Open();
        }

    }
}