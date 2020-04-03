using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEyes : MonoBehaviour
{

    public SkinnedMeshRenderer skin;
    public int materialIndex;
    private Material mat;

    public Color normalColor;

    [Header("Blink")]
    public Action blink;
    public Color blinkColor;

    [Header("Offset")]
    public Action horizontalOffset;

    [System.Serializable]
    public struct Action
    {
        public AnimationCurve curve;
        public float speed;
        public float chances;
        public float cooldown;

        [HideInInspector] public float value;
        [HideInInspector] public bool playing;
        [HideInInspector] public float time;

        public void Refresh(float deltatime)
        {
            if (playing)
            {
                //Playing anim
                time += deltatime * speed;
                value = curve.Evaluate(Mathf.Min(time, 1.0f));

                //Close anim
                if (time > 1.0f)
                {
                    value = curve.Evaluate(1.0f);
                    time = cooldown;
                    playing = false;
                }
            }
            else
            {
                //In cooldown
                time -= deltatime;

                if (time < -1.0f)
                {
                    time += 1.0f;
                    if (Random.Range(0.0f, 1.0f) < chances)
                    {
                        time = 0.0f;
                        value = curve.Evaluate(0.0f);
                        playing = true;
                    }
                }
            }
        }
    }

    void Awake()
    {
        mat = skin.materials[materialIndex];
    }

    void Update()
    {
        blink.Refresh(Time.deltaTime);
        mat.SetColor("_EyeSelect", Color.Lerp(normalColor, blinkColor, blink.value));

        horizontalOffset.Refresh(Time.deltaTime);
        mat.SetFloat("_EyesOffset", horizontalOffset.value);
    }
}
