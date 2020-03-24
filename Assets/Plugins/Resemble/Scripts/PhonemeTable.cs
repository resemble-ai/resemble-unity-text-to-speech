using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class PhonemeTable : ScriptableObject, iPhonemeTable
    {

        public PhonemeGroup[] groups;

        public Phonemes RefineData(PhonemesRaw raw)
        {
            throw new System.NotImplementedException();
        }

        [System.Serializable]
        public class PhonemeGroup
        {
            public string name;
            public string phonemes;
        }
    }
}