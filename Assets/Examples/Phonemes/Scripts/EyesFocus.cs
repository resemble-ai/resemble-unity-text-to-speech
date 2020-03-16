using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EyesFocus : MonoBehaviour
{
    public Transform focus;
    [Range(0.0f, 1.0f)] public float bodyWeight;
    [Range(0.0f, 1.0f)] public float headWeight;
    [Range(0.0f, 1.0f)] public float eyeWeight;
    [Range(0.0f, 1.0f)] public float clampWeight;

    protected Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        animator.SetLookAtPosition(focus.position);
        animator.SetLookAtWeight(1.0f, bodyWeight, headWeight, eyeWeight, clampWeight);
    }
}
