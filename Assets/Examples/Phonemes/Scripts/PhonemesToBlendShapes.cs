using UnityEngine;

public class PhonemesToBlendShapes : PhonemeReader
{

    public new SkinnedMeshRenderer renderer;
    public int materialIndex;
    public float factor = 1.0f;
    [HideInInspector] public int[] remap;

    protected virtual void Update()
    {
        if (renderer == null)
            return;

        if (isPlaying)
        {
            float time = audio.time / audio.clip.length;
            SetValuesToBlendShapes(time);
        }
    }

    public virtual void SetValuesToBlendShapes(float time)
    {
        if (renderer != null)
        {
            float f = 100 * factor;
            int count = data.curves.Length;
            float[] values = new float[count];
            for (int i = 0; i < count; i++)
            {
                float value = data.curves[i].curve.Evaluate(time);
                values[i] = Mathf.Max(values[i], value);
            }
            for (int i = 0; i < count; i++)
            {
                int id = remap[i];
                if (id == -1)
                    continue;
                renderer.SetBlendShapeWeight(id, values[i] * f);
            }
        }
    }

}
