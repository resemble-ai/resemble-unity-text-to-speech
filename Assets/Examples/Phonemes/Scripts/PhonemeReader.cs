using System.Collections.Generic;
using UnityEngine;
using Resemble;

public class PhonemeReader : MonoBehaviour
{
    public Phonemes data;
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
        if (data == null)
            throw new System.NullReferenceException("The data field is null on the Phoneme Reader.");

        return data.Evaluate(audio.time / audio.clip.length);
    }
}
