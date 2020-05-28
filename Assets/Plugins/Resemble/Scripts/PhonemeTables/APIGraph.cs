using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Resemble;

    
public class APIGraph : PhonemeTable
{

    public List<Vowels> vowels = new List<Vowels>();
    public List<Consonants> consonants = new List<Consonants>();

    [System.Serializable]
    public class Vowels
    {
        public string characters = "";
        public Vector2 position;
    }

    [System.Serializable]
    public class Consonants
    {
        public string characters = "";
        public float position;
    }

    public override Phonemes RefineData(PhonemesTimeStamps raw)
    {
        return null;

        //Curves
        //closeToOpen - frontToBack - labial - coronal - dorsal
    }
}
