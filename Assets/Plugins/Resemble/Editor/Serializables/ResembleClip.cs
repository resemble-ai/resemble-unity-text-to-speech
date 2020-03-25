using UnityEngine;

namespace Resemble.Structs
{
    [System.Serializable]
    public class ResembleClip
    {
        public int id;
        public int project_id;
        public string title;
        public string body;
        public string published;
        public string author;
        public string created_at;
        public string updated_at;
        public bool finished;
        public string link;
        public string uuid;
        public string voice;
        public bool @public;
        public string html;
        public bool archive;
        public string emotion;
        public string team_id;
        public string callback_uri;
        public bool phoneme_timestamps;

        public override string ToString()
        {
            return string.Format("(Title: {0}, Body: {1}, Finished: {2})", title, body, finished);
        }

        public static ResembleClip[] FromJson(string json)
        {
            return JsonUtility.FromJson<SerializablePodArray>(json).pods;
        }

        [System.Serializable]
        public struct SerializablePodArray
        {
            [SerializeField]
            public ResembleClip[] pods;
        }
    }
}