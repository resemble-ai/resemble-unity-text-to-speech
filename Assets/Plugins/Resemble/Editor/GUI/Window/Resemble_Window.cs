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
        private Tab tab
        {
            get
            {
                return _tab;
            }
            set
            {
                if (_tab != value)
                {
                    _tab = value;
                    switch (value)
                    {
                        case Tab.OneShot: EnableOneShot(); break;
                        case Tab.Pool: EnablePool(); break;
                    }
                }
            }
        }
        private Tab _tab;


        /// <summary> Open the Resemble window. This window is used to generate one-shots or seed pool data. </summary>
        [MenuItem("Window/Audio/Resemble")]
        public static void Open()   
        {
            window = (Resemble_Window)EditorWindow.GetWindow(typeof(Resemble_Window));
            window.minSize = new Vector2(390, 460);
            window.titleContent = new GUIContent("Resemble");
            window.Show();
        }

        /// <summary> Open the Resemble window. This window is used to generate one-shots or seed pool data. </summary>
        public static void Open(Tab tab)
        {
            Open();
            window.tab = (Tab)(-1);
            window.tab = tab;
            window.Repaint();
        }

        private void OnEnable()
        {
            //Refresh text area with delay
            int refreshDelay = 0;
            void RefreshText()
            {
                text.CallOnEdit();
                refreshDelay++;
                if (refreshDelay > 2)
                    EditorApplication.update -= RefreshText;
            }
            EditorApplication.update += RefreshText;
        }

        void OnGUI()
        {
            //Init components
            Styles.Load();
            if (drawer == null)
                drawer = new Text_Editor(text, OnEditCallback, OnRepaintCallback);
            windowSize = new Vector2(Screen.width, Screen.height);

            //Toolbar
            EditorGUI.BeginDisabledGroup(!Settings.haveProject);
            DrawToolbar();
            EditorGUI.EndDisabledGroup();

            switch (tab)
            {
                case Tab.OneShot:
                    DrawOneShotGUI();
                    break;
                case Tab.Pool:
                    DrawPoolGUI();
                    break;
            }
            Repaint();
        }

        public void OnEditCallback()
        {
            EditorUtility.SetDirty(Resources.instance);
        }

        public void OnRepaintCallback()
        {
            Repaint();
        }

        /// <summary> Called when the window is resized. </summary>
        private void OnResize()
        {
            if (drawer != null)
                drawer.Refresh();
        }

        /// <summary> Draw the top toolbar. </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //Tabs
            if (GUILayout.Toggle(tab == Tab.OneShot, "OneShot", EditorStyles.toolbarButton))
                tab = Tab.OneShot;
            if (GUILayout.Toggle(tab == Tab.Pool, "Pending requests", EditorStyles.toolbarButton))
                tab = Tab.Pool;

            GUILayout.FlexibleSpace();

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
            OneShot,
            Pool
        }

    }
}