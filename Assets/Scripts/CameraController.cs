using GD.MinMaxSlider;
using System.Collections;
using System.Collections.Generic;
using Unity.Rendering.HybridV2;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float radius; // Controls camera distance
    public float sensitivity = 5f; // Controls pitch/yaw sensitivity
    public float zoomSensitivity = 0.25f; // Controls zoom sensitivity

    [MinMaxSlider(0, 10)]
    public Vector2 radiusLimit = new Vector2(0.3f, 2f); // Hard limit on camera distance

    [MinMaxSlider(-90, 90)]
    public Vector2 hardPitchLimit = new Vector2(-35f, 70f); // Hard limit on camera pitch
    [MinMaxSlider(-90, 90)]
    public Vector2 softPitchLimit = new Vector2(-25f, 60f); // Moves camera inside the region to outside of it smoothly

    [MinMaxSlider(-180, 180)]
    public Vector2 hardYawLimit = new Vector2(-60f, 60f); // Hard limit on camera yaw
    [MinMaxSlider(-180, 180)]
    public Vector2 softYawLimit = new Vector2(-50f, 50f); // Moves camera inside the region to outside of it smoothly

    public float dampingTime = 0.3f; // Time to move from inside soft limit regions to outside

    private Vector2 euler = Vector2.zero;
    private Vector2 eulerVel = Vector2.zero; // Used for smooth interpolation

    void Update()
    {


        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Rotate Camera
            Cursor.lockState = CursorLockMode.Locked;
            euler.y += Input.GetAxisRaw("Mouse X") * sensitivity;
            euler.x += Input.GetAxisRaw("Mouse Y") * sensitivity;
        } else
        {
            // Unlock Mouse
            Cursor.lockState = CursorLockMode.None;
        }

        // Zoom view on scroll input
        radius += Input.mouseScrollDelta.y * -zoomSensitivity;
        radius = Mathf.Clamp(radius, radiusLimit.x, radiusLimit.y);

        // Hard limit angle values
        euler.y = Mathf.Clamp(euler.y, hardYawLimit.x, hardYawLimit.y);
        euler.x = Mathf.Clamp(euler.x, hardPitchLimit.x, hardPitchLimit.y);

        // Soft limit angle values, moves camera outside the soft limit range smoothly
        if (euler.y > softYawLimit.y)
        {
            euler.y = Mathf.SmoothDampAngle(euler.y, softYawLimit.y, ref eulerVel.y, dampingTime);
        } else if (euler.y < softYawLimit.x)
        {
            euler.y = Mathf.SmoothDampAngle(euler.y, softYawLimit.x, ref eulerVel.y, dampingTime);
        }

        if (euler.x > softPitchLimit.y)
        {
            euler.x = Mathf.SmoothDampAngle(euler.x, softPitchLimit.y, ref eulerVel.x, dampingTime);
        }
        else if (euler.x < softPitchLimit.x)
        {
            euler.x = Mathf.SmoothDampAngle(euler.x, softPitchLimit.x, ref eulerVel.x, dampingTime);
        }
    }
    private void LateUpdate()
    {
        // Rotate camera after all other objects have updated (reduces stuttering when moving)
        Quaternion lookRotation = Quaternion.Euler(euler);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = -lookDirection * radius;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
