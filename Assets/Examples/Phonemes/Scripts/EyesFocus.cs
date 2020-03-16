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

    //Hidden
    private Vector3 smoothFocus;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        smoothFocus = Vector3.Lerp(smoothFocus, focus, Time.deltaTime * 24.0f);
        animator.SetLookAtPosition(smoothFocus);
        animator.SetLookAtWeight(1.0f, bodyWeight, headWeight, eyeWeight, clampWeight);
    }
}
