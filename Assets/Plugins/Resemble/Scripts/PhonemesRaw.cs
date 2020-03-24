using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public struct PhonemesRaw
    {
        public char[] phonemes;
        public float[] end_times;

        /// <summary> Construct the struct from json data. </summary>
        public static PhonemesRaw FromJson(string json)
        {
            return JsonUtility.FromJson<PhonemesRaw>(json);
        }
    }
}