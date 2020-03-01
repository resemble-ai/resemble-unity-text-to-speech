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

            switch (tag.type)
            {
                case Tag.Type.Wait:
                    window.position = new Rect(pos.x, pos.y, 250, 88);
                    break;
                case Tag.Type.Emotion:
                    window.position = new Rect(pos.x, pos.y, 150, 132);
                    break;
            }

            window.ShowPopup();
            releaseFocusOnclose = fromResembleWindow;
            TagPopup.tag = tag;
            btns = new Utils.ButtonState[(int)Emotion.COUNT];
        }

        void OnGUI()
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", GUI.skin.window);
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 16).Offset(2, 2, -4, -4);

            switch (tag.type)
            {
                case Tag.Type.Wait:
                    GUI.Label(rect, "Break", EditorStyles.toolbar);
                    GUILayout.Space(8);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Duration :");
                    tag.duration = EditorGUILayout.FloatField(tag.duration);
                    GUILayout.EndHorizontal();

                    float temp = GUILayout.HorizontalSlider(tag.duration, 0.01f, 5.0f);
                    if (temp != tag.duration)
                    {
                        tag.duration = temp;
                        GUI.FocusControl("None");
                    }
                    tag.duration = Mathf.Clamp(tag.duration, 0.01f, 5.0f);

                    GUILayout.Space(16);
                    if (GUILayout.Button("Close"))
                        Close();
                    break;
                case Tag.Type.Emotion:
                    GUI.Label(rect, "Emotion", EditorStyles.toolbar);
                    GUILayout.Space(8);

                    for (int i = 1; i < btns.Length; i++)
                    {
                        Emotion em = (Emotion)i;
                        if (GUILayout.Button(em.ToString()))
                        {
                            tag.emotion = em;
                            tag.color = em.Color();
                            Close();
                        }
                    }
                    break;
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