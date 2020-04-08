using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Resemble;

    
public class APIGraph : PhonemeTable
{

    public List<PhoItem> items = new List<PhoItem>();

    [System.Serializable]
    public class PhoItem
    {
        public string phonemes;
        public bool vowel;
        public Vector2 vowelPos;
        public float consonantPos;
    }

    public override Phonemes RefineData(PhonemesRaw raw)
    {
        return null;

        //Curves
        //closeToOpen - frontToBack - labial - coronal - dorsal
    }
}
