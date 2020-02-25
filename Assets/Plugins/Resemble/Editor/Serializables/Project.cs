using UnityEngine;

namespace Resemble.Structs
{
    [System.Serializable]
    public class Project
    {
        public int id;
        public string uuid;
        public string name;
        public string description;
        public bool publish_google;

        public static Project[] FromJson(string json)
        {
            return JsonUtility.FromJson<SerializableProjectArray>(string.Format("{0}\"projects\": {2}{1}", '{', '}', json)).projects;
        }

        [System.Serializable]
        public struct SerializableProjectArray
        {
            [SerializeField]
            public Project[] projects;
        }
    }
}