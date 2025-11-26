using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float sensitivity = 2f;
    public float distance = 5f;
    private float rotationX;
    private float rotationY;
    public float minDistance = 0f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;
    public float heightOffset = 2f;
    void Start()
    {
        //remove and confine cursor to gamespace
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;
        //block player from moving too far
        rotationY = Mathf.Clamp(rotationY, 0, 70f);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        Vector3 targetPosition = player.position + Vector3.up * heightOffset;

        //follow after player
        transform.position = targetPosition + rotation * direction;
        transform.LookAt(targetPosition);

    }
}
