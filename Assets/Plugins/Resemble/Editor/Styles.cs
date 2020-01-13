using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Styles
{

    public static bool loaded { get;  private set; }
    private static string path;
    public static GUIStyle bodyStyle;
    public static GUIStyle linkStyle;
    public static Texture button;

    public static void Load()
    {
        if (loaded)
            return;

        if (Event.current == null)
            return;

        bodyStyle = new GUIStyle(EditorStyles.label);
        bodyStyle.wordWrap = false;

        linkStyle = new GUIStyle(bodyStyle);
        linkStyle.padding = new RectOffset(-5, 0, 2, 0);
        linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);

        loaded = true;
        return;
    }

}
