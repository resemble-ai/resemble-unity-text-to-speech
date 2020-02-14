using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
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
            GUIUtils.DrawSeparator();
            GUILayout.Space(10);

            if (!connectError && Settings.connected)
            {
                if (!Settings.haveProject)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    Rect rect = DrawProjectsSearchBar();
                    rect.Set(rect.x, rect.y + 20, rect.width, winRect.height - 200);
                    DrawProjectList(rect);
                    GUILayout.EndVertical();
                }
                DrawProjectAreaUnityStyle();
            }

            //Apply changes to scriptable object
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Settings.instance);

            //Need constant repaint
            Repaint();
        }

        private void DrawProjectFooterGUI()
        {
            GUIContent label = new GUIContent("Status : " + (Settings.haveProject ?
            "<Color=green>Project \"" + Settings.project.name + "\" binded.</Color>" :
            (Settings.connected ?
            "<Color=red>No binded project.</Color>" :
            "<Color=red>Disconnected.</Color>")));
            GUILayout.Label(label, Styles.footer);
        }

        public void DrawConnectGUI()
        {
            //Resemble API link
            GUIUtils.DrawLinkLabel("You can find your Resemble token here: ", resembleAPILinkLabel,
                "https://app.resemble.ai/account/api", Styles.bodyStyle, Styles.linkStyle);

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
                Settings.connected = false;
                Settings.haveProject = false;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(Settings.connected || string.IsNullOrEmpty(Settings.token) || tryConnecting);
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
            searchRect = GUILayoutUtility.GetLastRect();

            //Draw refresh button
            if (GUILayout.Button(tryConnecting ? "Refreshing..." : " Refresh list ", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) && !tryConnecting)
            {
                Settings.projects = new Project[0];
                tryConnecting = true;
                APIBridge.GetProjects(GetProjectCallback);
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
                Rect rect = GUILayoutUtility.GetRect(1, 16f);
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
                    EditorGUI.DrawRect(rect, Color.grey * 0.1f);

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
                if (tryConnecting)
                {
                    GUILayout.Label("Refreshing...", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    if (GUIUtils.BoxWithLink("You don't have a project on Resemble yet. Please go to",
                        "Resemble.ai to create a new project.", MessageType.Info))
                        WebPage.ResembleProjects.Open(); ;
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawProjectArea()
        {
            if (selected == -1)
            {
                EditorGUILayout.HelpBox("Please select a project from the list above", MessageType.Info);
                return;
            }

            Project project = Settings.projects[selected];
            if (Settings.haveProject)
                project = Settings.project;

            Rect rect = GUILayoutUtility.GetRect(winRect.width - 100, 200);
            rect.Set(rect.x + 5, rect.y + 20, Mathf.Min(rect.width, 446), 183);
            GUI.DrawTexture(rect, Resources.instance.projectHeader);
            rect.Set(rect.x + 20, rect.y, rect.width - 50, 63);
            GUI.Label(rect, project.name, Styles.projectHeaderLabel);

            rect.Set(rect.x, rect.y + rect.height, rect.width + 10, 80);
            GUI.Label(rect, project.description, EditorStyles.largeLabel);

            rect.Set(rect.x - 10, rect.y + rect.height + 5, rect.width + 20, 30);

            if (Settings.haveProject)
            {
                rect.Set(rect.x, rect.y, 200, rect.height);
                GUIUtils.FlatButton(rect, "Import all pods in wav", Color.grey, 1.0f, rect.Contains(Event.current.mousePosition) ? 0.8f : 1.0f);
                rect.x += rect.width;
                GUIUtils.FlatButton(rect, "Unbind", Color.grey);
            }
            else
            {
                if (GUIUtils.FlatButton(rect, "Bind", Color.grey, 1.0f, rect.Contains(Event.current.mousePosition) ? 0.8f : 1.0f))
                {
                    Settings.haveProject = true;
                    Settings.project = Settings.projects[selected];
                    Settings.projectUUID = Settings.projects[selected].uuid;
                    EditorUtility.SetDirty(Settings.instance);
                }
            }

            rect.Set(rect.x + rect.width - 30, rect.y - 142, 30, 30);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, Resources.instance.externalLink, GUIStyle.none))
                GUIUtils.OpenProjectInBrowser(Settings.project.uuid);
        }

        private void DrawProjectAreaUnityStyle()
        {
            if (selected == -1)
            {
                EditorGUILayout.HelpBox("Please select a project from the list above", MessageType.Info);
                return;
            }

            Project project = Settings.projects[selected];
            if (Settings.haveProject)
                project = Settings.project;

            //Begin box area
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            //Name and description
            GUILayout.Label("Project name : " + project.name);
            GUILayout.Label("Project description : " + project.description);

            GUILayout.FlexibleSpace();

            //Bot buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (Settings.haveProject)
            {
                if (GUILayout.Button("Unbind"))
                {
                    Settings.project = null;
                    Settings.projectUUID = "";
                    Settings.haveProject = false;
                    Settings.SetDirty();
                }
            }
            else
            {
                if (GUILayout.Button("Bind"))
                {
                    Settings.project = project;
                    Settings.project.uuid = project.uuid;
                    Settings.haveProject = true;
                    Settings.SetDirty();
                }
            }
            GUILayout.EndHorizontal();

            //Close box area
            GUILayout.EndVertical();
            GUILayout.Space(16);
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
    }
}

