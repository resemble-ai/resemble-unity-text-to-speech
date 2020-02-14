using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [CreateAssetMenu(fileName = "New CharacterSet", menuName = "Ressemble Character Set", order = 220), System.Serializable]
    public class CharacterSet : ScriptableObject
    {
        public string voice;
        public Tuning pitch = Tuning.Medium;
        public Tuning speed = Tuning.Medium;
        public List<Clip> pods = new List<Clip>();

        public enum Tuning
        {
            XLow,
            Low,
            Medium,
            High,
            XHigh
        }
    }
}