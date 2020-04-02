using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public class Phonemes
    {
        public string phonemes;
        public PhonemeCurve[] curves = new PhonemeCurve[0];

        [System.Serializable]
        public struct PhonemeCurve
        {
            public string name;
            public char phoneme;
            public AnimationCurve curve;

            public PhonemeCurve(char phoneme, AnimationCurve curve)
            {
                this.name = phoneme.ToString();
                this.phoneme = phoneme;
                this.curve = curve;
            }
        }

        /// <summary> Return all curves value at the given time. </summary>
        public KeyValuePair<string, float>[] Evaluate(float normalizedTime)
        {
            KeyValuePair<string, float>[] values = new KeyValuePair<string, float>[curves.Length];
            for (int i = 0; i < curves.Length; i++)
                values[i] = new KeyValuePair<string, float>(curves[i].name, curves[i].curve.Evaluate(normalizedTime));
            return values;
        }
    }
}