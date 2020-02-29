using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble;

namespace Resemble.GUIEditor
{
    public class TagPopup : EditorWindow
    {

        private static bool releaseFocusOnclose;
        private static Tag tag;

        private static Utils.ButtonState[] btns;

        public static void Show(Vector2 pos, Tag tag, bool fromResembleWindow)
        {
            TagPopup window = ScriptableObject.CreateInstance<TagPopup>();
            window.position = new Rect(pos.x, pos.y, 150, 125);
            window.ShowPopup();
            releaseFocusOnclose = fromResembleWindow;
            TagPopup.tag = tag;
            btns = new Utils.ButtonState[(int)Emotion.COUNT];
        }

        void OnGUI()
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", GUI.skin.window);

            for (int i = 1; i < btns.Length; i++)
            {
                Emotion em = (Emotion)i;
                if (GUILayout.Button(em.ToString()))
                {
                    tag.emotion = em;
                    tag.color = em.Color();
                    Close();
                }
                /*
                if (Utils.FlatButtonLayout(new GUIContent(em.ToString()), em.Color(), ref btns[i]))
                {
                    tag.emotion = em;
                    tag.color = em.Color();
                    Close();
                }*/
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