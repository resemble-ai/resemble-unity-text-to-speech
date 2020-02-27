using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Resemble.Structs;
using Resemble.GUIEditor;

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
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    _instance = AssetDatabase.LoadAssetAtPath<Settings>(path);
                }
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

        /// <summary> Bind a Resemble project to this UnityProject. </summary>
        public static void BindProject(Project value)
        {
            project = value;
            projectUUID = value.uuid;
            haveProject = true;
            SetDirty();
        }

        /// <summary> Unbind the current Resemble project from this UnityProject. </summary>
        public static void UnbindProject()
        {
            project = null;
            projectUUID = "";
            haveProject = false;
            SetDirty();
        }

        /// <summary> Request a delete on a Resemble project. (Show a confirmation dialog before) </summary>
        public static void DeleteProject(Project value)
        {
            if (EditorUtility.DisplayDialog("Delete Resemble project",
            string.Format("Are you sure you want to delete the project \"{0}\" ?\n\nThis action will delete " +
            "the resemble project (not only in Unity) and is irreversible !", value.name), "Suppress it anyway", "Cancel"))
            {
                APIBridge.DeleteProject(value);
                projects = projects.ToList().Where(x => x.uuid != value.uuid).ToArray();
            }
        }

        /// <summary> Show a complexe dialog offering to import wav file or speech files. </summary>
        public static void ImportClips(Project value)
        {
            //Display complex dialog
            int choice = EditorUtility.DisplayDialogComplex("Import resemble clips", "In what format do you want to import the clips?",
                "Cancel", "Resemble Speechs", "Wav files only");

            string path = "";
            switch(choice)
            {
                //Cancel by user
                case 0:
                    return;

                   //Import speechs
                case 1:
                    path = EditorUtility.SaveFolderPanel("Import Resemble Speechs", "Assets/", "");
                    if (string.IsNullOrEmpty(path))
                        return;
                    if (!Utils.LocalPath(ref path))
                    {
                        EditorUtility.DisplayDialog("Error", "Path need be in the project unity.", "Ok");
                        return;
                    }
                    ImportProjectSpeechs(path);
                    break;

                    //Import wav files
                case 2:
                    path = EditorUtility.SaveFolderPanel("Import Resemble wav files", "Assets/", "");
                    if (string.IsNullOrEmpty(path))
                        return;
                    ImportProjectWavfiles(path);
                    break;
            }
        }

        /// <summary> Import all speechs from current project. </summary>
        public static void ImportProjectSpeechs(string path)
        {
            APIBridge.GetClips((ResembleClip[] result, Error error) => 
            {
                if (error)
                    error.Log();
                else
                {
                    DownloadSpeechs(path, result);
                }
            });
        }

        private static void DownloadSpeechs(string path, ResembleClip[] clips)
        {
            Dictionary<string, Speech> speechs = new Dictionary<string, Speech>();

            //Group speech by voices
            for (int i = 0; i < clips.Length; i++)
            {
                ResembleClip clip = clips[i];

                //Add new speech to the dictionnary
                Speech speech;
                if (!speechs.ContainsKey(clip.voice))
                {
                    speech = ScriptableObject.CreateInstance<Speech>();
                    speech.voice = clip.voice;
                    speech.name = clip.voice;
                    speechs.Add(clip.voice, speech);
                }

                //Get existing speech
                else
                {
                    speech = speechs[clip.voice];
                }

                Clip clipAsset = ScriptableObject.CreateInstance<Clip>();
                clipAsset.name = clip.title;
                clipAsset.text = new Text();
                clipAsset.text.userString = clip.body;
                clipAsset.text.tags = new List<Tag>();
                clipAsset.speech = speech;
                speech.clips.Add(clipAsset);
            }

            //Write assets
            foreach (var speech in speechs)
            {
                AssetDatabase.CreateAsset(speech.Value, path + "/" + speech.Value.name + ".asset");
                foreach (var subClip in speech.Value.clips)
                    AssetDatabase.AddObjectToAsset(subClip, speech.Value);
            }

            //Import assets
            foreach (var speech in speechs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(speech.Value), ImportAssetOptions.ForceUpdate);
        }

        /// <summary> Import all wav files from current project. </summary>
        public static void ImportProjectWavfiles(string path)
        {
            APIBridge.GetClips((ResembleClip[] result, Error error) =>
            {
                if (error)
                    error.Log();
                else
                {
                    ImportPopup.ImpAsset[] assets = result.Select(x => new ImportPopup.ImpAsset() { content = new GUIContent(x.title + ".wav"), import = true, obj = x }).ToArray();
                    ImportPopup.Show(assets, path, (ImportPopup.ImpAsset[] selecteds) => { DownloadWavFiles(path, selecteds.Select(x => x.obj as ResembleClip).ToArray()); });
                }
            });
        }

        private static void DownloadWavFiles(string path, ResembleClip[] clips)
        {
            Resemble_Window.Open(Resemble_Window.Tab.Pool);
            bool localPath = path.Contains(Application.dataPath);
            for (int i = 0; i < clips.Length; i++)
            {
                string filePath = path + "/" + clips[i].title + ".wav";
                Task task = APIBridge.DownloadClip(clips[i].link, (byte[] data, Error error) =>
                {
                    System.IO.File.WriteAllBytes(filePath, data);
                    if (localPath)
                        AssetDatabase.ImportAsset(Utils.LocalPath(filePath), ImportAssetOptions.ForceUpdate);
                });
                Pool.AddTask(task);
            }
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