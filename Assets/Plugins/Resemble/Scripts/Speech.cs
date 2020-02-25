using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [CreateAssetMenu(fileName = "New Speech", menuName = "Ressemble Speech", order = 220), System.Serializable]
    public class Speech : ScriptableObject
    {
        public string voice;
        public Tuning pitch = Tuning.Medium;
        public Tuning speed = Tuning.Medium;
        public List<Clip> clips = new List<Clip>();

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