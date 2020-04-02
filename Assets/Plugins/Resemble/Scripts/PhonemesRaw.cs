using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public struct PhonemesRaw
    {
        public string[] phonemes;
        public float[] end_times;

        public bool isEmpty
        {
            get
            {
                return (phonemes == null || phonemes.Length == 0) && (end_times == null || end_times.Length == 0);
            }
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary> Construct the struct from json data. </summary>
        public static PhonemesRaw FromJson(string json)
        {
            return JsonUtility.FromJson<PhonemesRaw>(json);
        }
    }
}