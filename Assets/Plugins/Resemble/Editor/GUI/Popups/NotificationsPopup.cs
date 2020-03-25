using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    public class NotificationsPopup : EditorWindow
    {
        //GUI stuff
        private static NotificationsPopup window;
        private static List<Notification> notifs = new List<Notification>();
        private static bool registerToUpdates;
        private static Color highlightColor = new Color(0.2f, 0.6f, 1.0f, 0.3f);

        //Notifications properties
        const int notifHeight = 38;
        const int notifWidth = 320;
        const float notifTime = 5.0f;

        [System.Serializable]
        class Notification
        {
            public string message;
            public Object link;
            public bool isDiplaying;
            public MessageType type;
            public double displayTime;
            public float lastHeight;

            public Notification(string message, MessageType type, Object link)
            {
                this.message = message;
                this.link = link;
                this.type = type;
                displayTime = 0.0;
                isDiplaying = false;
            }

            public float GetHeight(double time)
            {
                float delta = (float)(time - displayTime);
                float t = 1.0f;

                if (delta < 0.5f)
                {
                    t = delta * 2.0f;
                    t *= t;
                    t *= t;
                }
                else if (notifTime - delta < 0.5f)
                {
                    t = (notifTime - delta) * 2.0f;
                    t *= t;
                    t *= t;
                }

                lastHeight = t * notifHeight;
                return lastHeight;
            }
        }

        /// <summary> Pop a notification on screen when the editor is not playing. Clicking on the notification selects the linked object. </summary>
        public static void Add(string message, MessageType type, Object linkedObject)
        {
            if (notifs.Count == 0 && !registerToUpdates)
            {
                EditorApplication.update += CheckDisponibility;
                registerToUpdates = true;
            }
            notifs.Add(new Notification(message, type, linkedObject));
        }

        /// <summary> Check if notifications can be shown. </summary>
        private static void CheckDisponibility()
        {
            if (Application.isPlaying)
                return;

            EditorApplication.update -= CheckDisponibility;
            registerToUpdates = false;
            Show();
        }

        private static Rect GetPosition(float height)
        {
            Resolution res = Screen.currentResolution;
            return new Rect(res.width - notifWidth - 10, res.height - height - 73, notifWidth, height);
        }

        private new static void Show()
        {
            window = CreateInstance<NotificationsPopup>();
            window.titleContent = new GUIContent("Notifications");
            window.minSize = Vector2.zero;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            //Revove old notifications
            double time = EditorApplication.timeSinceStartup;
            int count = notifs.Count;
            for (int i = 0; i < notifs.Count; i++)
            {
                if (!notifs[i].isDiplaying)
                    continue;
                if (time - notifs[i].displayTime > notifTime)
                {
                    notifs.RemoveAt(i);
                    i--;
                    count--;
                }
            }

            //No more notifications - Close window
            if (notifs.Count == 0)
            {
                Close();
                return;
            }

            //Compute heights
            count = Mathf.Min(8, notifs.Count);
            float totalHeight = 0;
            for (int i = 0; i < count; i++)
                totalHeight += notifs[i].GetHeight(time);

            //Place window
            window.position = GetPosition(totalHeight);

            //Display notifications
            Rect rect = new Rect(0, 0, notifWidth, notifHeight);
            Rect btnRect = new Rect(notifWidth - 70, 20, 60, 16);
            totalHeight = 0;
            Vector2 mp = Event.current.mousePosition;

            for (int i = 0; i < count; i++)
            {
                if (!notifs[i].isDiplaying)
                {
                    notifs[i].isDiplaying = true;
                    notifs[i].displayTime = time;
                }

                //Draw notif
                rect.height = notifs[i].lastHeight;
                EditorGUI.HelpBox(rect, notifs[i].message, notifs[i].type);

                //Close button
                if (GUI.Button(btnRect, "Close"))
                {
                    notifs[i].displayTime = time - notifTime;
                }

                //Main button
                else if (rect.Contains(mp))
                {
                    if (btnRect.Contains(mp))
                        EditorGUI.DrawRect(btnRect, highlightColor);
                    else
                        EditorGUI.DrawRect(rect, highlightColor);
                    if (GUI.Button(rect, "", GUIStyle.none))
                    {
                        notifs[i].displayTime = time - notifTime + 0.5f;
                        Selection.activeObject = notifs[i].link;
                        EditorGUIUtility.PingObject(notifs[i].link);
                    }
                }

                //Adapt rect for next notif
                totalHeight += notifs[i].lastHeight;
                rect.y = totalHeight;
                btnRect.y = totalHeight + 20;
            }

            //Keep refreshing
            Repaint();
        }
    }
}
