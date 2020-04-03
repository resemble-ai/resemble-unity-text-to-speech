using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{

    public Shot longShot;
    public Shot faceShot;

    public float sensibility = 0.1f;
    public Vector2 pitchMinMax;
    public Vector2 yawMinMax;
    public Vector3 posOffset;

    public EyesFocus eyeFocus;

    private float pitch;
    private float yaw;
    private float scroll;
    private float pitchTarget;
    private float yawTarget;
    private float scrollTarget;

    [System.Serializable]
    public struct Shot
    {
        public Vector3 focusPoint;
        public Vector3 focusLocalPosition;
        public float distance;

        public static Shot Lerp(Shot a, Shot b, float t)
        {
            return new Shot()
            {
                focusPoint = Vector3.Lerp(a.focusPoint, b.focusPoint, t),
                focusLocalPosition = Vector3.Lerp(a.focusLocalPosition, b.focusLocalPosition, t),
                distance = Mathf.Lerp(a.distance, b.distance, t),
            };
        }
    }

    void LateUpdate()
    {
        //Init var
        Shot currentShot = Shot.Lerp(longShot, faceShot, scroll);
        float smoothLerp = Time.deltaTime * 6.0f;

        //Get inputs
        yawTarget += Input.GetAxis("Mouse X") * sensibility;
        pitchTarget -= Input.GetAxis("Mouse Y") * sensibility;
        float deltaScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(deltaScroll) > 0.01f)
            scrollTarget = Mathf.Clamp01(scrollTarget + deltaScroll);
        scroll = Mathf.Lerp(scroll, scrollTarget, smoothLerp);

        //Clamps values
        yawTarget = Mathf.Clamp(yawTarget, yawMinMax.x, yawMinMax.y);
        pitchTarget = Mathf.Clamp(pitchTarget, pitchMinMax.x, pitchMinMax.y);

        //Smooth values
        yaw = Mathf.Lerp(yaw, yawTarget, smoothLerp);
        pitch = Mathf.Lerp(pitch, pitchTarget, smoothLerp);

        //Compute and apply position & rotation
        float cosPitch = Mathf.Cos(pitch);
        Vector3 offset = new Vector3(Mathf.Sin(yaw) * cosPitch, Mathf.Sin(pitch), Mathf.Cos(yaw) * cosPitch);
        transform.position = currentShot.focusPoint + offset * currentShot .distance+ posOffset;
        transform.rotation = Quaternion.LookRotation(-offset.normalized, Vector3.up);

        eyeFocus.focus = transform.localToWorldMatrix.MultiplyPoint(currentShot.focusLocalPosition);
    }
}
