using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Resemble_Settings : ScriptableObject
{
    //Saved settings - Saved
    [SerializeField] public string _token;
    [SerializeField] public Project[] _projects;
    [SerializeField] public Project _project;
    [SerializeField] public string _projectUUID;
    [SerializeField] public Dictionary<string, Project> _projectNames = new Dictionary<string, Project>();

    //Editor only - Not saved
    private static string _path;

    //Static acces
    public static IEnumerable<string> projectNames
    {
        get
        {
            return instance._projectNames.Keys;
        }
    }
    public static string projectUUID
    {
        get
        {
            return instance._projectUUID;
        }
        set
        {
            if (instance._projectUUID != value)
            {
                instance._projectUUID = value;
                EditorUtility.SetDirty(instance);
            }
        }
    }
    public static Project[] projects
    {
        get
        {
            return instance._projects;
        }
        set
        {
            if (instance._projects != value)
            {
                instance._projects = value;

                //Rebuild projectNames sortedset - Avoid same name project
                instance._projectNames.Clear();
                instance._projectNames.Add("None", null);
                for (int i = 0; i < instance._projects.Length; i++)
                {
                    string name = instance._projects[i].name;

                    if (!instance._projectNames.ContainsKey(name))
                        instance._projectNames.Add(name, instance._projects[i]);
                    else
                    {
                        int redundancy = 1;
                        while (instance._projectNames.ContainsKey(name + " " + redundancy))
                            redundancy++;
                        instance._projectNames.Add(name + " " + redundancy, instance._projects[i]);
                    }
                }

                EditorUtility.SetDirty(instance);
            }
        }
    }
    public static Project project
    {
        get
        {
            return instance._project;
        }
        set
        {
            instance._project = value;
        }
    }
    public static string token
    {
        get
        {
            return instance._token;
        }
        set
        {
            if (instance._token != value)
            {
                instance._token = value;
                EditorUtility.SetDirty(instance);
            }
        }
    }
    public static string path
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
            {
                Resemble_Settings settings = ScriptableObject.CreateInstance<Resemble_Settings>();
                _path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settings));
                ScriptableObject.DestroyImmediate(settings);
                _path = _path.Replace(".cs", ".asset");
            }
            return _path;
        }
    }

    //Instance ref
    public static Resemble_Settings instance
    {
        get
        {
            if (_instance == null)
                _instance = AssetDatabase.LoadAssetAtPath<Resemble_Settings>(path);
            return _instance;
        }
    }
    private static Resemble_Settings _instance;
    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(instance);
    }

    //Functions
    public static void SelectProjectByName(string name)
    {
        instance._project = instance._projectNames[name];
        instance._projectUUID = instance._project.uuid;
        EditorUtility.SetDirty(instance);
    }

}
