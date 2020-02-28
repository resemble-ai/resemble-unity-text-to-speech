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

        #region Saved settings

        //Saved settings - Saved between session - Acces via static getter only
        [SerializeField] private string _token;
        [SerializeField] private bool _connected;
        [SerializeField] private string _projectUUID;
        [SerializeField] private PathMethode _pathMethode = PathMethode.SamePlace;
        [SerializeField] private bool _useSubFolder = true;
        [SerializeField] private bool _showWelcomePopup = true;
        [SerializeField] private string _folderPathA = "";
        [SerializeField] private string _folderPathB = "";


        //Acces to the saved settings - Make this object dirty when change
        /// <summary> User token, Used to authenticate to the Resemble API.  </summary>
        public static string token
        {
            get { return instance._token; }
            set { if (instance._token != value) instance._token = value; SetDirty(); }
        }

        /// <summary> Indicate if the user is connected. </summary>
        public static bool connected
        {
            get { return instance._connected; }
            set { if (instance._connected != value) instance._connected = value; SetDirty(); }
        }

        /// <summary> UUID of the current binded project. </summary>
        public static string projectUUID
        {
            get { return instance._projectUUID; }
        }

        /// <summary> Indicates the method used to generate the path for saving new audio files. </summary>
        public static PathMethode pathMethode
        {
            get { return instance._pathMethode; }
            set { if (instance._pathMethode != value) instance._pathMethode = value; SetDirty(); }
        }

        /// <summary> Indicates if the method used to generate the path for saving new audio files need a sub folder. </summary>
        public static bool useSubFolder
        {
            get { return instance._useSubFolder; }
            set { if (instance._useSubFolder != value) instance._useSubFolder = value; SetDirty(); }
        }

        /// <summary> Indicate if the Welcome panel need the be show on the first opening of the plugin. </summary>
        public static bool showWelcomePopup
        {
            get { return instance._showWelcomePopup; }
            set { if (instance._showWelcomePopup != value) instance._showWelcomePopup = value; SetDirty(); }
        }

        /// <summary> Used to generate the save path of new audio files. </summary>
        public static string folderPathA
        {
            get { return instance._folderPathA; }
            set { if (instance._folderPathA != value) instance._folderPathA = value; SetDirty(); }
        }

        /// <summary> Used to generate the save path of new audio files. </summary>
        public static string folderPathB
        {
            get { return instance._folderPathB; }
            set { if (instance._folderPathB != value) instance._folderPathB = value; SetDirty(); }
        }

        /// <summary> Return true if the plugin is bind to a Resemble project. </summary>
        public static bool haveProject
        {
            get { return !string.IsNullOrEmpty(instance._projectUUID); }
        }

        #endregion

        #region Acces to the saved data
        private static Settings _instance;
        /// <summary> Returns the path of the scriptable object that holds all the information related to the user's connection.
        public static Settings instance
        {
            get
            {
                if (_instance == null)
                {
                    //Build path - This particular method allows to move the location of the plugin without any problem.
                    Settings settings = CreateInstance<Settings>();
                    string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(settings));
                    DestroyImmediate(settings);
                    path = path.Replace(".cs", ".asset");

                    //Force unity to import asset (Required for the first load of the plugin)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                    //Load scriptable object
                    _instance = AssetDatabase.LoadAssetAtPath<Settings>(path);
                }
                return _instance;
            }
        }
        #endregion

        //Cached data - Not save between session - Often refreshed
        //Accessible only via the getter that will perform a refresh if needed.
        private Voice[] _voices;
        private Project[] _projects;
        private Project _project;
        private Dictionary<string, Project> _projectNames = new Dictionary<string, Project>();

        /// <summary> Voices that the user has access to. return empty array if null. 
        /// You can call RefreshVoices() to fetch the value from API </summary>
        public static Voice[] voices
        {
            get
            {
                if (instance._voices == null)
                    return new Voice[0];
                return instance._voices;
            }
        }

        /// <summary> Enable when th eplugin try to connect to the API. </summary>
        public static bool tryToConnect { private set; get; }

        /// <summary> Exit after a connection failure. </summary>
        public static Error connectionError { private set; get; } = Error.None;

        //Static acces

        public static IEnumerable<string> projectNames
        {
            get
            {
                return instance._projectNames.Keys;
            }
        }

        public static Project[] projects
        {
            get
            {
                return instance._projects;
            }
            private set
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

                    //Refresh binded project
                    if (value != null && haveProject)
                    {
                        bool find = false;
                        for (int i = 0; i < projects.Length; i++)
                        {
                            //Project UUID find - Refresh with new values from the list 
                            if (projects[i].uuid == projectUUID)
                            {
                                project = projects[i];
                                find = true;
                                break;
                            }
                        }

                        //Current project UUID not find in the list - Unbind project
                        if (!find)
                            UnbindProject();
                    }
                    SetDirty();
                }
            }
        }

        public static Project project
        {
            get
            {
                return instance._project;
            }
            private set
            {
                instance._project = value;
                instance._projectUUID = value == null ? null : value.uuid;
                SetDirty();
            }
        }

        //Instance ref

        #region  Callbacks
        //Callbacks to get updates of any changes
        public delegate void OnSettingsChange();
        public static OnSettingsChange OnBind;
        public static OnSettingsChange OnUnbind;
        public static OnSettingsChange OnRefreshProjects;
        public static OnSettingsChange OnRefreshVoices;
        #endregion

        //Functions

        /// <summary> Connect with the current token. </summary>
        public static void Connect()
        {
            //Check token
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("You must enter a valid token to connect to the Resemble API.");
                return;
            }

            //Indicates that the plugin is trying to make the connection
            tryToConnect = true;

            //If the projects are returned, the connection is considered established.
            RefreshProjects();
        }

        /// <summary> Disconnect from the current token. </summary>
        public static void Disconnect()
        {
            UnbindProject();
            connected = false;
        }

        /// <summary> Make the request to refresh the list of projects. </summary>
        public static void RefreshProjects()
        {
            //Clean old error if there are any. 
            connectionError = Error.None;

            //Make the request
            APIBridge.GetProjects((Project[] projects, Error error) =>
            {
                tryToConnect = false;
                if (error)
                {
                    connected = false;
                    connectionError = error;
                }
                else
                {
                    connected = true;
                    Settings.projects = projects;

                    if (OnRefreshProjects != null)
                        OnRefreshProjects.Invoke();
                }
            });
        }

        /// <summary> Make the request to refresh the list of voices. </summary>
        public static void RefreshVoices()
        {
            //Clean old error if there are any. 
            connectionError = Error.None;

            //Check connection
            if (!connected)
            {
                Debug.LogError("You needs to be connected to the Resemble API before loading the voices list.");
                return;
            }

            //Make the request
            APIBridge.GetVoices((Voice[] voices, Error error) =>
            {
                if (error)
                {
                    connectionError = error;
                }
                else
                {
                    instance._voices = voices;
                    if (OnRefreshVoices != null)
                        OnRefreshVoices.Invoke();
                }
            });
        }

        /// <summary> Bind a Resemble project to this UnityProject. </summary>
        public static void BindProject(Project value)
        {
            project = value;
            if (OnBind != null)
                OnBind.Invoke();
        }

        /// <summary> Unbind the current Resemble project from this UnityProject. </summary>
        public static void UnbindProject()
        {
            project = null;
            if (OnUnbind != null)
                OnUnbind.Invoke();
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

        /// <summary> Show a save dialog then the import panel to select wav to import. </summary>
        public static void ImportClips(Project value)
        {
            string path = EditorUtility.SaveFolderPanel("Import Resemble wav files", "Assets/", "");
            if (string.IsNullOrEmpty(path))
                return;
            ImportProjectWavfiles(path);
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

        /// <summary> Download and save at path clips in wav format. </summary>
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