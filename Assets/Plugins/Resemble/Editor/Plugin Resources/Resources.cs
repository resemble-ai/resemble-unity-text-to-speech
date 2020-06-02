using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public class Resources : ScriptableObject
    {
        public Texture2D icon;
        public Texture2D externalLink;
        public Texture2D breakIco;
        public Texture2D loadingTex;
        public Texture2D[] docImages;
        public Texture2D[] pathImages;

        public Material textMat
        {
            get
            {
                if (_textMat == null)
                    _textMat = new Material(textShader);
                return _textMat;
            }
        }
        public Material boxMat
        {
            get
            {
                if (_boxMat == null)
                    _boxMat = new Material(boxShader);
                return _boxMat;
            }
        }
        public Material loadingMat
        {
            get
            {
                if (_loadingMat == null)
                    _loadingMat = new Material(loadingShader);
                return _loadingMat;
            }
        }
        private Material _textMat;
        private Material _boxMat;
        private Material _loadingMat;

        public Shader textShader;
        public Shader boxShader;
        public Shader loadingShader;

        public Font font;
        public Object processClip;

        //One-shot stuff
        [HideInInspector] public Text oneShotText = new Text();
        [HideInInspector] public string oneShotVoiceName = "";
        [HideInInspector] public string oneShotVoiceUUID = "";
        [HideInInspector] public string oneShotPath = "";

        /// <summary> A pool of requests that will be saved and constantly executed until they are finished. </summary>
        public List<AsyncRequest> requests = new List<AsyncRequest>();

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
        public static bool instanceReferenced
        {
            get
            {
                return _instance != null;
            }
        }
        private static Resources _instance;
    }
}