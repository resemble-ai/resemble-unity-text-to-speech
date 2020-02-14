using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class Clip : ScriptableObject
    {
        public CharacterSet set;
        public AudioClip clip;
        public AudioClip clipCopy;
        public PodText text;
        public bool autoRename;
    }
}