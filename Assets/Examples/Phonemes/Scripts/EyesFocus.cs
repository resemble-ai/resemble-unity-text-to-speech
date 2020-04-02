using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EyesFocus : MonoBehaviour
{
    //Exposed
    public Vector3 focus;
    [Range(0.0f, 1.0f)] public float bodyWeight;
    [Range(0.0f, 1.0f)] public float headWeight;
    [Range(0.0f, 1.0f)] public float eyeWeight;
    [Range(0.0f, 1.0f)] public float clampWeight;
    [Range(0.0f, 1.0f)] public float lid;
    public Transform leftLid;
    public Transform rightLid;
    public bool autoRemapLid;
    public Vector4 lidRemap;

    //Hidden
    private Vector3 smoothFocus;
    private Animator animator;

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
    }

    private void OnAnimatorIK()
    {
        smoothFocus = Vector3.Lerp(smoothFocus, focus, Time.deltaTime * 24.0f);
        animator.SetLookAtPosition(smoothFocus);
        animator.SetLookAtWeight(1.0f, bodyWeight, headWeight, eyeWeight, clampWeight);
        leftLid.localRotation = Quaternion.Euler(Vector3.right * lid * 40);
        rightLid.localRotation = Quaternion.Euler(Vector3.right * (lid * 40 + 180));
    }
}
