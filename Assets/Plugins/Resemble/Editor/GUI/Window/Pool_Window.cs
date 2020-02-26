using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    public partial class Resemble_Window
    {
        private static Vector2 scroll;
        private static int requestCount;

        /// <summary> Draw list of all task. </summary>
        public static void DrawPoolGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true);

            int count = Pool.tasks.Count;
            double time = EditorApplication.timeSinceStartup;
            for (int i = 0; i < count; i++)
            {
                Task task = Pool.tasks[i];

                //Remove old completed task
                if (task.status == Task.Status.Completed && time > task.time + 2.0f)
                {
                    Pool.tasks.RemoveAt(i);
                    i--;
                    count--;
                }

                //Draw task
                else
                {
                    DrawTaskGUI(Pool.tasks[i]);
                }
            }
            if (Event.current.type == EventType.Layout)
                requestCount = count;

            //Draw [No pending requests] box info.
            if (requestCount == 0)
            {
                EditorGUILayout.HelpBox("There is no pending requests...", MessageType.Info);
            }

            GUILayout.EndScrollView();
        }

        /// <summary> Call this when the pool change. </summary>
        public static void RefreshPoolList()
        {
            if (window != null && window.tab == Tab.Pool)
                window.Repaint();
        }

        /// <summary> Draw gui status of one task. </summary>
        private static void DrawTaskGUI(Task task)
        {
            Rect rect = GUILayoutUtility.GetRect(Screen.width - 20, 50);
            GUI.Box(rect, "");
            GUI.Label(rect, task.status.ToString());

            if (task.status == Task.Status.Downloading)
            {
                rect.Set(rect.x + 5, rect.y + 40, rect.width - 10, 8);
                EditorGUI.ProgressBar(rect, task.preview.progress, "");
            }

        }

    }
}