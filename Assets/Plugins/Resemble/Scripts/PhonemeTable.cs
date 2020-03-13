using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class PhonemeTable : ScriptableObject
    {

        public PhonemeGroup[] groups;

        [System.Serializable]
        public class PhonemeGroup
        {
            public string name;
            public string phonemes;
        }
    }
}