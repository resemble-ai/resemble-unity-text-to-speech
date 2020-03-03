using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NotificationsPopup : EditorWindow
{
    private static NotificationsPopup window;
    private static List<Notification> notifs = new List<Notification>();
    private static bool registerToUpdates;

    [System.Serializable]
    struct Notification
    {
        string message;
        Object link;

        public Notification(string message, Object link)
        { this.message = message;
            this.link = link;
        }
    }

    public static void Add(string message, Object linkedObject)
    {
        notifs.Add(new Notification(message, linkedObject));

        if (!registerToUpdates)
        {
            EditorApplication.update += CheckDisponibility;
            registerToUpdates = true;
        }
    }

    private static void CheckDisponibility()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EditorApplication.update -= CheckDisponibility;
        registerToUpdates = false;
        Show();
    }

    public new static void Show()
    {
        window = CreateInstance<NotificationsPopup>();
        Resolution res = Screen.currentResolution;
        window.position = new Rect(res.width - 110, res.height - 310, 100, 100);
        window.titleContent = new GUIContent("Notifications");
        window.ShowPopup();
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical(GUI.skin.window);
        if (GUILayout.Button("Close"))
            Hide();
        GUILayout.EndVertical();
    }

    public static void Hide()
    {
        if (window != null)
        {
            window.Close();
            window = null;
        }
    }
}
