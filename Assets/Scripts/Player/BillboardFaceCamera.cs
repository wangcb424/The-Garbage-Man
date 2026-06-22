using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main == null)
        {
            return;
        }

        transform.rotation = Camera.main.transform.rotation;
    }
}