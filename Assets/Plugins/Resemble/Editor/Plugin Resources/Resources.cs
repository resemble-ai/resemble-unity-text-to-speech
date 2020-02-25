using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public class Resources : ScriptableObject
    {
        public Texture2D icon;
        public Texture2D projectHeader;
        public Texture2D externalLink;
        public Texture2D breakIco;
        public Texture2D[] docImages;
        public Texture2D[] pathImages;
        public Material textMat;
        public Font font;
        public Text text = new Text();
        public Object processClip;

        public static string path
        {
            get
            {
                if (string.IsNullOrEmpty(_path))
                {
                    Resources settings = ScriptableObject.CreateInstance<Resources>();
                    _path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settings));
                    ScriptableObject.DestroyImmediate(settings);
                    _path = _path.Replace(".cs", ".asset");
                }
                return _path;
            }
        }
        private static string _path;

        public static Resources instance
        {
            get
            {
                if (_instance == null)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    _instance = AssetDatabase.LoadAssetAtPath<Resources>(path);
                }
                return _instance;
            }
        }
        private static Resources _instance;
    }
}