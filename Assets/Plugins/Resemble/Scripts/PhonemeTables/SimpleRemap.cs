using UnityEngine;
using Resemble;

public class SimpleRemap : PhonemeTable
{
    public override Phonemes RefineData(PhonemesTimeStamps raw)
    {
        Phonemes phonemes = new Phonemes();
        phonemes.curves = new Phonemes.PhonemeCurve[groups.Length];

        for (int i = 0; i < groups.Length; i++)
        {
            phonemes.curves[i].curve = new AnimationCurve();
            phonemes.curves[i].name = groups[i].name;
        }

        float inLength = 1.0f / raw.end_times[raw.end_times.Length - 1];

        for (int j = 0; j < groups.Length; j++)
        {
            for (int i = 0; i < raw.end_times.Length; i++)
            {
                float time = raw.end_times[i] * inLength;
                string pho = raw.phonemesChars[i].ToString();
                bool contains = groups[j].phonemes.Contains(pho);
                Keyframe keyFrame = new Keyframe(time, contains ? 0.5f : 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
                phonemes.curves[j].curve.AddKey(keyFrame);
            }
        }

        for (int i = 0; i < phonemes.curves.Length; i++)
        {
            for (int j = 0; j < phonemes.curves[i].curve.length; j++)
            {
                phonemes.curves[i].curve.SmoothTangents(j, 0.0f);
            }
        }

        return phonemes;
    }
}
