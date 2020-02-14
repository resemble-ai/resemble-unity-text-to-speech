using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Resemble
{
    public class Settings : ScriptableObject
    {
        //Saved settings - Saved
        [SerializeField] public string _token;
        [SerializeField] public string _projectUUID;
        [SerializeField] public PathMethode pathMethode = PathMethode.SamePlace;
        [SerializeField] public bool useSubFolder = true;
        [SerializeField] public bool showWelcomePopup = true;
        [SerializeField] public string folderPathA = "";
        [SerializeField] public string folderPathB = "";


        [HideInInspector] public Project[] _projects;
        [HideInInspector] public Project _project;
        [HideInInspector] public Dictionary<string, Project> _projectNames = new Dictionary<string, Project>();

        //Editor only - Not saved
        private static string _path;

        /// <summary> Indicate if the project is connected to the API. </summary>
        public static bool connected;

        /// <summary> Return true if there is a connect Resemble project. </summary>
        public static bool haveProject;

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

                    //Rebuild projectNames dictionnary - Avoid same name project
                    instance._projectNames.Clear();
                    instance._projectNames.Add("None", null);
                    if (instance._projects != null)
                    {
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
                EditorUtility.SetDirty(instance);
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
                    Settings settings = ScriptableObject.CreateInstance<Settings>();
                    _path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settings));
                    ScriptableObject.DestroyImmediate(settings);
                    _path = _path.Replace(".cs", ".asset");
                }
                return _path;
            }
        }

        //Instance ref
        public static Settings instance
        {
            get
            {
                if (_instance == null)
                    _instance = AssetDatabase.LoadAssetAtPath<Settings>(path);
                return _instance;
            }
        }
        private static Settings _instance;
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(instance);
        }

        //Functions
        public static void SelectProjectByName(string name)
        {
            project = instance._projectNames[name];
            projectUUID = project.uuid;
            EditorUtility.SetDirty(instance);
        }

        /// <summary> Open the preference page about Resemble in the Editor. </summary>
        public static void OpenWindow()
        {
            SettingsService.OpenUserPreferences("Preferences/Resemble");
        }

        /// <summary> Set settings asset dirty. Call this after any changes on the settings.  </summary>
        public new static void SetDirty()
        {
            EditorUtility.SetDirty(instance);
        }

        public enum PathMethode
        {
            Absolute,
            SamePlace,
            MirrorHierarchy
        }
    }
}