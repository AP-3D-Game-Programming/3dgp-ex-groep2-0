using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isLocked = false;
    public bool hasClosed = false;

    public float openAngle = 0f;
    public float closeSpeed = 250f;

    public float targetAngle = 0f;

    void Update()
    {
        float angle = Mathf.LerpAngle(transform.localEulerAngles.y, targetAngle, Time.deltaTime * (closeSpeed / 100f));
        transform.localEulerAngles = new Vector3(0, angle, 0);
    }

    public void SlamShut(float delay)
    {
        Invoke(nameof(SlamShutPr), delay);
    }
    private void SlamShutPr()
    {
        if (hasClosed) return;

        targetAngle = -90f;           // closed angle
        closeSpeed = 250f;          // fast rotation
        isLocked = true;
        hasClosed = true;
    }

    // optional: try to open the door later
    public void TryOpen()
    {
        if (isLocked) return;
        targetAngle = openAngle;
    }
}
