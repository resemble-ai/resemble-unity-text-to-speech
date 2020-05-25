using System.Collections.Generic;
using UnityEngine;
using Resemble;

public class PhonemeReader : MonoBehaviour
{
    [Tooltip("Resemble Clip whose audio and phonemes will be used for this reader.")]
    public Clip clip;
    [Tooltip("The audioSource that will be used as a reference to time phoneme reading.")]
    public new AudioSource audio;
    [Tooltip("Smoothing value applied to the data."), Range(0.0f, 1.0f)]
    public float smoothFactor = 0.2f;

    [HideInInspector]
    public float[] smoothValues;
    [HideInInspector]
    public bool sleeping;

    /// <summary>
    /// This function will be called every time a phoneme group is changed.
    /// </summary>
    /// <param name="label">The phoneme group' s label. These labels are given in the phoneme table.</param>
    /// <param name="instantValue">The value of the phoneme group at the precise moment of the clip.</param>
    /// <param name="smoothValue">The smoothed value  of the phoneme group.</param>
    protected virtual void OnReadPhoneme(string label, int groupID, float instantValue, float smoothValue){}

    /// <summary> There will be no phoneme group reading if this function returns false. </summary>
    protected virtual bool CanRead() { return true; }

    #region Update stuff

    protected virtual void Update()
    {
        if (!CanRead())
            return;
        bool playing = audio.isPlaying;

        //Wake up or bypasse evaluation
        sleeping &= !playing;
        if (sleeping)
            return;

        //Get and build arrays
        KeyValuePair<string, float>[] pairs = GetValues();
        int count = pairs.Length;
        if (smoothValues == null || count != smoothValues.Length)
            smoothValues = new float[count];

        //No smoothing
        if (smoothFactor < 0.00001f)
        {
            for (int i = 0; i < count; i++)
            {
                float newValue = playing ? pairs[i].Value : 0;
                if (Mathf.Abs(smoothValues[i] - newValue) > 0.0001f)
                {
                    smoothValues[i] = newValue;
                    OnReadPhoneme(pairs[i].Key, i, newValue, newValue);
                }
            }
            if (!playing)
                sleeping = true;
        }

        //Smoothing
        else
        {
            float smoothDelta = Time.deltaTime * Mathf.Lerp(60.0f, 1.0f, smoothFactor);
            bool dirty = false;
            for (int i = 0; i < count; i++)
            {
                float newValue = Mathf.Lerp(smoothValues[i], playing ? pairs[i].Value : 0, smoothDelta);
                if (Mathf.Abs(smoothValues[i] - newValue) > 0.0001f)
                {
                    smoothValues[i] = newValue;
                    dirty = true;
                    OnReadPhoneme(pairs[i].Key, i, pairs[i].Value, smoothValues[i]);
                }
            }
            if (!dirty)
                sleeping = true;
        }
    }

    #endregion

    #region Phonemes utilities
    public delegate void GetValueCallback(string label, float value);

    /// <summary> Get values at current audioTime </summary>
    public virtual void GetValues(GetValueCallback callback)
    {
        KeyValuePair<string, float>[] pairs = GetValues();
        for (int i = 0; i < pairs.Length; i++)
            callback.Invoke(pairs[i].Key, pairs[i].Value);
    }

    /// <summary> Get values at time </summary>
    public virtual void GetValues(GetValueCallback callback, float time)
    {
        KeyValuePair<string, float>[] pairs = GetValues(time);
        for (int i = 0; i < pairs.Length; i++)
            callback.Invoke(pairs[i].Key, pairs[i].Value);
    }

    /// <summary> Get values at current audioTime </summary>
    public virtual KeyValuePair<string, float>[] GetValues()
    {
        if (clip == null)
            throw new System.NullReferenceException("The data field is null on the Phoneme Reader.");
        if (audio == null)
            throw new System.NullReferenceException("AudioSource is missing.");
        if (audio.clip == null)
            throw new System.NullReferenceException("The audio source doesn't have a clip.");
        return clip.phonemes.refined.Evaluate(audio.time / audio.clip.length);
    }

    /// <summary> Get values at time </summary>
    public virtual KeyValuePair<string, float>[] GetValues(float time)
    {
        if (clip == null)
            throw new System.NullReferenceException("The data field is null on the Phoneme Reader.");
        return clip.phonemes.refined.Evaluate(time);
    }
    #endregion

    #region Audio utilities

    /// <summary> returns true if audio is playing. </summary>
    public bool isPlaying
    {
        get
        {
            return audio != null && audio.isPlaying;
        }
    }

    public void Play()
    {
        audio.Play();
    }

    public void Pause()
    {
        audio.Pause();
    }

    public void UnPause()
    {
        audio.UnPause();
    }

    public void Stop()
    {
        audio.Stop();
    }
    #endregion
}