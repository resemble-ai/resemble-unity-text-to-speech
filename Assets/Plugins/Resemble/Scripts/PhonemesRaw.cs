using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public struct PhonemesRaw
    {
        public string phonemes;
        public char[] phonemesChars;
        public float[] end_times;

        public bool isEmpty
        {
            get
            {
                return (phonemesChars == null || phonemesChars.Length == 0) && (end_times == null || end_times.Length == 0);
            }
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary> Construct the struct from json data. </summary>
        public static PhonemesRaw FromJson(string json)
        {
            PhonemesRaw raw = JsonUtility.FromJson<PhonemesRaw>(json);
            raw.phonemesChars = raw.phonemes.ToCharArray();
            return raw;
        }
    }
}