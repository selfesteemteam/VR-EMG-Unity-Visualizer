using System.Collections;
using System.Collections.Generic;
using Unity.Rendering.HybridV2;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float radius;
    public float sensitivity = 1f;

    public Vector2 euler = Vector2.zero;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // Rotate Camera
            Cursor.lockState = CursorLockMode.Locked;
            euler.y += Input.GetAxisRaw("Mouse X") * sensitivity;
            euler.x += Input.GetAxisRaw("Mouse Y") * sensitivity;
        } else if (Input.GetMouseButton(1))
        {
            // Pan Camera
            Cursor.lockState = CursorLockMode.Locked;
        } else
        {
            // Unlock Mouse
            Cursor.lockState = CursorLockMode.None;
        }
        euler.y = euler.y % 360f;
        euler.x = Mathf.Clamp(euler.x, -90f, 90f);
    }
    private void LateUpdate()
    {
        Quaternion lookRotation = Quaternion.Euler(euler);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = -lookDirection * radius;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
