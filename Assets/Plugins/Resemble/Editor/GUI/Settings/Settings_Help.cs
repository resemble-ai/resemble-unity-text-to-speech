using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public partial class RessembleSettingsProvider
    {

        private void DrawHelpSettingsGUI()
        {
            //Table of contents
            DrawLink("0. Begin with Resemble", 110);
            DrawLink("1. Setup the project", 210);
            DrawLink("2. Create a CharacterSet", 470);
            DrawLink("3. Use a CharacterSet in your scripts", 1000);
            DrawLink("4. Generate one shot clips", 1000);
            DrawLink("5. Use the plugin functions in your code", 1000);
            GUIUtils.DrawSeparator();
            GUILayout.Space(10);

            //Begin with Resemble
            GUILayout.Label("0. Begin with Resemble", Styles.header);
            GUILayout.Label("To use the plugin you first need a Resemble account. If you don't have one you can create it here : ", Styles.settingsBody);
            DrawLink("Sign up", WebPage.ResembleSignUp);
            GUILayout.Space(25);

            //Setup the project
            GUILayout.Label("1. Setup the project", Styles.header);
            GUILayout.Label("Your unity project must be connected to a Resemble project. For this you can go to the Project page.", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[0], Styles.settingsBody);
            GUILayout.Label("You have to enter your Resemble token, you can find it here: ", Styles.settingsBody);
            DrawLink("Resemble token", WebPage.ResembleToken);
            GUILayout.Label("Then, you can bind a project from the list.", Styles.settingsBody);
            GUILayout.Space(25);

            //Create a CharacterSet
            GUILayout.Label("2. Create a CharacterSet", Styles.header);
            GUILayout.Label("To create a new CharacterSet, you can select \n<Color=grey>Assets > Create > Resemble CharacterSet</Color>\nor right-click " +
                "on a folder in your Project window and select\n<Color=grey>Create > Resemble CharacterSet</Color>", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[1], Styles.settingsBody);
            GUILayout.Space(25);

            GUILayout.Space(500);
        }

        private void DrawHelpFooterGUI()
        {
            if (GUILayout.Button("<color=grey>Need more help ? Go to plugin documentation page.  </color>", Styles.footer))
                WebPage.PluginDoc.Open();
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        private void DrawLink(string label, WebPage page)
        {
            if (GUILayout.Button(label, Styles.settingsLink, GUILayout.ExpandWidth(false)))
                page.Open();
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        private void DrawLink(string label, float scrollValue)
        {
            if (GUILayout.Button(label, Styles.settingsIndex, GUILayout.ExpandWidth(false)))
                scroll[2].y = scrollValue;
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

    }
}
