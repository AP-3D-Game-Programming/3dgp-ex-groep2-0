using UnityEngine;

public class teleport : MonoBehaviour
{
    public GameObject teleportPoint;
    public GameObject player;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player.transform.position = teleportPoint.transform.position;
    }
}
