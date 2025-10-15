using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonOccludingFollow : MonoBehaviour
{
    [Header("Placement")]
    public float baseDistance = 0.12f;  
    public float sideOffset = 0.08f;   
    public float upOffset = 0.02f;   
    public LayerMask occlusionMask;      

    [Header("Facing")]
    public bool yawOnly = true;
    public float posLerp = 14f;
    public float rotLerp = 14f;

    // XR  Rig Main Camera
    [SerializeField] private Transform cameraTransform;

    // object localize and rotate
    private Vector3 desiredPos;
    private Quaternion desiredRot;

    public void SetCamera(Transform cam)
    {
        cameraTransform = cam;
    }

    private void Awake()
    {
        TryFindCameraIfNull();
    }

    private void TryFindCameraIfNull()
    {
        if (cameraTransform) return;

        // MainCamera
        if (Camera.main) { cameraTransform = Camera.main.transform; return; }

        // Find any Camera
        var anyCam = FindObjectOfType<Camera>();
        if (anyCam) cameraTransform = anyCam.transform;
    }

    /// <summary>
    /// </summary>
    public void PlaceAt(Vector3 hitPoint, Vector3 hitNormal)
    {
        TryFindCameraIfNull();
        if (!cameraTransform) return;

        // initial position
        var pos = hitPoint
                  + hitNormal.normalized * baseDistance
                  + cameraTransform.right * sideOffset
                  + Vector3.up * upOffset;

        // simple avoid
        var dir = (pos - cameraTransform.position).normalized;
        float d = Vector3.Distance(cameraTransform.position, pos);
        if (Physics.Raycast(cameraTransform.position, dir, out var rh, d, occlusionMask, QueryTriggerInteraction.Ignore))
            pos = rh.point + hitNormal.normalized * 0.05f;

        desiredPos = pos;

        // orientation
        Vector3 toCam = (cameraTransform.position - desiredPos);
        if (yawOnly) toCam.y = 0f;
        if (toCam.sqrMagnitude < 1e-6f) toCam = Vector3.forward;
        desiredRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);

        transform.position = desiredPos;
        transform.rotation = desiredRot;
    }

    private void LateUpdate()
    {
        if (!cameraTransform) return;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * posLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotLerp);
    }
}