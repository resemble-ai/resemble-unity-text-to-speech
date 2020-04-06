using UnityEngine;
using UnityEditor;
using Resemble.Structs;

namespace Resemble.GUIEditor
{
    /// <summary> Project tab of preference window. </summary>
    public partial class RessembleSettingsProvider
    {
        private Vector2 projectListScroll;

        private void DrawProjectSettingsGUI()
        {
            //Check changes
            EditorGUI.BeginChangeCheck();

            //Draw connection stuff
            EditorGUI.indentLevel++;
            DrawConnectGUI();
            EditorGUI.indentLevel--;
            Utils.DrawSeparator();
            GUILayout.Space(10);

            if (!Settings.connectionError && Settings.connected)
            {
                if (!Settings.haveProject)
                {
                    Rect rect = EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUI.DrawRect(rect.Shrink(1), new Color(1.0f, 1.0f, 1.0f, 0.2f));
                    rect = DrawProjectsSearchBar();
                    rect.Set(rect.x, rect.y + 20, rect.width, winRect.height - 200);
                    DrawProjectList(rect);
                    EditorGUILayout.EndVertical();

                    if (selected >= Settings.projects.Length)
                        selected = -1;
                    DrawProjectArea(selected == -1 ? null : Settings.projects[selected]);
                }
                else
                {
                    DrawProjectArea(Settings.project);
                }
            }

            //Apply changes to scriptable object
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Settings.instance);

            //Need constant repaint
            Repaint();
        }

        private void DrawProjectFooterGUI()
        {
            GUIContent label = new GUIContent("Status : " + ((Settings.haveProject && Settings.project != null) ?
            "<Color=green>Project \"" + Settings.project.name + "\" binded.</Color>" :
            (Settings.connected ?
            "<Color=red>No binded project.</Color>" :
            "<Color=red>Disconnected.</Color>")));
            GUILayout.Label(label, Styles.footer);
        }

