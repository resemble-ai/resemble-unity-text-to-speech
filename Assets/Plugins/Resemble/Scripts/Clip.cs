using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class Clip : ScriptableObject
    {
        public Speech set;
        public AudioClip clip;
        public AudioClip clipCopy;
        public Text text;
        public bool autoRename;
    }
}