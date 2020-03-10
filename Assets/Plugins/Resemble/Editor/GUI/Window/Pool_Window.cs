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
        private static List<AsyncRequest> requests
        {
            get
            {
                return Resources.instance.requests;
            }
        }

        /// <summary> Draw list of all request. </summary>
        public static void DrawPoolGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll, false, true);

            int count = requests.Count;
            double time = EditorApplication.timeSinceStartup;

            //Draw requests
            for (int i = 0; i < count; i++)
            {
                DrawRequestGUI(requests[i]);
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

        /// <summary> Called when the window is show. </summary>
        private static void EnablePool()
        {
            RefreshPoolList();
        }

        /// <summary> Call this when the pool change. </summary>
        public static void RefreshPoolList()
        {
            if (window != null && window.tab == Tab.Pool)
                window.Repaint();
        }

        /// <summary> Draw gui status of one request. </summary>
        private static void DrawRequestGUI(AsyncRequest request)
        {
            //Init vars
            bool error = request.error;
            bool linked = request.notificationLink != null;
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
                    Selection.activeObject = request.notificationLink;
                    EditorGUIUtility.PingObject(request.notificationLink);
                }
            }


            if (error)
                return;

            //Label rect
            Rect labelRect = rect.Shrink(4); labelRect.height -= 10;

            //Draw bar
            Rect barRect = new Rect(rect.x + 5, rect.y + rect.height - 8, rect.width - 80, 4);
            EditorGUI.ProgressBar(barRect, 0, "");
            switch (request.status)
            {
                default:
                    GUI.Label(labelRect, request.requestName + " - Processing...");
                    EditorGUI.DrawRect(barRect.Shrink(1), orange);
                    break;
                case AsyncRequest.Status.Downloading:
                    GUI.Label(labelRect, request.requestName + " - Downloading...");
                    EditorGUI.ProgressBar(barRect, request.downloadProgress, "");
                    break;
                case AsyncRequest.Status.Completed:
                    GUI.Label(labelRect, request.requestName + " - Completed");
                    EditorGUI.DrawRect(barRect.Shrink(1), Color.green);
                    break;
            }
        }
    }
}