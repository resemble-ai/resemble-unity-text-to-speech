using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhonemesToBlendShapes))]
public class PhonemeToBlendShapes_Editor : Editor
{

    private static PhonemesToBlendShapes reader;
    private float time;
    private string[] shapesNames;

    private void OnEnable()
    {
        reader = target as PhonemesToBlendShapes;
        LoadBlendShapes();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        reader = target as PhonemesToBlendShapes;
        bool isPlaying = Application.isPlaying && reader.audio != null && reader.audio.isPlaying;
        CheckRemapArraySize();

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
            Phonemes_Editor.DrawGraph(reader.clip.phonemes.refined, ref temp, Repaint, BlendShapeField);
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

    private void LoadBlendShapes()
    {
        shapesNames = new string[] { "None" };
        if (reader.renderer != null && reader.renderer.sharedMesh != null)
        {
            Mesh mesh = reader.renderer.sharedMesh;
            int count = mesh.blendShapeCount;
            shapesNames = new string[count + 1];
            shapesNames[0] = "None";
            for (int i = 0; i < count; i++)
                shapesNames[i + 1] = mesh.GetBlendShapeName(i);
        }
    }

    private void CheckRemapArraySize()
    {
        if (reader.clip == null)
            return;

        int lengthTarget = reader.clip.phonemes.refined.curves.Length;
        if (reader.remap == null)
            reader.remap = new int[lengthTarget];
        else if (reader.remap.Length != lengthTarget)
            System.Array.Resize<int>(ref reader.remap, lengthTarget);
    }

    private void BlendShapeField(Rect rect, int index)
    {
        LoadBlendShapes();

        bool containsID = index < reader.remap.Length;
        int shapeId = containsID ? reader.remap[index] + 1 : -1;

        int temp = EditorGUI.Popup(rect, shapeId, shapesNames);
        if (temp != shapeId)
            reader.remap[index] = temp - 1;
    }
}