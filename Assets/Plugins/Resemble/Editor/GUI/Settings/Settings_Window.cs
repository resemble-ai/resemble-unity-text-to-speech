using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using Resemble;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public partial class RessembleSettingsProvider : SettingsProvider
    {

        //Misc GUI stuff
        private EditorWindow prefWindow;
        private static Vector2[] scroll = new Vector2[3];
        private int selected = -1;
        private VisualElement visualElement;
        private Rect winRect;
        public static int pageID;

        //Search
        private string search;
        private SearchField searchField = new SearchField();

        //Labels & tooltips
        private GUIContent resembleAPILinkLabel = new GUIContent("Resemble API.", "Go to the token page on resemble.ai.");
        private GUIContent tokenLabel = new GUIContent("Token", "Your token is used to identify you to Resemble services.");


        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            visualElement = rootElement;

            //Refresh projects
            if (Settings.haveProject && Settings.project == null && !Settings.tryToConnect)
                Settings.Connect();
        }

        public override void OnGUI(string searchContext)
        {
            //Init styles
            Styles.Load();

            //Set a min size on pref window
            if (prefWindow == null)
            {
                Assembly assem = typeof(EditorWindow).Assembly;
                System.Type t = assem.GetType("UnityEditor.PreferenceSettingsWindow");
                prefWindow = EditorWindow.GetWindow(t, false, "Preferences", false);
                prefWindow.minSize = new Vector2(550, 500);
            }

            //Draw (project | options | help) toolbar
            DrawToolbar();

            //Open a scrollable layout area
            Utils.DrawSeparator();
            winRect = visualElement.contentRect.Offset(9, 30, -20, -80);
            GUI.BeginClip(winRect);
            GUILayout.BeginArea(new Rect(0, 5, winRect.width, winRect.height - 10));
            scroll[pageID] = GUILayout.BeginScrollView(scroll[pageID], GUIStyle.none);
            GUILayout.Space(5);

            //Draw page content
            switch (pageID)
            {
                default:
                    DrawProjectSettingsGUI();
                    break;
                case 1:
                    DrawOptionsSettingsGUI();
                    break;
                case 2:
                    DrawHelpSettingsGUI();
                    break;
            }

            //Close the scrollable layout area
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.EndClip();
            GUILayout.Space(winRect.height - 20);
            Utils.DrawSeparator();

            //Draw page footer
            switch (pageID)
            {
                default:
                    DrawProjectFooterGUI();
                    break;
                case 1:
                    DrawOptionsFooterGUI();
                    break;
                case 2:
                    DrawHelpFooterGUI();
                    break;
            }
        }

        private void DrawToolbar()
        {
            pageID = GUILayout.Toolbar(pageID, new string[] { "Project", "Options", "Help" });
        }

        //Settings tags
        class ContentStyles
        {
            public static GUIContent resemble = new GUIContent("Resemble");
            public static GUIContent token = new GUIContent("Token");
        }

        public RessembleSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        [SettingsProvider]
        public static SettingsProvider CreateResembleSettingsProvider()
        {
            var provider = new RessembleSettingsProvider("Preferences/Resemble", SettingsScope.User);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<ContentStyles>();
            return provider;
        }
    }
}