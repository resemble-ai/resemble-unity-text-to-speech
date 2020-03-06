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
        private static Color orange = new Color(1.0f, 0.6f, 0.2f, 1.0f);

        /// <summary> Draw list of all task. </summary>
        public static void DrawPoolGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true);

            int count = Pool.tasks.Count;
            double time = EditorApplication.timeSinceStartup;
            for (int i = 0; i < count; i++)
            {
                Task task = Pool.tasks[i];

                //Remove null or old completed task
                if (task == null || task.status == Task.Status.Completed) // && time > task.time + 2.0f
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
            //Init vars
            bool error = task.error;
            bool linked = task.link != null;
            Rect rect = GUILayoutUtility.GetRect(Screen.width - 20, 40);
            

            //Draw box
            if (error)
            {
                EditorGUI.HelpBox(rect, error.ToString(), MessageType.Error);
            }
            else
            {
                EditorGUI.HelpBox(rect, "", MessageType.None);
            }


            //Select button
            if (linked)
            {
                Rect linkRect = new Rect(rect.x + rect.width - 70, rect.y + rect.height - 20, 68, 18);
                if (GUI.Button(linkRect, "Select"))
                {
                    Selection.activeObject = task.link;
                    EditorGUIUtility.PingObject(task.link);
                }
            }


            if (error)
                return;

            //Label rect
            Rect labelRect = rect.Shrink(4); labelRect.height -= 10;
            string label = (linked ? task.link.name : "Unknown") + " : ";

            //Draw bar
            Rect barRect = new Rect(rect.x + 5, rect.y + rect.height - 8, rect.width - 80, 4);
            EditorGUI.ProgressBar(barRect, 0, "");
            switch (task.status)
            {
                case Task.Status.InQueue:
                    GUI.Label(labelRect, label + "In queue...");
                    EditorGUI.DrawRect(barRect.Shrink(1), Color.grey);
                    break;
                case Task.Status.Processing:
                    GUI.Label(labelRect, label + "Processing...");
                    EditorGUI.DrawRect(barRect.Shrink(1), orange);
                    break;
                case Task.Status.Downloading:
                    GUI.Label(labelRect, label + "Downloading...");
                    EditorGUI.ProgressBar(barRect, task.preview.progress, "");
                    break;
                case Task.Status.Completed:
                    GUI.Label(labelRect, label + "Completed");
                    EditorGUI.DrawRect(barRect.Shrink(1), Color.green);
                    break;
            }
        }

    }
}