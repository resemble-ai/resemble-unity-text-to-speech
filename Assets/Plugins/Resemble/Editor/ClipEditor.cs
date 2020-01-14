using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Audio;
using UnityEditor.Experimental.AssetImporters;

[CustomEditor(typeof(AudioClip))]
public class ClipEditor : Editor
{

    public override bool HasPreviewGUI()
    {
        return base.HasPreviewGUI();
    }

    public override void OnInspectorGUI()
    {
        //Classic audioclip
        AudioClip clip = target as AudioClip;
        if (AssetDatabase.IsMainAsset(clip))
        {
            base.OnInspectorGUI();
            return;
        }

        //AudioClip from other plugin
        CharacterSet pod = AssetDatabase.LoadAssetAtPath<CharacterSet>(AssetDatabase.GetAssetPath(clip));
        if (pod == null)
        {
            base.OnInspectorGUI();
            return;
        }

        base.OnInspectorGUI();
        GUILayout.Space(10);
        pod.text.OnGUI();
        GUILayout.Button("Rebuild clip");
    }

}
