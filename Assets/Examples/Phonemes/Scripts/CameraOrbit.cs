using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{

    public Vector3 focusPoint;
    public float distance = 10.0f;
    public float sensibility = 0.1f;
    public Vector2 pitchMinMax;
    public Vector2 yawMinMax;
    public Vector3 posOffset;

    private float pitch;
    private float yaw;
    private float pitchTarget;
    private float yawTarget;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        //Get inputs
        yawTarget += Input.GetAxis("Mouse X") * sensibility;
        pitchTarget -= Input.GetAxis("Mouse Y") * sensibility;

        //Clamps values
        yawTarget = Mathf.Clamp(yawTarget, yawMinMax.x, yawMinMax.y);
        pitchTarget = Mathf.Clamp(pitchTarget, pitchMinMax.x, pitchMinMax.y);

        //Smooth values
        yaw = Mathf.Lerp(yaw, yawTarget, Time.deltaTime * 6.0f);
        pitch = Mathf.Lerp(pitch, pitchTarget, Time.deltaTime * 6.0f);

        //Compute and apply position & rotation
        float cosPitch = Mathf.Cos(pitch);
        Vector3 offset = new Vector3(Mathf.Sin(yaw) * cosPitch, Mathf.Sin(pitch), Mathf.Cos(yaw) * cosPitch);
        transform.position = focusPoint + offset * distance + posOffset;
        transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);
    }
}
