using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public struct PhonemesTimeStamps
    {
        public string phonemes;
        public char[] phonemesChars;
        public float[] end_times;

        public bool isEmpty
        {
            get
            {
                return string.IsNullOrEmpty(phonemes);
            }
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void ReadData()
        {
            phonemesChars = phonemes.ToCharArray();
        }
    }
}