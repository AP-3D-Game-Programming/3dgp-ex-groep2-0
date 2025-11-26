using UnityEngine;

public class TeleportOnTouch : MonoBehaviour
{
    public Transform teleportTarget;    // Where the player should be sent
    public string playerTag = "Player"; // The tag of the player

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.transform.position = teleportTarget.position;
        }
    }
}
