using UnityEngine;

namespace Resemble.Structs
{
    [System.Serializable]
    public class Voice
    {
        public string name;
        public string language;
        public long id;
        public string uuid;

        public static Voice[] FromJson(string json)
        {
            return JsonUtility.FromJson<SerializableVoiceArray>(string.Format("{0}\"voices\": {2}{1}", '{', '}', json)).voices;
        }

        [System.Serializable]
        public struct SerializableVoiceArray
        {
            [SerializeField]
            public Voice[] voices;
        }
    }
}