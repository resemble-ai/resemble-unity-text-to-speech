using Resemble.Structs;

namespace Resemble
{
    /// <summary> Contains various callbacks to simplify the usage of functions to the API. </summary>
    public static class Callback
    {
        //Callbacks for each functions type
        public delegate void GetProject(Project[] projects, Structs.Error error);
        public delegate void GetClip(ResembleClip clip, Structs.Error error);
        public delegate void GetClips(ResembleClip[] clips, Structs.Error error);
        public delegate void GetVoices(Voice[] voices, Structs.Error error);
        public delegate void CreateProject(ProjectStatus status, Structs.Error error);
        public delegate void CreateClip(ClipStatus status, Structs.Error error);
        public delegate void Error(long errorCode, string errorMessage);
        public delegate void Simple(string content, Structs.Error error);
        public delegate void Download(byte[] data, Structs.Error error);
    }
}