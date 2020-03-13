using System.Collections.Generic;
using UnityEngine;

public class PhonemesToShader : PhonemeReader
{

    public new Renderer renderer;
    public int materialIndex;
    [HideInInspector] public Material material;
    public float factor = 1.0f;

    protected virtual void Awake()
    {
        material = renderer.materials[materialIndex];
    }


    protected virtual void Update()
    {
        if (renderer == null)
            return;

        if (isPlaying)
        {
            KeyValuePair<string, float>[] values = GetValues();
            for (int i = 0; i < values.Length; i++)
                material.SetFloat(string.Format("_{0}", values[i].Key), Mathf.Clamp01(values[i].Value * factor));
        }
    }
}
