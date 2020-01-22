using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.IMGUI.Controls;

class RessembleSettingsProvider : SettingsProvider
{
    private SerializedObject m_CustomSettings;
    private Vector2 scroll;
    private float screenWidth;

    private Error connectError;
    private bool haveProject;
    private string search;
    private Rect searchRect;
    private SearchField searchField = new SearchField();
    private bool tryConnecting;
    private int selected = -1;

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

        RefreshSelected();
    }

    private void RefreshSelected()
    {
        //Get selected project
        if (Resemble_Settings.project != null)
            for (int i = 0; i < Resemble_Settings.projects.Length; i++)
                if (Resemble_Settings.projects[i] == Resemble_Settings.project)
                    selected = i;
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

        DrawProjectArea();
        //DrawFooter();

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
        EditorGUI.BeginDisabledGroup(Resemble_Settings.connected);
        Resemble_Settings.token = EditorGUILayout.TextField("Token", Resemble_Settings.token);
        EditorGUI.EndDisabledGroup();

        //Connect - Disconnect buttons
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!Resemble_Settings.connected);
        if (GUILayout.Button("Disconnect"))
        {
            Resemble_Settings.connected = false;
            Resemble_Settings.haveProject = false;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(Resemble_Settings.connected || string.IsNullOrEmpty(Resemble_Settings.token) || tryConnecting);
        if (GUILayout.Button("Connect"))
        {
            tryConnecting = true;
            APIBridge.GetProjects(GetProjectCallback);
        }
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
        if (connectError || !Resemble_Settings.connected)
            return;

        GUILayout.Space(10);
        Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (Event.current.type == EventType.Repaint)
            screenWidth = rect.width;

        //Search
        rect = GUILayoutUtility.GetRect(150, 16);
        rect.Set(rect.x - 6, rect.y + 2, rect.width, rect.height);
        search = searchField.OnToolbarGUI(rect, search);
        searchRect = GUILayoutUtility.GetLastRect();

        //Refresh button
        if (GUILayout.Button(tryConnecting ? "Refreshing..." : " Refresh list ", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) && !tryConnecting)
        {
            Resemble_Settings.projects = new Project[0];
            tryConnecting = true;
            APIBridge.GetProjects(GetProjectCallback);
        }

        EditorGUILayout.EndHorizontal();

        //ShowProjectList();
        DrawList();

        Repaint();
    }

    public void DrawList()
    {
        Rect areaRect = new Rect(0, 97, screenWidth, Screen.height - 400);
        areaRect.width = screenWidth;
        GUILayout.BeginArea(areaRect, GUI.skin.box);

        //Mouse event
        Vector2 mp = Event.current.mousePosition;
        mp.y -= 6;

        scroll = GUILayout.BeginScrollView(scroll, false, true);
        bool searchEnable = !string.IsNullOrEmpty(search);
        string searchPattern = searchEnable ? search.ToLower() : "";
        int j = 0;
        for (int i = 0; i < Resemble_Settings.projects.Length; i++)
        {
            //Search filter
            string name = Resemble_Settings.projects[i].name;
            if (searchEnable)
                if (!name.ToLower().Contains(searchPattern))
                    continue;

            //Draw project line
            Rect rect = GUILayoutUtility.GetRect(1, 16f);
            rect.Set(rect.x + 20, rect.y, rect.width - 30, rect.height);
            if (rect.Contains(mp))
            {
                GUI.Box(rect, "");
                Rect btnRect = new Rect(rect.x + rect.width - 90, rect.y, 90, rect.height);
                if (GUI.Button(btnRect, "Bind"))
                {
                    Resemble_Settings.project = Resemble_Settings.projects[i];
                    Resemble_Settings.haveProject = true;
                    selected = j;
                }
            }
            if (j == selected)
                EditorGUI.DrawRect(rect, Color.cyan * 0.2f);
            else if (j % 2 == 0)
                EditorGUI.DrawRect(rect, Color.grey * 0.1f);

            j++;
            GUI.Label(rect, Resemble_Settings.projects[i].name);
        }

        //No projects with this name
        if (searchEnable && j == 0)
        {
            GUILayout.Label("No projects matching the search", EditorStyles.centeredGreyMiniLabel);
        }

        //No project on Resemble
        if (Resemble_Settings.projects.Length == 0)
        {
            if (tryConnecting)
            {
                GUILayout.Label("Refreshing...", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                if (GUIUtils.BoxWithLink("You don't have a project on Resemble yet. Please go to",
                    "Resemble.ai to create a new project.", MessageType.Info))
                    GUIUtils.OpenResembleWebSite();
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUILayout.Space(areaRect.height);
    }

    private void DrawProjectArea()
    {
        if (!Resemble_Settings.connected)
            return;

        if (!Resemble_Settings.haveProject)
        {
            EditorGUILayout.HelpBox("Please bind a project from the list above", MessageType.Info);
            return;
        }

        Rect rect = GUILayoutUtility.GetRect(screenWidth, 200);
        rect.Set(rect.x + 20, rect.y + 20, Mathf.Min(rect.width - 40, 446), 183);
        GUI.DrawTexture(rect, Resemble_Resources.instance.projectHeader);
        rect.Set(rect.x + 20, rect.y, rect.width - 50, 63);
        GUI.Label(rect, Resemble_Settings.project.name, Styles.projectHeaderLabel);

        rect.Set(rect.x, rect.y + rect.height, rect.width + 10, 80);
        GUI.Label(rect, Resemble_Settings.project.description, EditorStyles.largeLabel);

        rect.Set(rect.x + rect.width - 185, rect.y + rect.height + 5, 200, 30);
        GUIUtils.FlatButton(rect, "Import all pods in wav", Color.grey,  1.0f, rect.Contains(Event.current.mousePosition) ? 0.8f : 1.0f);

        rect.Set(rect.x + rect.width - 30, rect.y - 142, 30, 30);
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        if (GUI.Button(rect, Resemble_Resources.instance.externalLink, GUIStyle.none))
            GUIUtils.OpenResembleWebSite();
    }

    private void DrawFooter()
    {
        Rect rect = new Rect(screenWidth - 190, Screen.height - 90, 190, 30);
        GUI.Label(rect, "Status : " + (Resemble_Settings.project == null ?
            "<Color=red>No connected project.</Color>" :
            "<Color=green>Project connected.</Color>"), Styles.footer);
    }

    private void GetProjectCallback(Project[] projects, Error error)
    {
        this.tryConnecting = false;
        this.connectError = error;
        Resemble_Settings.connected = !error;
        Resemble_Settings.projects = projects;
        RefreshSelected();
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