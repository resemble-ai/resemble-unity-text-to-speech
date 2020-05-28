using UnityEngine;

namespace Resemble
{
    public class Clip : ScriptableObject
    {
        //Main data
        public Speech speech;
        public AudioClip clip;
        public Text text;
        public PhonemeData phonemes;
        public string uuid;
        public string clipName;
        public bool havePhonemes
        {
            get
            {
                return speech.includePhonemes && phonemes != null && !phonemes.raw.isEmpty;
            }
        }

        //User custom data
        public string userdata;
        public Label[] labels;

        #region Label stuff

        /// <summary> Return true if the clip contains the label. </summary>
        public bool ContainsLabel(string label)
        {
            if (labels == null)
                return false;
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] == label)
                    return true;
            return false;
        }

        /// <summary> Return true if the clip contains the label with the same value. </summary>
        public bool ContainsLabel(Label label)
        {
            if (labels == null)
                return false;
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] == label)
                    return true;
            return false;
        }

        /// <summary> Return true if the clip contains the label. </summary>
        public bool ContainsLabel(string label, out int value)
        {
            value = 0;
            if (labels == null)
                return false;
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] == label)
                {
                    value = labels[i].value;
                    return true;
                }
            return false;
        }

        public int this[string index]
        {
            get
            {
                //Compare and return value
                int hash = index.GetHashCode();
                for (int i = 0; i < labels.Length; i++)
                {
                    if (labels[i].hash == hash)
                        return labels[i].value;
                }

                //Error
                throw new System.IndexOutOfRangeException(
                    string.Format("The clip does not contain the label corresponding to the name of {0}",
                    index));
            }
        }

        #endregion

        public void SetPhonemesRaw(PhonemesTimeStamps raw)
        {
            phonemes = new PhonemeData(raw, speech.phonemeTable);
        }

    }
}