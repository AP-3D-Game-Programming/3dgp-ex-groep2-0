using UnityEngine;

public class PortalWindowCamera : MonoBehaviour
{
    public Transform playerCamera; // Your real camera (main camera)
    public Transform plane;        // The plane showing the RenderTexture
    public float distanceBehindPlane = 5f;

    void LateUpdate()
    {
        //
        // 1. Position the portal camera behind the plane
        //
        transform.position = plane.position - plane.forward * distanceBehindPlane;

        //
        // 2. Compute how the player is looking relative to the plane
        //
        Vector3 relativeLookDir = plane.InverseTransformDirection(playerCamera.forward);

        //
        // 3. Apply that same “relative look direction” to the portal camera
        //
        Vector3 portalLookDir = transform.TransformDirection(relativeLookDir);

        //
        // 4. Rotate the portal camera
        //
        transform.rotation = Quaternion.LookRotation(portalLookDir, Vector3.up);
    }
}
