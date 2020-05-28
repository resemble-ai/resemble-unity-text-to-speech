using UnityEngine;

namespace Resemble
{
    public class PhonemeTable : ScriptableObject, iPhonemeTable
    {

        public PhonemeGroup[] groups;

        public virtual Phonemes RefineData(PhonemesTimeStamps raw)
        {
            Debug.LogError("Refine not implemented");
            return null;
        }

        [System.Serializable]
        public class PhonemeGroup
        {
            public string name;
            public string phonemes;
        }
    }
}