using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhonemeReader))]
public class PhonemeReader_Editor : Editor
{

    private static PhonemeReader reader;
    private float time;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        reader = target as PhonemeReader;
        bool isPlaying = Application.isPlaying && reader.audio != null && reader.audio.isPlaying;

        //Playing refresh
        if (isPlaying)
        {
            time = reader.audio.time / reader.audio.clip.length;
            Repaint();
        }

        //Draw graph
        if (reader.data != null)
        {
            Phonemes_Editor.DrawGraph(reader.data, ref time, Repaint);
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
