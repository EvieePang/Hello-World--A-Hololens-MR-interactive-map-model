using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;

public class NonOccludingFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public bool followLeftHand = false;       // whether to follow the left hand 
    public Vector3 palmOffset = new Vector3(0f, 0.05f, 0.08f);  // up offset of the hand to set the initialized position

    [Header("Smoothness")]
    public float posLerp = 14f;
    public float rotLerp = 14f;
    public int stabilizationFrames = 5;
    private int remainingFrames;

    private bool hasPlacedInitially = false;   // whether initialize the position
    private Vector3 desiredPos;
    private Quaternion desiredRot;

    void Start()
    {
        remainingFrames = stabilizationFrames;
    }

    void LateUpdate()
    {
        if (followLeftHand && remainingFrames > 0)
        {
            TryPlaceAtLeftHandFacingCamera();
            remainingFrames--;
            if (remainingFrames == 0)
            {
                followLeftHand = false;
                hasPlacedInitially = true;
            }
        }
    }

    //void LateUpdate()
    //{
    //    if (followLeftHand && !hasPlacedInitially)
    //    {
    //        TryPlaceAtLeftHandFacingCamera();
    //    }
    //}

    private void TryPlaceAtLeftHandFacingCamera()
    {
        var handAggregator = XRSubsystemHelpers.HandsAggregator;
        if (handAggregator == null) return;

        // get the position of left hand
        if (handAggregator.TryGetJoint(TrackedHandJoint.Palm, UnityEngine.XR.XRNode.LeftHand, out HandJointPose palmPose))
        {
            Vector3 palmPos = palmPose.Position;     // + palmPose.Rotation * palmOffset;
            Quaternion palmRot = palmPose.Rotation;

            // menu towards camera forever
            Transform cam = Camera.main ? Camera.main.transform : null;
            if (cam)
            {
                Vector3 toCam = cam.position - palmPos;
                if (toCam.sqrMagnitude < 1e-6f)
                    toCam = cam.forward;

                Quaternion faceCamRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
                Quaternion outward = palmRot * Quaternion.Euler(0, 180f, 0);


                float t = 0.99f;
                //transform.position = Vector3.Lerp(transform.position, palmPos, Time.deltaTime * posLerp);
                //transform.rotation = Quaternion.Slerp(transform.rotation, faceCamRot, Time.deltaTime * rotLerp);
                transform.position = Vector3.Lerp(transform.position, palmPos, t);
                Quaternion targetRot = Quaternion.Slerp(outward, Quaternion.LookRotation(Camera.main.transform.forward), 0.5f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.99f);
            }

            //// once localized locked
            //hasPlacedInitially = true;
            //followLeftHand = false;

            Debug.Log($"[NonOccludingFollow] Menu placed at left hand position={transform.position}, facing camera.");
        }
    }

    // 供 EXIT 或外部复位使用
    public void ResetFollowToHand()
    {
        hasPlacedInitially = false;
        followLeftHand = true;
        remainingFrames = stabilizationFrames;
        Debug.Log("[Follow] Reset follow → start stabilizing hand position");
    }
}
