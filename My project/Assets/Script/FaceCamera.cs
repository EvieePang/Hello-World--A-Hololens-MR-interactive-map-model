using UnityEngine;
public class FaceCamera : MonoBehaviour
{
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
