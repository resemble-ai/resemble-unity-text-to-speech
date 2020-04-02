using System.Collections.Generic;
using UnityEngine;
using Resemble;

public class PhonemeReader : MonoBehaviour
{
    public Clip clip;
    public new AudioSource audio;

    public bool isPlaying
    {
        get
        {
            return audio != null && audio.isPlaying;
        }
    }

    /// <summary> Get values at current audioTime </summary>
    public virtual KeyValuePair<string, float>[] GetValues()
    {
        if (clip == null)
            throw new System.NullReferenceException("The data field is null on the Phoneme Reader.");

        return clip.phonemes.refined.Evaluate(audio.time / audio.clip.length);
    }
}
