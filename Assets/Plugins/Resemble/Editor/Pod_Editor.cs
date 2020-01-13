using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Pod))]
public class Pod_Editor : Editor
{
    private Pod pod;
    private AudioPreview preview;
    public AudioClip clip;

    protected override bool ShouldHideOpenButton()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {
        pod = target as Pod;

        EditorGUILayout.Popup("Voice", 0, new string[] { "Lucie", "Jhon", "OldMen" });
        EditorGUILayout.IntSlider("Pitch", 0, -3, 3);


        EditorGUI.BeginChangeCheck();
        pod.url = EditorGUILayout.TextField("Url", pod.url);
        pod.name = EditorGUILayout.TextField("Name", pod.name);

        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField);
        GUIStyle richTextStyle = new GUIStyle(EditorStyles.largeLabel);
        richTextStyle.richText = true;
        richTextStyle.fontSize = 14;
        richTextStyle.fontStyle = FontStyle.Normal;
        richTextStyle.wordWrap = true;

        /*
        GUI.Box(rect, "");
        rect.Set(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
        
        Vector2 start = richTextStyle.GetCursorPixelPosition(rect, new GUIContent(pod.oldText), 5);
        Vector2 end = richTextStyle.GetCursorPixelPosition(rect, new GUIContent(pod.oldText), 13);
        float heigth = richTextStyle.CalcHeight(new GUIContent("|"), 1) - 4;
        

        //EditorGUI.DrawRect(new Rect(start.x, start.y, end.x - start.x, 14), Color.cyan);
        Rect btnRect = new Rect(start.x - 1, start.y, end.x - start.x + 2, heigth);
        Resemble_Resources.instance.textMat.SetFloat("_Ratio", btnRect.width / btnRect.height);
        EditorGUI.DrawPreviewTexture(btnRect, Texture2D.whiteTexture, Resemble_Resources.instance.textMat);

*/
        pod.text.OnGUI();
/*
        Rect rect = GUILayoutUtility.GetRect(300, 150);

        
        GUILayout.Space(10);
        GUILayout.Label("Adding new sentence", EditorStyles.largeLabel);
        GUILayout.Space(10);
        pod.oldText = GUI.TextArea(rect, pod.oldText, richTextStyle);
        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        
        GUILayout.Label(editor.cursorIndex + "  " + editor.SelectedText);
        GUILayout.Space(10);
        */


        //GUILayout.Label("Test label with <color=red>material</color> change inside.", richTextStyle);

        GUILayout.Space(30);

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(pod);

        if (GUILayout.Button("Clear pod"))
        {
            Object[] clips = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(pod));
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] is AudioClip)
                    AssetDatabase.RemoveObjectFromAsset(clips[i]);
            }
            EditorUtility.SetDirty(pod);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(pod), ImportAssetOptions.ForceUpdate);
        }

        if (GUILayout.Button("Regenerate audio clip"))
        {
            preview = new AudioPreview(pod.url);
            Repaint();
        }

        if (preview != null && preview.clip != null && GUILayout.Button("Save as audioClip"))
        {
            clip = preview.clip;
            clip.name = pod.name;
            AssetDatabase.AddObjectToAsset(clip, pod);
            EditorUtility.SetDirty(pod);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(pod), ImportAssetOptions.ForceUpdate);
            Repaint();
        }
    }

}
