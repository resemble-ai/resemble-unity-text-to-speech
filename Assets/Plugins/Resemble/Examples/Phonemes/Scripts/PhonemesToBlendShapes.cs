using System.Collections.Generic;
using UnityEngine;

public class PhonemesToBlendShapes : PhonemeReader
{
    //Exposed
    public new SkinnedMeshRenderer renderer;
    public float factor = 1.0f;

    //Hidden
    [HideInInspector] public int[] remap;

    protected private void Awake()
    {
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
        int id = remap[groupID];
        if (id == -1)
            return;
        renderer.SetBlendShapeWeight(id, smoothValue * factor * 100);
    }

    /// <summary> Applies phoneme group values to shared material. Useful for the editor's preview functions. </summary>
    public virtual void Evaluate(float time)
    {
        if (renderer == null)
            return;

        KeyValuePair<string, float>[] values = GetValues(time);
        for (int i = 0; i < values.Length; i++)
            renderer.SetBlendShapeWeight(remap[i], values[i].Value * factor * 100);
    }
}
