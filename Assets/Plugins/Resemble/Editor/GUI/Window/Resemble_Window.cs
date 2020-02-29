using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble.Structs;
using Resemble;

namespace Resemble.GUIEditor
{
    public partial class Resemble_Window : EditorWindow
    {
        public static Resemble_Window window;
        private Vector2 windowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                if (_windowSize != value)
                {
                    _windowSize = value;
                    OnResize();
                }
            }
        }
        private Vector2 _windowSize;
        private Tab tab;


        /// <summary> Open the Resemble window. This window is used to generate one-shots or seed pool data. </summary>
        [MenuItem("Window/Audio/Resemble")]
        public static void Open()   
        {
            window = (Resemble_Window)EditorWindow.GetWindow(typeof(Resemble_Window));
            window.minSize = new Vector2(300, 400);
            window.titleContent = new GUIContent("Resemble");
            window.Show();
        }

        /// <summary> Open the Resemble window. This window is used to generate one-shots or seed pool data. </summary>
        public static void Open(Tab tab)
        {
            Open();
            window.tab = tab;
        }

        void OnGUI()
        {
            //Init components
            Styles.Load();
            if (drawer.text == null)
                drawer.text = text;
            windowSize = new Vector2(Screen.width, Screen.height);

            //Toolbar
            EditorGUI.BeginDisabledGroup(!Settings.haveProject);
            DrawToolbar();
            EditorGUI.EndDisabledGroup();

            switch (tab)
            {
                case Tab.OneShoot:
                    DrawOneShotGUI();
                    break;
                case Tab.Pool:
                    DrawPoolGUI();
                    break;
            }
            Repaint();
        }

        /// <summary> Called when the window is resized. </summary>
        private void OnResize()
        {
            if (drawer != null)
                drawer.Refresh();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);


            if (GUILayout.Toggle(tab == Tab.OneShoot, "OneShoot", EditorStyles.toolbarButton))
                tab = Tab.OneShoot;

            if (GUILayout.Toggle(tab == Tab.Pool, "Pending requests", EditorStyles.toolbarButton))
                tab = Tab.Pool;

            GUILayout.FlexibleSpace();

            /*
            //File button
            fileDropDown.DoLayout(new GUIContent("File"),
                new Dropdown.Item("Save wav file...", preview != null && preview.clip != null, SaveClipFile),
                new Dropdown.Item("Save as CharacterSet...", SaveAsCharacterPod)
            );

            //Edit button
            editDropDown.DoLayout(new GUIContent("Edit"),
                new Dropdown.Item("Generate", !string.IsNullOrEmpty(text.userString), Generate),
                new Dropdown.Item(""),
                new Dropdown.Item("Clear tags", ClearTags),
                new Dropdown.Item("Clear", Clear)
            );

            GUILayout.FlexibleSpace();
            */
            //Settings button
            settingsDropDown.DoLayout(new GUIContent(Styles.popupBtn),
                new Dropdown.Item("Copy clip body", CopyClipBody),
                new Dropdown.Item("Settings", Settings.OpenWindow),
                new Dropdown.Item(""),
                new Dropdown.Item("Help", () => { WebPage.PluginWindow.Open(); }),
                new Dropdown.Item("Resemble API", () => { WebPage.ResembleAPIDoc.Open(); })
            );

            GUILayout.Space(-6);
            GUILayout.EndHorizontal();
        }

        public enum Tab
        {
            OneShoot,
            Pool
        }

    }
}