using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using Resemble;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    public partial class RessembleSettingsProvider : SettingsProvider
    {

        //Misc GUI stuff
        private Vector2[] scroll = new Vector2[3];
        private int selected = -1;
        private VisualElement visualElement;
        private Rect winRect;
        public static int pageID;

        //Connection
        private Error connectError;
        private bool haveProject;
        private bool tryConnecting;

        //Search
        private string search;
        private Rect searchRect;
        private SearchField searchField = new SearchField();

        //Labels & tooltips
        private GUIContent resembleAPILinkLabel = new GUIContent("Resemble API.", "Go to the token page on resemble.ai.");
        private GUIContent tokenLabel = new GUIContent("Token", "Your token is used to identify you to Resemble services.");

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            if (!string.IsNullOrEmpty(Settings.token))
                APIBridge.GetProjects(GetProjectCallback);
            visualElement = rootElement;
        }

        private void RefreshSelected()
        {
            //Get selected project
            if (Settings.projects != null)
                for (int i = 0; i < Settings.projects.Length; i++)
                    if (Settings.projects[i].uuid == Settings.projectUUID)
                    {
                        selected = i;
                        Settings.haveProject = true;
                    }
        }

        public override void OnGUI(string searchContext)
        {
            //Init styles
            Styles.Load();

            //Draw (project | paths | help) toolbar
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
                    DrawPathsSettingsGUI();
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
                    DrawPathsFooterGUI();
                    break;
                case 2:
                    DrawHelpFooterGUI();
                    break;
            }
        }

        private void DrawToolbar()
        {
            pageID = GUILayout.Toolbar(pageID, new string[] { "Project", "Paths", "Help" });
        }

        private void GetProjectCallback(Project[] projects, Error error)
        {
            this.tryConnecting = false;
            this.connectError = error;
            Settings.connected = !error;
            Settings.projects = projects;
            RefreshSelected();
            Repaint();
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