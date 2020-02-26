using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    /// <summary> Help tab of preference window. </summary>
    public partial class RessembleSettingsProvider
    {

        private float[] titleHeights = new float[6];
        private int titleID = 0;

        private void DrawHelpSettingsGUI()
        {
            //Table of contents
            DrawLink("0. Begin with Resemble", titleHeights[0]);
            DrawLink("1. Setup the project", titleHeights[1]);
            DrawLink("2. Create a CharacterSet", titleHeights[2]);
            DrawLink("3. Use a CharacterSet in your scripts", titleHeights[3]);
            DrawLink("4. Generate one shot clips", titleHeights[4]);
            DrawLink("5. Use the plugin functions in your code", titleHeights[5]);
            titleID = 0;
            Utils.DrawSeparator();
            GUILayout.Space(10);

            //Begin with Resemble
            GUILayout.Label("0. Begin with Resemble", Styles.header);
            SetTitleHeight();
            GUILayout.Label("To use the plugin you first need a Resemble account. If you don't have one you can create it here : ", Styles.settingsBody);
            DrawLink("Sign up", WebPage.ResembleSignUp);
            GUILayout.Space(25);

            //Setup the project
            GUILayout.Label("1. Setup the project", Styles.header);
            SetTitleHeight();
            GUILayout.Label("Your unity project must be connected to a Resemble project. For this you can go to the Project page.", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[0], Styles.settingsBody);
            GUILayout.Label("You have to enter your Resemble token, you can find it here: ", Styles.settingsBody);
            DrawLink("Resemble token", WebPage.ResembleToken);
            GUILayout.Label("Then, you can bind a project from the list.", Styles.settingsBody);
            GUILayout.Space(25);

            //Create a CharacterSet
            GUILayout.Label("2. Create a CharacterSet", Styles.header);
            SetTitleHeight();
            GUILayout.Label("To create a new CharacterSet, you can select \n<Color=grey>Assets > Create > Resemble CharacterSet</Color>\nor right-click " +
                "on a folder in your Project window and select\n<Color=grey>Create > Resemble CharacterSet</Color>", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[1], Styles.settingsBody);
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);

            //Use a CharacterSet in your scripts
            GUILayout.Label("3. Use a CharacterSet in your scripts", Styles.header);
            SetTitleHeight();
            GUILayout.Label("You can use CharacterSet in your script. This can be handy to quickly get all the clips on the same voice.", Styles.settingsBody);
            Color c = GUI.color;
            GUI.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            GUILayout.BeginVertical(Styles.codeBox);
            EditorGUILayout.SelectableLabel(
                string.Format(
                "{0}" +
                "{1}using{3} System.Collections;\n" +
                "{1}using{3} System.Collections.Generic;\n" +
                "{1}using{3} UnityEngine;\n" +
                "{1}using{3} Resemble;\n" +
                "\n" +
                "{1}public class {3}{2}Orator{3} : {2}MonoBehaviour{3}\n{4}" +
                "\n\n" +
                "{1}{6}public{3} {2}CharacterSet{3} myCharacterSet;\n" +
                "\n" +
                "{1}{6}void{3} Start()\n{6}{4}\n\n{6}{5}\n\n{1}{6}void{3} Update()\n{6}{4}\n\n{6}{5}" +
                "\n\n{5}{3}", 
                "<Color=#ffffff>", "<Color=#6084f0>", "<Color=#57bda7>", "</Color>", "{", "}", "      "), Styles.settingsCode, GUILayout.Height(300));
            GUILayout.EndVertical();
            GUI.color = c;
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);

            //Generate one shot clips
            GUILayout.Label("4. Generate one shot clips", Styles.header);
            SetTitleHeight();
            GUILayout.Label("You can also use Resemble to quickly create AudioClips without using a characterSet.", Styles.settingsBody);
            GUILayout.Space(16);
            if (GUILayout.Button("Open Resemble window", Styles.settingsButton, GUILayout.ExpandWidth(false)))
                Resemble_Window.Open();
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);

            //Use the plugin functions in your code
            GUILayout.Label("5. Use the plugin functions in your code", Styles.header);
            SetTitleHeight();
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);

            GUILayout.Space(500);
        }

        private void DrawHelpFooterGUI()
        {
            GUILayout.BeginHorizontal();
            if (scroll[2].y > 50 && GUILayout.Button("<color=grey>Top</color>", Styles.footer))
                scroll[2].y = 0;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<color=grey>Need more help ? Go to plugin documentation page.</color>", Styles.footer))
                WebPage.PluginDoc.Open();
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.EndHorizontal();
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

        private void SetTitleHeight()
        {
            if (Event.current.type == EventType.Repaint)
                titleHeights[titleID] = GUILayoutUtility.GetLastRect().y;
            titleID++;
        }

    }
}
