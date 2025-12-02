using UnityEngine;

public class FPVCam : MonoBehaviour
{
    public Transform target;   // player or vehicle root
    public Transform head;     // head position for camera offset
    public float sensitivity = 2f;

    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Snap camera to head position (without parenting)
        transform.position = head.position;
    }

    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Rotate target (player/vehicle) horizontally
        target.Rotate(Vector3.up * mouseX);

        // Vertical rotation for camera
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Update camera position & rotation
        transform.position = head.position; // follow head position
        transform.rotation = Quaternion.Euler(pitch, target.eulerAngles.y, 0f);
    }
}
