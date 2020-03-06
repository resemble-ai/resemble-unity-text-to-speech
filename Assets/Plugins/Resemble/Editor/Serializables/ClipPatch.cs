using UnityEngine;

namespace Resemble.Structs
{
    [System.Serializable]
    public class ClipPatch
    {
        public Data data;

        public ClipPatch(string title, string body, string voice)
        {
            data = new Data() {title = title,  body = body, voice = voice};
        }

        [System.Serializable]
        public class Data
        {
            public string title;
            public string body;
            public string voice;
        }

        public override string ToString()
        {
            return ToJson();
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}