using UnityEngine;

public class WorldSpaceBillboard : MonoBehaviour
{
    public Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            targetCamera = FindAnyObjectByType<Camera>();
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        transform.LookAt(
            transform.position + targetCamera.transform.rotation * Vector3.forward,
            targetCamera.transform.rotation * Vector3.up
        );
    }
}