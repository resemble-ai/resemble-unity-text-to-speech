using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    /// <summary> Help tab of preference window. </summary>
    public partial class RessembleSettingsProvider
    {

        private float[] titleHeights = new float[7];
        private int titleID = 0;

        private void DrawHelpSettingsGUI()
        {
            //Table of contents
            DrawLink("0. Begin with Resemble", titleHeights[0]);
            DrawLink("1. Setup the project", titleHeights[1]);
            DrawLink("2. Create a Speech", titleHeights[2]);
            DrawLink("3. Use emotions", titleHeights[3]);
            DrawLink("4. Use a Speech in your scripts", titleHeights[4]);
            DrawLink("5. Generate one shot clips", titleHeights[5]);
            DrawLink("6. Use the plugin functions in your code", titleHeights[6]);
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
            GUILayout.Label("2. Create a Speech", Styles.header);
            SetTitleHeight();
            GUILayout.Label("To create a new Speech, you can select \n<Color=grey>Assets > Create > Resemble Speech</Color>\nor right-click " +
                "on a folder in your Project window and select\n<Color=grey>Create > Resemble Speech</Color>", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[1], Styles.settingsBody);
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);


            //Use emotion
            GUILayout.Label("3. Use emotions", Styles.header);
            SetTitleHeight();
            GUILayout.Label("You can add emotion to your texts. To do this, select the desired text portion and click on [Add Emotion].");
            GUILayout.Label(Resources.instance.docImages[2], Styles.settingsBody);
            GUILayout.Label("You can always edit an emotion by placing your cursor over it and then clicking on [Edit]. You can also edit an emotion by shift + right-click on it.\n\n" +
                "To remove an emotion, put your cursor on it or select the desired area and click on [Delete].\n\n" +
                "The context menu (right click) also allows you to manage emotions quickly.", Styles.settingsBody);;
            GUILayout.Space(25);


            //Use a CharacterSet in your scripts
            GUILayout.Label("4. Use a Speech in your scripts", Styles.header);
            SetTitleHeight();
            GUILayout.Label("You can use Speech in your script. This can be handy to keep your data organized.", Styles.settingsBody);
            Color c = GUI.color;
            GUI.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            GUILayout.BeginVertical(Styles.codeBox);
            EditorGUILayout.SelectableLabel(
                string.Format(
                "{0}" +
                "{1}using{3} UnityEngine;\n" +
                "{1}using{3} Resemble;\n" +
                "\n" +
                "{1}public class {3}{2}Orator{3} : {2}MonoBehaviour{3}\n{4}" +
                "\n" +
                "{1}{6}public{3} {2}Speech{3} mySpeech;\n" +
                "\n" +
                "{7}{6}// Find and play the clip of the given name.{3}\n" +
                "{1}{6}public void{3} PlayDialogue({1}string{3} name)\n" +
                "{6}{4}\n" +
                "{2}{6}{6}AudioClip{3} audioclip = mySpeech.GetAudio(name);\n" +
                "{2}{6}{6}AudioSource{3}.PlayClipAtPoint(audioClip, transform.position);\n" +
                "{6}{5}\n" +
                "{5}{3}", 
                "<Color=#ffffff>", "<Color=#6084f0>", "<Color=#57bda7>", "</Color>", "{", "}", "      ", "<Color=#00c732>"), Styles.settingsCode, GUILayout.Height(210));
            //   {0} Normal         {1} Blue            {2}Green            {3}       {4} {5}   {6}       {7} Comment
            GUILayout.EndVertical();
            GUI.color = c;
            GUILayout.Space(16);
            GUILayout.Label("With the UserData, you can also store useful information when you create your clips in order to access them later in your scripts.", Styles.settingsBody);
            GUILayout.Label(Resources.instance.docImages[3], Styles.settingsBody);
            GUI.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            GUILayout.BeginVertical(Styles.codeBox);
            EditorGUILayout.SelectableLabel(
                string.Format(
                "{0}" +
                "{1}using{3} System.Collections.Generic;\n" +
                "{1}using{3} UnityEngine;\n" +
                "{1}using{3} Resemble;\n" +
                "\n" +
                "{1}public class {3}{2}Orator{3} : {2}MonoBehaviour{3}\n{4}" +
                "\n" +
                "{1}{6}public{3} {2}Speech{3} mySpeech;\n" +
                "\n" +
                "{7}{6}// Returns a list containing all choices for the dialog at the given index.{3}\n" +
                "{1}{6}public {3}{2}list{3}<{1}string{3}> GetDialogueWheelChoices({1}int{3} dialogueIndex)\n" +
                "{6}{4}\n" +
                "{2}{6}{6}Label{3} label = {1}new{3} {2}Label{3}({8}\"Dialogue\"{3}, dialogueIndex);\n" +
                "{2}{6}{6}List{3}<{2}Clip{3}> mySpeech.GetClipsWithLabel(label);\n" +
                "{2}{6}{6}List{3}<{1}string{3}> choiceName = {1}new{3} {2}List{3}<{1}string{3}>();\n" +
                "{1}{6}{6}foreach{3} ({1}var{3} clip {1}in{3} clips)\n" +
                "{6}{6}{6}choiceName.Add(clip.userdata);\n" +
                "{6}{6}{1}return {3}choiceName;\n" +
                "{6}{5}\n" +
                "{5}{3}",
                "<Color=#ffffff>", "<Color=#6084f0>", "<Color=#57bda7>", "</Color>", "{", "}", "      ", "<Color=#00c732>", "<Color=#d67e11>"), Styles.settingsCode, GUILayout.Height(290));
            //   {0} Normal         {1} Blue            {2}Green            {3}       {4} {5}   {6}       {7} Comment          {8} string
            GUILayout.EndVertical();
            GUI.color = c;
            GUILayout.Space(16);
            GUILayout.Label("In this example, if the function is called with dialogIndex = 3. All clips of the speech containing the Dialogue tag with a value of 3 will be return.", Styles.settingsBody);
            GUILayout.Space(25);


            //Generate one shot clips
            GUILayout.Label("5. Generate one shot clips", Styles.header);
            SetTitleHeight();
            GUILayout.Label("You can also use Resemble to quickly create AudioClips without using a Speech.", Styles.settingsBody);
            GUILayout.Space(16);
            if (GUILayout.Button("Open Resemble window", Styles.settingsButton, GUILayout.ExpandWidth(false)))
                Resemble_Window.Open();
            GUILayout.Label("WIP...", Styles.settingsBody);
            GUILayout.Space(25);


            //Use the plugin functions in your code
            GUILayout.Label("6. Use the plugin functions in your code", Styles.header);
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
