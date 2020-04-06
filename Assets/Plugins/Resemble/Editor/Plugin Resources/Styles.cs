using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public static class Styles
    {

        public static bool loaded { get; private set; }
        private static string path;
        public static GUIStyle bodyStyle;
        public static GUIStyle settingsBody;
        public static GUIStyle settingsIndex;
        public static GUIStyle settingsLink;
        public static GUIStyle settingsCode;
        public static GUIStyle settingsButton;
        public static GUIStyle settingsQuote;
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
        public static GUIStyle textStyle;
        public static GUIStyle codeBox;
        public static GUIStyle folderPathField;
        public static GUIStyle folderPathFieldRight;
        public static GUIStyle whiteFrame;
        public static GUIStyle whiteLabel;
        public static GUIStyle phonemesInfos;
        public static GUIStyle spectrumBackground;
        public static GUIContent characterSetHelpBtn;
        public static GUIContent popupBtn;
        public static Color clipGreenColor = new Color(0.1921f, 0.8196f, 0.6352f);
        public static Color clipOrangeColor = new Color(1.0f, 0.6509f, 0.0f);
        public static Color selectColor = new Color(0.0f, 0.611f, 1.0f, 0.5f);
        public static Color lightGreen = new Color(0.196f, 0.8196f, 0.6352f);
        public static Color purple = new Color(0.3921f, 0.2705f, 0.5921f);
        public static Color background;


        public static void Load()
        {
            if (loaded)
                return;

            if (Event.current == null)
                return;

            //Styles
            bodyStyle = new GUIStyle(EditorStyles.label);
            bodyStyle.wordWrap = false;
            bodyStyle.padding = new RectOffset(0, 0, 0, 0);

            settingsBody = new GUIStyle(EditorStyles.label);
            settingsBody.richText = true;
            settingsBody.wordWrap = true;
            settingsBody.fontSize = 12;
            settingsCode = new GUIStyle(settingsBody);
            settingsCode.alignment = TextAnchor.UpperLeft;
            settingsBody.margin = new RectOffset(30, 10, 0, 0);

            settingsLink = new GUIStyle(settingsBody);
            settingsLink.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);

            settingsIndex = new GUIStyle(settingsLink);
            settingsIndex.fontSize = 11;
            settingsIndex.margin = new RectOffset(0, 0, 0, 0);

            settingsButton = new GUIStyle(GUI.skin.button);
            settingsButton.margin = new RectOffset(30, 10, 0, 0);

            settingsQuote = new GUIStyle(GUI.skin.box);
            settingsQuote.wordWrap = true;
            settingsQuote.fontSize = 12;
            settingsCode.alignment = TextAnchor.MiddleLeft;
            settingsQuote.margin = new RectOffset(30, 10, 0, 0);
            if (EditorGUIUtility.isProSkin)
                settingsQuote.normal.textColor = new Color(0.6f, 0.6f, 0.8f, 1.0f);

            linkStyle = new GUIStyle(bodyStyle);
            linkStyle.padding = new RectOffset(-5, 0, 0, 0);
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
            footer.fontSize = 10;
            footer.richText = true;
            footer.alignment = TextAnchor.LowerRight;

            headerField = new GUIStyle(GUI.skin.textField);
            headerField.fontSize = 16;
            headerField.fixedHeight = 22;

            arrayBox = new GUIStyle(GUI.skin.box);
            arrayBox.margin = new RectOffset(0, 0, 0, 0);

            codeBox = new GUIStyle(GUI.skin.box);
            codeBox.margin = new RectOffset(30, 30, 5, 5);

            projectHeaderLabel = new GUIStyle(EditorStyles.whiteLargeLabel);
            projectHeaderLabel.alignment = TextAnchor.MiddleLeft;
            projectHeaderLabel.normal.textColor = Color.white;
            projectHeaderLabel.font = Resources.instance.font;
            projectHeaderLabel.fontStyle = FontStyle.Bold;
            projectHeaderLabel.fontSize = 20;

            textStyle = new GUIStyle(EditorStyles.largeLabel);
            textStyle.wordWrap = true;
            textStyle.fontSize = 14;

            folderPathField = new GUIStyle(GUI.skin.textField);
            folderPathFieldRight = new GUIStyle(folderPathField);
            folderPathFieldRight.alignment = TextAnchor.MiddleRight;

            whiteFrame = new GUIStyle(GUI.skin.textArea);
            whiteFrame.normal = whiteFrame.focused;

            whiteLabel = new GUIStyle(GUI.skin.label);
            whiteLabel.normal.textColor = Color.white;

            phonemesInfos = new GUIStyle(whiteLabel);
            phonemesInfos.alignment = TextAnchor.UpperLeft;
            phonemesInfos.fontSize = 16;

            spectrumBackground = new GUIStyle(GUI.skin.box);


            //GUIContent
            characterSetHelpBtn = EditorGUIUtility.IconContent("_Help");
            characterSetHelpBtn.tooltip = "Open the documentation for Resemble CharacterSet";
            popupBtn = EditorGUIUtility.IconContent("_Popup");


            //Color
            background = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.4f, 0.4f, 1.0f) : Color.white;

            loaded = true;
            return;
        }

    }
}