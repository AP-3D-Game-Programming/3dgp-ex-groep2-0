using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform player;   // Assign the player object (root)
    public Transform head;     // Assign the sphere representing the head
    public float sensitivity = 2f;

    private float pitch = 0f;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Snap camera to head position
        transform.position = head.position;
        transform.parent = player;           // Parent to player for horizontal rotation
        transform.localPosition = head.localPosition; // maintain head offset
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Rotate player left/right
        player.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
