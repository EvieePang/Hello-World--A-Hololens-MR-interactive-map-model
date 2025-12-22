using UnityEngine;
public class FaceCamera : MonoBehaviour
{
    // Rotates label to always face the main camera
    void LateUpdate()
    {
        if (Camera.main)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position,
                Camera.main.transform.up);
        }
    }
}
