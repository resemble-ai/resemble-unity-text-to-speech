using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public class ResemblePod
    {
        public int id;
        public string title;
        public string body;
        public string published;
        public string author;
        public bool finished;
        public string link;
        public string voice;
        public int project_id;

        public override string ToString()
        {
            return string.Format("(Title: {0}, Body: {1}, Finished: {2})", title, body, finished);
        }

        public static ResemblePod[] FromJson(string json)
        {
            return JsonUtility.FromJson<SerializablePodArray>(string.Format("{0}\"pods\": {2}{1}", '{', '}', json)).pods;
        }

        [System.Serializable]
        public struct SerializablePodArray
        {
            [SerializeField]
            public ResemblePod[] pods;
        }
    }

    [System.Serializable]
    public class PostPod
    {
        public string title;
        public string body;
        public string voice;
    }
}