        public void DrawConnectGUI()
        {
            //Resemble API link
            if (Utils.DrawLinkLabel("You can find your Resemble token here: ", 
                resembleAPILinkLabel, Styles.bodyStyle, Styles.linkStyle))
                WebPage.ResembleToken.Open();

            //Token field
            EditorGUI.BeginDisabledGroup(Settings.connected);
            Settings.token = EditorGUILayout.TextField(tokenLabel, Settings.token);
            EditorGUI.EndDisabledGroup();

            //Connect - Disconnect buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!Settings.connected);
            if (GUILayout.Button("Disconnect"))
            {
                Settings.Disconnect();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(Settings.connected || string.IsNullOrEmpty(Settings.token) || Settings.tryToConnect);
            if (GUILayout.Button("Connect"))
            {
                Settings.Connect();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            //Draw errors
            if (Settings.connectionError)
            {
                Utils.DrawErrorBox(Settings.connectionError, Settings.connectionError.code == 401 ?
                    "Autentification error. Please, check your token validity." :
                    "Unable to connect to Resemble API");
            }
        }

        public Rect DrawProjectsSearchBar()
        {
            //Get rect and begin toolbar area
            Rect rect = GUILayoutUtility.GetRect(16, 16).Offset(-2, -2, 4, 0);
            EditorGUI.DrawRect(rect, Color.red);
            GUI.Box(rect, "", EditorStyles.toolbar);
            GUILayout.Space(-18);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            //Draw Search bar
            rect = GUILayoutUtility.GetRect(150, 16);
            rect.Set(rect.x - 6, rect.y + 2, rect.width, rect.height);
            search = searchField.OnToolbarGUI(rect, search);

            //Draw refresh button
            if (GUILayout.Button(Settings.tryToConnect ? "Refreshing..." : " Refresh list ", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) && !Settings.tryToConnect)
            {
                Settings.Connect();
            }

            //End toolbar area
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            return rect;
        }

        private void DrawProjectList(Rect areaRect)
        {
            //Mouse event
            Vector2 mp = Event.current.mousePosition;
            mp.y += projectListScroll.y - areaRect.y + 3;

            //Begin scroll view
            projectListScroll = GUILayout.BeginScrollView(projectListScroll, false, true, 
                GUILayout.MinHeight(100));

            bool searchEnable = !string.IsNullOrEmpty(search);
            string searchPattern = searchEnable ? search.ToLower() : "";
            int j = 0;
            for (int i = 0; i < Settings.projects.Length; i++)
            {
                //Search filter
                string name = Settings.projects[i].name;
                if (searchEnable)
                    if (!name.ToLower().Contains(searchPattern))
                        continue;

                //Draw project line
                Rect rect = GUILayoutUtility.GetRect(Screen.width, 16f);
                rect.Set(rect.x, rect.y, rect.width, rect.height);
                if (rect.Contains(mp))
                {
                    if (GUI.Button(rect, "", GUI.skin.box))
                    {
                        selected = j;
                    }
                }
                if (j == selected)
                    EditorGUI.DrawRect(rect, Color.cyan * 0.2f);
                else if (j % 2 == 0)
                    EditorGUI.DrawRect(rect, Color.white * 0.1f);

                j++;
                GUI.Label(rect, Settings.projects[i].name);
            }

            //No projects with this name
            if (searchEnable && j == 0)
            {
                GUILayout.Label("No projects matching the search", EditorStyles.centeredGreyMiniLabel);
            }

            //No project on Resemble
            if (Settings.projects.Length == 0)
            {
                if (Settings.tryToConnect)
                {
                    GUILayout.Label("Refreshing...", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    if (Utils.BoxWithLink("You don't have a project on Resemble yet. Please go to",
                        "Resemble.ai to create a new project.", MessageType.Info))
                        WebPage.ResembleProjects.Open(); ;
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawProjectArea(Project project)
        {
            //Help box if nothing is selected
            if (project == null)
            {
                EditorGUILayout.HelpBox("Please select a project from the list above", MessageType.Info);
                return;
            }

            //Background box
            Rect rect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true)).Shrink(1);
            GUILayout.Space(160);
            EditorGUILayout.EndVertical();

            Utils.FlatBox(rect, Styles.lightGreen, Styles.background, 0.01f, 40.0f);

            //Header Label
            Rect temp = rect;
            temp.Set(temp.x + 10, temp.y + 10, temp.width - 50, 20);
            GUI.Label(temp, project.name, Styles.projectHeaderLabel);

            //Open project in extern browser button
            temp.Set(temp.x + temp.width + 5, temp.y - 5, 30, 30);
            EditorGUIUtility.AddCursorRect(temp, MouseCursor.Link);
            if (GUI.Button(temp, Resources.instance.externalLink, GUIStyle.none))
                WebPage.ResembleProjects.Open(project.uuid);

            //Description
            temp.Set(rect.x + 5, rect.y + 50, rect.width - 10, rect.height - 90);
            GUI.Label(temp, project.description, EditorStyles.largeLabel);

            //Buttons area
            rect.Set(temp.x, temp.y + temp.height + 5, temp.width, 30);

            if (Settings.haveProject)
            {
                //Unbind button
                temp.Set(rect.x + rect.width - 250, rect.y, 80, rect.height);
                if (Utils.FlatButton(temp, new GUIContent("Unbind"), Color.grey, 0.4f, 0.8f))
                    Settings.UnbindProject();

                //Delete button
                temp.Set(rect.x + rect.width - 170, rect.y, 170, rect.height);
                if (Utils.FlatButton(temp, new GUIContent("Import clips"), Styles.purple, 0.4f, 0.8f))
                    Settings.ImportClips(project);
            }
            else
            {
                //Bind button
                temp.Set(rect.x + rect.width - 80, rect.y, 80, rect.height);
                if (Utils.FlatButton(temp, new GUIContent("Bind"), Styles.purple, 0.4f, 0.8f))
                    Settings.BindProject(project);

                //Delete button
                temp.Set(rect.x + rect.width - 180, rect.y, 100, rect.height);
                if (Utils.FlatButton(temp, new GUIContent("Delete"), Color.grey, 0.4f, 0.8f))
                    Settings.DeleteProject(project);
            }
        }
    }
}

