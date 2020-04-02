using UnityEngine;

public class PhonemesToBlendShapes : PhonemeReader
{

    public new SkinnedMeshRenderer renderer;
    public int materialIndex;
    public float factor = 1.0f;
    public float smoothFactor = 3.0f;

    [HideInInspector] public int[] remap;
    private float[] smoothValues;

    protected private void Awake()
    {
        smoothValues = new float[clip.phonemes.refined.curves.Length];
        audio.clip = clip.clip;
    }

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
            if (smoothValues == null)
                smoothValues = new float[clip.phonemes.refined.curves.Length];

            float f = 100 * factor;
            int count = clip.phonemes.refined.curves.Length;
            float[] values = new float[count];
            float smoothDelta = Time.deltaTime * smoothFactor;
            for (int i = 0; i < count; i++)
            {
                float value = clip.phonemes.refined.curves[i].curve.Evaluate(time);
                values[i] = Mathf.Max(values[i], value);
            }
            for (int i = 0; i < count; i++)
            {
                int id = remap[i];
                if (id == -1)
                    continue;

                if (Application.isPlaying)
                {
                    if (values[i] > smoothValues[i] || true)
                        smoothValues[i] = values[i];
                    else
                        smoothValues[i] = Mathf.Lerp(smoothValues[i], values[i], smoothDelta);
                    renderer.SetBlendShapeWeight(id, smoothValues[i] * f);
                }
                else
                {
                    renderer.SetBlendShapeWeight(id, values[i] * f);
                }
            }
        }
    }

}
