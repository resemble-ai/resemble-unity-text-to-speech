using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    public class Phonemes : ScriptableObject
    {

        public string phonemes;
        public PhonemeCurve[] curves;

        [System.Serializable]
        public struct PhonemeCurve
        {
            [HideInInspector] public string name;
            public char phoneme;
            public AnimationCurve curve;

            public PhonemeCurve(char phoneme, AnimationCurve curve)
            {
                this.name = phoneme.ToString();
                this.phoneme = phoneme;
                this.curve = curve;
            }

        }

        public void BuildCurves(string alignementFilePath)
        {
            //Read alignement file
            string alignement = System.IO.File.ReadAllText(alignementFilePath);

            //Build a float array from alignement
            string[] s = alignement.Split(',');
            float[] values = new float[s.Length];
            for (int i = 0; i < values.Length; i++)
                values[i] = float.Parse(s[i], System.Globalization.CultureInfo.InvariantCulture);

            //Build curves
            Vector2Int alignementSize = new Vector2Int(values.Length / phonemes.Length, phonemes.Length);
            curves = new PhonemeCurve[phonemes.Length];
            for (int y = 0; y < alignementSize.y; y++)
            {
                Keyframe[] keys = new Keyframe[alignementSize.x];
                for (int x = 0; x < alignementSize.x; x++)
                {
                    int id = y * alignementSize.x + x;
                    float t = (float)x / alignementSize.x;
                    keys[x] = new Keyframe(t, values[id]);
                }
                curves[y] = new PhonemeCurve(phonemes[y], new AnimationCurve(keys));
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