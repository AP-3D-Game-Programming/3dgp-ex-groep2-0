using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 300f; // hogere waarde voor beter effect
    public Transform player; // sleep hier de Player in

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // roteer speler horizontaal
        player.Rotate(Vector3.up * mouseX);

        // roteer camera verticaal
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
