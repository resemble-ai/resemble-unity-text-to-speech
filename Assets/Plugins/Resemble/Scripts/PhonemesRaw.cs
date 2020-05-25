using Boo.Lang;
using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public struct PhonemesRaw
    {
        public PhonemeSentence[] phonemes;
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
            //Format json for unity
            json = "{\"phonemes\":" + json + "}";

            PhonemesRaw raw = JsonUtility.FromJson<PhonemesRaw>(json);

            //Read data from sentences
            string phonemes = "";
            List<float> end_times = new List<float>();
            float lastEndTime = 0.0f;
            for (int i = 0; i < raw.phonemes.Length; i++)
            {
                phonemes += raw.phonemes[i].phonemes;
                for (int j = 0; j < raw.phonemes[i].end_times.Length; j++)
                {
                    end_times.Add(raw.phonemes[i].end_times[j] + lastEndTime);
                    if (j == raw.phonemes[i].end_times.Length - 1)
                        lastEndTime += raw.phonemes[i].end_times[j];
                }
            }
            raw.phonemesChars = phonemes.ToCharArray();
            raw.end_times = end_times.ToArray();

            return raw;
        }
    }

    [System.Serializable]
    public struct PhonemeSentence
    {
        public string phonemes;
        public float[] end_times;
    }

}