using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [CreateAssetMenu(fileName = "New Speech", menuName = "Resemble Speech", order = 220), System.Serializable]
    public class Speech : ScriptableObject
    {

        public bool includePhonemes;
        public PhonemeTable phonemeTable;
        public string voiceName;
        public string voiceUUID;
        public List<Clip> clips = new List<Clip>();

        /// <summary> Returns the clip with the given name. </summary>
        public Clip GetClip(string name)
        {
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].clipName == name)
                    return clips[i];
            }
            throw new System.NullReferenceException(string.Format("Clip {0} can't be find int Speech {1}.", name, this.name));
        }

        /// <summary> Returns the audio clip attached to the clip with the given name. </summary>
        public AudioClip GetAudio(string name)
        {
            return GetClip(name).clip;
        }

        /// <summary> Returns the userdata text attached to the clip with the given name. </summary>
        public string GetUserData(string name)
        {
            return GetClip(name).userdata;
        }

        /// <summary> Returns all clips containing the given tag. </summary>
        public List<Clip> GetClipsWithLabel(string label)
        {
            Label l = new Label(label, 0);
            List<Clip> result = new List<Clip>();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].ContainsLabel(l))
                    result.Add(clips[i]);
            }
            return result;
        }

        /// <summary> Returns all clips containing the given tag at the given value. </summary>
        public List<Clip> GetClipsWithLabel(string label, int value)
        {
            List<Clip> result = new List<Clip>();
            for (int i = 0; i < clips.Count; i++)
            {
                int v;
                if (clips[i].ContainsLabel(label, out v) && v == value)
                    result.Add(clips[i]);
            }
            return result;
        }

        /// <summary> Returns all clips containing the given tag at the given value. </summary>
        public List<Clip> GetClipsWithLabel(Label label)
        {
            List<Clip> result = new List<Clip>();
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].ContainsLabel(label))
                    result.Add(clips[i]);
            }
            return result;
        }
    }
}
