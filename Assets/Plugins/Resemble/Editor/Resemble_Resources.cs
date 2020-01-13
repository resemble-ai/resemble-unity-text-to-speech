using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Resemble_Resources : ScriptableObject
{
    public GUISkin resembleButton;
    public Texture2D icon;
    public Material textMat;
    public Font font;

    public static string path
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
            {
                Resemble_Resources settings = ScriptableObject.CreateInstance<Resemble_Resources>();
                _path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settings));
                ScriptableObject.DestroyImmediate(settings);
                _path = _path.Replace(".cs", ".asset");
            }
            return _path;
        }
    }
    private static string _path;

    public static Resemble_Resources instance
    {
        get
        {
            if (_instance == null)
                _instance = AssetDatabase.LoadAssetAtPath<Resemble_Resources>(path);
            return _instance;
        }
    }
    private static Resemble_Resources _instance;
}
