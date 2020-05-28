using UnityEngine;

namespace Resemble.Structs
{
    [System.Serializable]
    public class ClipPatch
    {
        public Data data;
        public bool phoneme_timestamps;

        public ClipPatch(string title, string body, string voice, bool phonemes)
        {
            data = new Data(title, body, voice);
            phoneme_timestamps = phonemes;
        }

        public bool CompareContent(ResembleClip clip)
        {
            //Temp
            return clip.title == data.title && clip.body == data.body && clip.voice == data.voice && clip.phoneme_timestamps.isEmpty != phoneme_timestamps;
        }

        [System.Serializable]
        public class Data
        {
            public string title;
            public string body;
            public string voice;

            public Data(string title, string body, string voice)
            {
                this.title = title;
                this.body = body;
                this.voice = voice;
            }
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