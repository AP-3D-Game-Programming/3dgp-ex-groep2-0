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
        Vector3 playerOffset = playerCam.transform.position - portalOrigin.position;
        Vector3 localOffset = portalOrigin.InverseTransformVector(playerOffset);

        portalCam.transform.position = linkedPortal.position - linkedPortal.TransformVector(localOffset);

        Quaternion relativeRot = Quaternion.Inverse(portalOrigin.rotation) * playerCam.transform.rotation;
        portalCam.transform.rotation = linkedPortal.rotation * relativeRot;

        portalCam.transform.Rotate(0f, 0f, 0f, Space.Self);
    }

}
