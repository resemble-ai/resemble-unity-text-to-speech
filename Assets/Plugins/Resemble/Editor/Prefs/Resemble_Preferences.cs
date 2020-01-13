using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.IMGUI.Controls;

class RessembleSettingsProvider : SettingsProvider
{
    private SerializedObject m_CustomSettings;
    private Vector2 scroll;

    private Error connectError;
    private bool connected;
    private bool haveProject;
    private string search;
    private Rect searchRect;
    private SearchField searchField = new SearchField();

    class ContentStyles
    {
        public static GUIContent number = new GUIContent("My Number");
        public static GUIContent token = new GUIContent("Token");
    }

    public RessembleSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
        : base(path, scope) { }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
         m_CustomSettings = Resemble_Settings.GetSerializedSettings();
        if (!string.IsNullOrEmpty(Resemble_Settings.token))
            APIBridge.GetProjects(GetProjectCallback);
    }

    public override void OnGUI(string searchContext)
    {
        Styles.Load();

        //Get up-dated settings
        m_CustomSettings.UpdateIfRequiredOrScript();

        EditorGUI.BeginChangeCheck();

        //Draw connection stuff
        EditorGUI.indentLevel++;
        DrawConnectGUI();
        GUIUtils.DrawSeparator();
        DrawProjectsGUI();
        EditorGUI.indentLevel--;
        
        //Apply changes to serialized object
        if (EditorGUI.EndChangeCheck())
            m_CustomSettings.ApplyModifiedProperties();
    }

    public void DrawConnectGUI()
    {
        //Resemble API link
        GUIUtils.DrawLinkLabel("You can find your Ressemble token here: ", "Ressemble API.",
            "https://app.resemble.ai/account/api", Styles.bodyStyle, Styles.linkStyle);

        //Token
        EditorGUI.BeginDisabledGroup(connected);
        Resemble_Settings.token = EditorGUILayout.TextField("Token", Resemble_Settings.token);
        EditorGUI.EndDisabledGroup();

        //Connect - Disconnect buttons
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!connected);
        if (GUILayout.Button("Disconnect"))
            connected = false;
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(connected || string.IsNullOrEmpty(Resemble_Settings.token));
        if (GUILayout.Button("Connect"))
            APIBridge.GetProjects(GetProjectCallback);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //Draw errors
        if (connectError)
        {
            GUIUtils.DrawErrorBox(connectError, connectError.code == 401 ?
                "Autentification error. Please, check your token validity." :
                "Unable to connect to Resemble API");
        }
    }

    public void DrawProjectsGUI()
    {
        if (connectError || !connected)
            return;


        GUILayout.Space(10);
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        //List Popup
        //EditorGUILayout.Popup(0, Resemble_Settings.projectNames.ToArray(), EditorStyles.toolbarPopup);

        //Search
        Rect rect = GUILayoutUtility.GetRect(150, 16);
        rect.Set(rect.x - 6, rect.y + 2, rect.width, rect.height);
        search = searchField.OnToolbarGUI(rect, search);
        searchRect = GUILayoutUtility.GetLastRect();

        //Refresh button
        if (GUILayout.Button("Refresh list", EditorStyles.toolbarButton))
            APIBridge.GetProjects(GetProjectCallback);
        if (GUILayout.Button("Unbind project", EditorStyles.toolbarButton))
            Resemble_Settings.project = null;

        GUILayout.EndHorizontal();

        
        if (Resemble_Settings.project == null)
        {
            EditorGUILayout.HelpBox("Select a project in the list or create a new one.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(Resemble_Settings.project.name, MessageType.None);
        }


        ShowProjectList();
    }

    public void ShowProjectList()
    {
        if (!string.IsNullOrEmpty(search) && searchField.HasFocus())
        {
            Event e = Event.current;
            string lowerSearch = search.ToLower();
            string[] searchResults = Resemble_Settings.projectNames.Where(x => x.ToLower().Contains(lowerSearch)).ToArray();
            if (searchResults.Length == 0)
                searchResults = new string[] { "No results" };
            Rect rect = searchRect;
            rect.Set(rect.x, rect.y, rect.width, searchResults.Length * 16 + 18);
            if (!rect.Contains(e.mousePosition) && e.type == EventType.Repaint)
                GUI.FocusControl("None");

            rect.Set(rect.x + 15, rect.y + 14, rect.width - 14, searchResults.Length * 16 + 4);
            GUI.Box(rect, "");
            rect.Set(rect.x + 2, rect.y + 2, rect.width - 4, 16);

            for (int i = 0; i < searchResults.Length; i++)
            {
                if (rect.Contains(e.mousePosition))
                    EditorGUI.DrawRect(rect, Color.grey * 0.3f);
                if (GUI.Button(rect, searchResults[i], EditorStyles.label))
                {
                    GUI.FocusControl("None");
                    search = "";
                    Resemble_Settings.SelectProjectByName(searchResults[i]);
                }
                rect.y += 16;
            }
            Repaint();
        }
    }

    private void GetProjectCallback(Project[] projects, Error error)
    {
        this.connectError = error;
        this.connected = !error;
        Resemble_Settings.projects = projects;
        Repaint();
    }

    [SettingsProvider]
    public static SettingsProvider CreateResembleSettingsProvider()
    {
        var provider = new RessembleSettingsProvider("Preferences/Resemble", SettingsScope.User);
        provider.keywords = GetSearchKeywordsFromGUIContentProperties<ContentStyles>();
        return provider;
    }
}