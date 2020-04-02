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

        //Phonemes
        #pragma warning disable 0649
        [SerializeField] private string phoneme_timestamps;
        public PhonemesRaw phonemesRaw;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
        
        public static ResembleClip FromJson(string json)
        {
            ResembleClip clip = JsonUtility.FromJson<ResembleClip>(json);
            clip.phonemesRaw = JsonUtility.FromJson<PhonemesRaw>(clip.phoneme_timestamps);
            return clip;
        }

        public static ResembleClip[] ArrayFromJson(string json)
        {
            ResembleClip[] clips = JsonUtility.FromJson<SerializablePodArray>(json).clips;
            for (int i = 0; i < clips.Length; i++)
                clips[i].phonemesRaw = JsonUtility.FromJson<PhonemesRaw>(clips[i].phoneme_timestamps);
            return clips;
        }

        [System.Serializable]
        public struct SerializablePodArray
        {
            [SerializeField]
            public ResembleClip[] clips;
        }
    }
}