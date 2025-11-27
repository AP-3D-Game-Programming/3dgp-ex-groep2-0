using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 300f;  // muisgevoeligheid
    public Transform player;          // sleep hier de Player in
    public float defaultFOV = 60f;    // standaard FOV
    public float fovSpeed = 20f;      // snelheid van FOV-aanpassing met scroll
    public float minFOV = 30f;        // minimale FOV
    public float maxFOV = 120f;       // maximale FOV

    private float xRotation = 0f;
    private Camera cam;               // referentie naar de camera

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>();     // camera ophalen
        if (cam != null)
            cam.fieldOfView = defaultFOV; // standaard FOV instellen
    }

    void Update()
    {
        // --- Muis kijkbeweging ---
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // horizontale rotatie van speler
        player.Rotate(Vector3.up * mouseX);

        // verticale rotatie van camera
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- FOV aanpassen met scrollwheel ---
        if (cam != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                cam.fieldOfView += scroll * fovSpeed;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
            }
        }
    }
}
