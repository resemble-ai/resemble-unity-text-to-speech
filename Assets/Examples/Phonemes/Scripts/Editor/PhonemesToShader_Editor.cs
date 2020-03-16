using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhonemesToShader))]
public class PhonemeToShader_Editor : Editor
{

    private static PhonemesToShader reader;
    private float time;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        reader = target as PhonemesToShader;
        bool isPlaying = Application.isPlaying && reader.audio != null && reader.audio.isPlaying;

        //Playing refresh
        if (isPlaying)
        {
            time = reader.audio.time / reader.audio.clip.length;
            Repaint();
        }

        //Draw graph
        float temp = time;
        if (reader.data != null)
        {
            Phonemes_Editor.DrawGraph(reader.data, ref temp, Repaint, null);
        }
        if (temp != time)
        {
            time = temp;
            if (!Application.isPlaying)
                SetValuesToShader(time);
        }

        //Draw play button
        if (Application.isPlaying)
        {
            if (!isPlaying)
            {
                if (GUILayout.Button("Play"))
                {
                    reader.audio.Play();
                }
            }
            else
            {
                if (GUILayout.Button("Stop"))
                {
                    reader.audio.Stop();
                }
            }
        }
    }

    void SetValuesToShader(float time)
    {
        if (reader.renderer == null || reader.data == null)
            return;
        Material material = reader.renderer.sharedMaterials[reader.materialIndex];
        if (material == null)
            return;

        KeyValuePair<string, float>[] values = reader.data.Evaluate(time);
        for (int i = 0; i < values.Length; i++)
        {
            material.SetFloat(string.Format("_{0}", values[i].Key), values[i].Value * reader.factor);
        }
    }
}