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
    public static GUIStyle linkStyleSmall;
    public static GUIStyle centredLabel;
    public static GUIStyle bigTitle;
    public static GUIStyle header;
    public static GUIStyle footer;
    public static GUIStyle headerField;
    public static GUIStyle arrayBox;
    public static GUIStyle arrayScrollBar;
    public static GUIStyle projectHeaderLabel;
    public static GUIContent characterSetHelpBtn;
    public static GUIContent popupBtn;
    public static Color podColor = new Color(0.1921f, 0.8196f, 0.6352f);
    public static Color clipColor = new Color(1.0f, 0.6509f, 0.0f);

    public static void Load()
    {
        if (loaded)
            return;

        if (Event.current == null)
            return;

        //Styles
        bodyStyle = new GUIStyle(EditorStyles.label);
        bodyStyle.wordWrap = false;

        linkStyle = new GUIStyle(bodyStyle);
        linkStyle.padding = new RectOffset(-5, 0, 2, 0);
        linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        linkStyleSmall = new GUIStyle(linkStyle);
        linkStyleSmall.fontSize = 9;

        centredLabel = new GUIStyle(EditorStyles.label);
        centredLabel.alignment = TextAnchor.MiddleCenter;
        centredLabel.padding = new RectOffset(12, 12, 0, 2);
        centredLabel.fontSize = 14;

        bigTitle = new GUIStyle("In BigTitle");

        header = new GUIStyle(EditorStyles.largeLabel);
        header.fontSize = 16;

        footer = new GUIStyle(EditorStyles.largeLabel);
        footer.fontSize = 12;
        footer.richText = true;

        headerField = new GUIStyle(GUI.skin.textField);
        headerField.fontSize = 16;
        headerField.fixedHeight = 22;

        arrayBox = new GUIStyle(GUI.skin.box);
        arrayBox.margin = new RectOffset(0, 0, 0, 0);

        projectHeaderLabel = new GUIStyle(EditorStyles.whiteLargeLabel);
        projectHeaderLabel.alignment = TextAnchor.MiddleLeft;
        projectHeaderLabel.normal.textColor = Color.white;
        projectHeaderLabel.font = Resemble_Resources.instance.font;
        projectHeaderLabel.fontStyle = FontStyle.Bold;
        projectHeaderLabel.fontSize = 20;


        //GUIContent
        characterSetHelpBtn = EditorGUIUtility.IconContent("_Help");
        characterSetHelpBtn.tooltip = "Open the documentation for Resemble CharacterSet";
        popupBtn = EditorGUIUtility.IconContent("_Popup");

        loaded = true;
        return;
    }

}
