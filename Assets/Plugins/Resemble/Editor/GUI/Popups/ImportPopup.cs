using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble.GUIEditor
{
    public class ImportPopup : EditorWindow
    {

        private static ImportPopup window;
        private static ImpAsset[] assets;
        private static Vector2 scroll;
        private static ValidateCallback callback;
        private static AllCheckState allCheck;
        private static string path;
        public delegate void ValidateCallback(ImpAsset[] assets);

        public class ImpAsset
        {
            public object obj;
            public GUIContent content;
            public bool import;
            public ImpAsset[] childs;
        }


        public static void Show(ImpAsset[] assets, string path, ValidateCallback callback)
        {
            ImportPopup.path = Utils.LocalPath(path);
            ImportPopup.assets = assets;
            ImportPopup.callback = callback;
            RefreshAllCheck();
            window = EditorWindow.CreateInstance<ImportPopup>();
            window.ShowModalUtility();
            window.titleContent = new GUIContent("Import");
            window.minSize = new Vector2(200, 300);
        }

        private void OnGUI()
        {
            //Avoid corrupted window after a script reload
            if (assets == null)
            {
                Close();
                return;
            }

            //Draw all content
            DrawHeader();
            DrawContent();
            DrawFooter();
        }

        private void DrawHeader()
        {
            GUILayout.Label("Target : " + path);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //Global toggle
            bool temp = GUILayout.Toggle(allCheck == AllCheckState.Check, "All");

            //Partial check knob
            if (allCheck == AllCheckState.Partial)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.Set(rect.x + 4, rect.y + 4, rect.height - 7, rect.height - 7);
                EditorGUI.DrawRect(rect, Color.black * 0.8f);
            }

            //Change global check
            if (temp != (allCheck == AllCheckState.Check))
                SetFullCheck(temp);

            GUILayout.FlexibleSpace();


            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < assets.Length; i++)
                DrawAsset(assets[i]);

            GUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 1);
            EditorGUI.DrawRect(rect, Color.black * 0.8f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            if (GUILayout.Button("Import"))
            {
                callback.Invoke(assets);
                Close();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary> Draw an asset and its children. </summary>
        private void DrawAsset(ImpAsset asset)
        {
            //Toggle
            bool temp = GUILayout.Toggle(asset.import, asset.content);
            if (temp != asset.import)
            {
                asset.import = temp;
                RefreshAllCheck();
            }

            //Draw childs
            if (asset.childs != null)
            {
                for (int j = 0; j < asset.childs.Length; j++)
                    DrawAsset(asset.childs[j]);
            }
        }

        /// <summary> Apply check value on all assets. </summary>
        public static void SetFullCheck(bool value)
        {
            for (int i = 0; i < assets.Length; i++)
                SetFullCheck(assets[i], value);
            allCheck = value ? AllCheckState.Check : AllCheckState.Uncheck;
        }

        /// <summary> Apply check value on an asset and its childs.  </summary>
        private static void SetFullCheck(ImpAsset asset, bool value)
        {
            asset.import = value;
            if (asset.childs != null)
            {
                for (int i = 0; i < asset.childs.Length; i++)
                    SetFullCheck(asset.childs[i], value);
            }
        }

        /// <summary> Check if all assets are selected for the import. </summary>
        public static void RefreshAllCheck()
        {
            allCheck = IsFullCheck(assets[0]);
            if (allCheck == AllCheckState.Partial)
                return;
            for (int i = 1; i < assets.Length; i++)
            {
                allCheck = Blend(allCheck, IsFullCheck(assets[i]));
                if (allCheck == AllCheckState.Partial)
                    return;
            }
        }

        /// <summary> Check if the asset and its childs are selected for the import. </summary>
        private static AllCheckState IsFullCheck(ImpAsset asset)
        {
            AllCheckState state = asset.import ? AllCheckState.Check : AllCheckState.Uncheck;
            if (state == AllCheckState.Partial)
                return AllCheckState.Partial;
            if (asset.childs != null)
            {
                for (int j = 0; j < asset.childs.Length; j++)
                {
                    state = Blend(state, IsFullCheck(asset.childs[j]));
                    if (state == AllCheckState.Partial)
                        return AllCheckState.Partial;
                }
            }
            return state;
        }

        private enum AllCheckState
        {
            Uncheck,
            Check,
            Partial
        }

        private static AllCheckState Blend(AllCheckState a, AllCheckState b)
        {
            if (a != b)
                return AllCheckState.Partial;
            if (a == AllCheckState.Partial || b == AllCheckState.Partial)
                return AllCheckState.Partial;
            if (a == AllCheckState.Check && b == AllCheckState.Check)
                return AllCheckState.Check;
            return AllCheckState.Uncheck;
        }

    }
}