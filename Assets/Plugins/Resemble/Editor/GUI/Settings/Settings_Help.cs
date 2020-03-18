using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    /// <summary> Help tab of preference window. </summary>
    public partial class RessembleSettingsProvider
    {

        private static float[] titleHeights = new float[7];
        private static int titleID = 0;

        private readonly TextItem[] items = new TextItem[]
        {
            //Table of contents
            TextItem.Link("0. Begin with Resemble", 0, false),
            TextItem.Link("1. Setup the project", 1, false),
            TextItem.Link("2. Create a Speech", 2, false),
            TextItem.Link("3. Use emotions", 3, false),
            TextItem.Link("4. Use a Speech in your scripts", 4, false),
            TextItem.Link("5. Generate one shot clips", 5, false),
            TextItem.Separator(),
            TextItem.Space(10),


            //Begin with Resemble
            TextItem.Header("0. Begin with Resemble"),
            TextItem.Text("To use the plugin you first need a Resemble account. If you don't have one you can create it here :\n"),
            TextItem.Link("Sign up", WebPage.ResembleSignUp),
            TextItem.Space(25),


            //Setup project
            TextItem.Header("1. Setup the project"),
            TextItem.Text("Your unity project must be connected to a Resemble project. For this you can go to the Project page.\n"),
            TextItem.Image(0),
            TextItem.Text("\nYou have to enter your Resemble token, you can find it here: "),
            TextItem.Link("Resemble token", WebPage.ResembleToken),
            TextItem.Text("\nThen, you can bind a project from the list.\n"),
            TextItem.Image(1),
            TextItem.Space(25),


            //Create a Speech
            TextItem.Header("2. Create a Speech"),
            TextItem.Text("To create a new Speech, you can select"),
            TextItem.Quote("Assets > Create > Resemble Speech"),
            TextItem.Text("or right-click on a folder in your Project window and select"),
            TextItem.Quote("Create > Resemble Speech"),
            TextItem.Space(10),
            TextItem.Image(2),
            TextItem.Space(10),

            //Create a clip
            TextItem.Title("   Create a Clip"),
            TextItem.Text("You can create multiple clips in a speech that will all share the same voice. This is also a convenient way to organize your clips."),
            TextItem.Link("( See : Use a speech in your scripts)", 4, true),
            TextItem.Text("\nSo first you need to select a voice. Then you can add clips with the [Add] button or import existing clips from your Resemble project with [Import]."),
            TextItem.Image(3),
            TextItem.Text("Write your text, click generate and your audio file will be created."),
            TextItem.Space(10),
            TextItem.Quote("To change the location of the generated files you can go to :\nPreferences > Resemble > Paths"),
            TextItem.Space(10),
            TextItem.Quote("When you start the generation of a file, a placeholder is directly generated. You can already reference it in your scripts or your scene. It will be automatically replaced by the generated version without breaking any links."),
            TextItem.Space(25),


            //Use emotion
            TextItem.Header("3. Use Emotions"),
            TextItem.Text("You can add emotion to your texts. To do this, select the desired text portion and clic on [Add Emotion]."),
            TextItem.Image(4),
            TextItem.Text("\n  - You can always edit an emotion by placing your cusor over it and the clicking on [Edit]."),
            TextItem.Text("\n  - You can also edit and emotion by shift + right-click on it."),
            TextItem.Text("\n  - To remove an emotion, put your cursor on it or select the desired area and click on [Remove]."),
            TextItem.Space(10),
            TextItem.Quote("The context menu (right-click) allows you to manage emotion quickly.\nYou can also shift + rightClick on an emotion to edit it directly."),
            TextItem.Space(25),


            //Use a Speech in your scripts
            TextItem.Header("4. Use Speech in your scripts"),
            TextItem.Text("You can use Speech in your script. this can be handy to keep your data organized."),
            TextItem.Code(Code_01, 210),
            TextItem.Space(25),

            TextItem.Title("   UserData"),
            TextItem.Text("With the UserData, you can also store useful information when you create your clips in order to access them later in your scripts"),
            TextItem.Image(5),
            TextItem.Text("\nIn the example below, if the funtion is called with dialogIndex = 3. Allclips of the speech containing the Dialogue tag with a value of 3 will be return."),
            TextItem.Code(Code_02, 290),
            TextItem.Space(25),


            //Generate one-shot clips
            TextItem.Header("5. Generate One-Shot clips"),
            TextItem.Text("You can generate oneShot clips. This method allows you to quickly create audio files without saving clips to the Resemble project.\nTo open the one-shot window go to"),
            TextItem.Quote("Window > Audio > Resemble"),
            TextItem.Text("\nAnd select the OneShot tab."),
            TextItem.Image(6),
            TextItem.Text("\nThe window works like a classic clip. You enter your text, choose a voice, then press the generate button.\n\nChoose the location and the name of the file and the generation will start.\n\nThe file will be generated in the background. You can see its progress in the Pending Request Tab. Once the generation is complete you will receive a notification.\n"),
            TextItem.Quote("When you start the generation of a file, a placeholder is directly generated. You can already reference it in your scripts or your scene. It will be automatically replaced by the generated version without breaking any links."),
            TextItem.Space(25),

            TextItem.Space(100),
        };

        #region Code
        private static readonly string Code_01 = string.Format("{0}" +
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
                "<Color=#ffffff>", "<Color=#6084f0>", "<Color=#57bda7>", "</Color>", "{", "}", "      ", "<Color=#00c732>");

        private static readonly string Code_02 = string.Format("{0}" +
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
                "<Color=#ffffff>", "<Color=#6084f0>", "<Color=#57bda7>", "</Color>", "{", "}", "      ", "<Color=#00c732>", "<Color=#d67e11>");
        //      {0} Normal         {1} Blue            {2}Green            {3}       {4} {5}   {6}       {7} Comment          {8} string
        #endregion

        private void DrawHelpSettingsGUI()
        {
            titleID = 0;
            for (int i = 0; i < items.Length; i++)
                items[i].Draw(winRect.width - 70);
        }

        private struct TextItem
        {
            public Type type;
            private WebPage page;
            private string label;
            private int index;
            private float size;
            private bool indent;

            public static TextItem Text(string label)
            { return new TextItem() { type = Type.Text, label = label }; }

            public static TextItem Header(string label)
            { return new TextItem() { type = Type.Header, label = label }; }

            public static TextItem Title(string label)
            { return new TextItem() { type = Type.Title, label = label }; }

            public static TextItem Link(string label, WebPage page)
            { return new TextItem() { type = Type.Link, label = label, index = -1, page = page }; }

            public static TextItem Link(string label, int headerID, bool indent)
            { return new TextItem() { type = Type.Link, label = label, index = headerID, indent = indent }; }

            public static TextItem Image(int id)
            { return new TextItem() { type = Type.Image, index = id }; }

            public static TextItem Space(int size)
            { return new TextItem() { type = Type.Space, size = size }; }

            public static TextItem Separator()
            { return new TextItem() { type = Type.Separator }; }

            public static TextItem Quote(string label)
            { return new TextItem() { type = Type.Quote, label = label }; }

            public static TextItem Code(string label, float size)
            { return new TextItem() { type = Type.Code, label = label, size = size }; }


            public void Draw(float width)
            {
                switch (type)
                {
                    case Type.Text:
                        GUILayout.Label(label, Styles.settingsBody, GUILayout.Width(width));
                        break;
                    case Type.Link:
                        if (index == -1)
                        {
                            if (GUILayout.Button(label, Styles.settingsLink, GUILayout.ExpandWidth(false)))
                                page.Open();
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                        }
                        else
                        {
                            if (GUILayout.Button(label, indent ? Styles.settingsLink : Styles.settingsIndex, GUILayout.ExpandWidth(false)))
                                scroll[2].y = titleHeights[index];
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                        }
                        break;
                    case Type.Header:
                        GUILayout.Label(label, Styles.header, GUILayout.Width(width));
                        if (Event.current.type == EventType.Repaint)
                            titleHeights[titleID] = GUILayoutUtility.GetLastRect().y;
                        titleID++;
                        break;
                    case Type.Title:
                        GUILayout.Label(label, Styles.header, GUILayout.Width(width));
                        break;
                    case Type.Image:
                        GUILayout.Label(Resources.instance.docImages[index], Styles.settingsBody, GUILayout.Width(width));
                        break;
                    case Type.Separator:
                        Utils.DrawSeparator();
                        break;
                    case Type.Space:
                        GUILayout.Space(size);
                        break;
                    case Type.Quote:
                        GUILayout.Label(label, Styles.settingsQuote, GUILayout.Width(width));
                        break;
                    case Type.Code:
                        Color c = GUI.color;
                        GUI.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
                        GUILayout.BeginVertical(Styles.codeBox);
                        EditorGUILayout.SelectableLabel(label, Styles.settingsCode, GUILayout.Height(size), GUILayout.Width(width));
                        GUILayout.EndVertical();
                        GUI.color = c;
                        break;
                }
            }
        }

        private enum Type
        {
            Text,
            Link,
            Header,
            Image,
            Separator,
            Space,
            Quote,
            Code,
            Title,
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
    }
}
