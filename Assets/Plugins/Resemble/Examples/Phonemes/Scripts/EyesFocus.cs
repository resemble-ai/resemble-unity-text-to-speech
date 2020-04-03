using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EyesFocus : MonoBehaviour
{
    //Exposed
    public Vector3 focus;
    [Header("Look at weights")]
    [Range(0.0f, 1.0f)] public float bodyWeight;
    [Range(0.0f, 1.0f)] public float headWeight;
    [Range(0.0f, 1.0f)] public float eyeWeight;
    [Range(0.0f, 1.0f)] public float clampWeight;

    [Header("Lid")]
    [Range(0.0f, 1.0f)] public float lid;
    public Transform leftLid;
    public Transform rightLid;
    public bool autoRemapLid;
    public Vector4 lidRemap;

    [Header("Blink")]
    public Vector2 blinkFrequency;
    public AnimationCurve blink;
    public float blinkSpeed;

    //Hidden
    private Vector3 smoothFocus;
    private Animator animator;
    private float lastBlink;
    private float nextBlink;
    private float blinkFactor;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (autoRemapLid)
        {
            float verticality = Vector3.Dot(Vector3.up, focus);
            verticality = Mathf.InverseLerp(lidRemap.x, lidRemap.y, verticality);
            lid = Mathf.Lerp(lidRemap.z, lidRemap.w, verticality);
        }

        //Blink
        if (Time.time > nextBlink)
        {
            lastBlink = Time.time;
            nextBlink = lastBlink + Random.Range(blinkFrequency.x, blinkFrequency.y);
        }
        float blinkDelta = Time.time - lastBlink;
        blinkFactor = blinkDelta < blinkSpeed ? blink.Evaluate(blinkDelta / blinkSpeed) : 0.0f;
    }

    private void OnAnimatorIK()
    {
        smoothFocus = Vector3.Lerp(smoothFocus, focus, Time.deltaTime * 24.0f);
        animator.SetLookAtPosition(smoothFocus);
        animator.SetLookAtWeight(1.0f, bodyWeight, headWeight, eyeWeight, clampWeight);
        float lidValue = Mathf.Max(lid, blinkFactor);
        leftLid.localRotation = Quaternion.Euler(Vector3.right * lidValue * 40);
        rightLid.localRotation = Quaternion.Euler(Vector3.right * (lidValue * 40 + 180));
    }
}
