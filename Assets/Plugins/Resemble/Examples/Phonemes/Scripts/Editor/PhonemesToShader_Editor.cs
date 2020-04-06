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
        if (reader.clip != null)
        {
            Phonemes_Editor.DrawGraph(reader.clip.phonemes.refined, ref temp, Repaint, null);
        }
        if (temp != time)
        {
            time = temp;
            if (!Application.isPlaying)
                reader.Evaluate(time);
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
}