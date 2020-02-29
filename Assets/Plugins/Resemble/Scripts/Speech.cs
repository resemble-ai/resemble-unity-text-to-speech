using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [CreateAssetMenu(fileName = "New Speech", menuName = "Resemble Speech", order = 220), System.Serializable]
    public class Speech : ScriptableObject
    {
        public string voiceName;
        public string voiceUUID;
        public List<Clip> clips = new List<Clip>();
    }
}