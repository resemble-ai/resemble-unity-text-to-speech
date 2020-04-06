using System.Collections.Generic;
using UnityEngine;

public class PhonemesToShader : PhonemeReader
{
    //Exposed
    public new Renderer renderer;
    public int materialIndex;
    public float factor = 1.0f;

    //Hidden
    [HideInInspector] public Material material;


    protected virtual void Awake()
    {
        material = renderer.materials[materialIndex];
        audio.clip = clip.clip;
    }

    protected override bool CanRead()
    {
        //Stop reading if renderer is null
        return renderer != null;
    }

    /// <summary> Called when a phoneme group change. </summary>
    protected override void OnReadPhoneme(string label, int groupID, float instantValue, float smoothValue)
    {
        material.SetFloat(string.Format("_{0}", label), smoothValue * factor);
    }

    /// <summary> Applies phoneme group values to shared material. Useful for the editor's preview functions. </summary>
    public virtual void Evaluate(float time)
    {
        if (renderer == null || renderer.sharedMaterials.Length <= materialIndex)
            return;
        Material mat = renderer.sharedMaterials[materialIndex];
        if (mat == null)
            return;
        KeyValuePair<string, float>[] values = GetValues(time);
        for (int i = 0; i < values.Length; i++)
            mat.SetFloat(string.Format("_{0}", values[i].Key), Mathf.Clamp01(values[i].Value * factor));
    }
}