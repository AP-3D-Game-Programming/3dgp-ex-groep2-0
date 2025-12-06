using UnityEngine;

public class PortalWindowCamera : MonoBehaviour
{
    public Camera portalCam;
    public Material portalMat;
    public Camera playerCam;

    public Transform portalOrigin;     // Your portal's Transform
    public Transform linkedPortal;     // The other portal's Transform

    void Start()
    {
        var rt = new RenderTexture(Screen.width, Screen.height, 24);
        portalCam.targetTexture = rt;
        portalMat.mainTexture = rt;
    }

    void LateUpdate()
    {
        // 1. Compute player's position relative to the portal plane
        Vector3 playerOffset = playerCam.transform.position - portalOrigin.position;

        // 2. Transform offset into portal local space
        Vector3 localOffset = portalOrigin.InverseTransformVector(playerOffset);

        // 3. Mirror offset on Z axis (assuming portal faces +Z)
        localOffset.z = -localOffset.z;

        // 4. Transform back into world space relative to linked portal
        portalCam.transform.position = linkedPortal.position + linkedPortal.TransformVector(localOffset);

        // 5. Adjust rotation to mirror portal orientation
        Quaternion relativeRot = Quaternion.Inverse(portalOrigin.rotation) * playerCam.transform.rotation;
        portalCam.transform.rotation = linkedPortal.rotation * relativeRot;
    }
}
