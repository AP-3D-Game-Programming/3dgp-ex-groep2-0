using UnityEngine;

public class PlayerRespawnHandler : MonoBehaviour
{
    public Transform player; // drag PLAYER root here
    public Transform[] respawnPoints;

    private int lastIndex = -1;

    public void RespawnPlayer()
    {
        if (!player)
        {
            Debug.LogError("PlayerRespawnHandler: PLAYER reference missing!");
            return;
        }

        if (respawnPoints.Length == 0)
        {
            Debug.LogWarning("NO RESPAWN POINTS ASSIGNED!");
            return;
        }

        // Pick a new spawn index different from last time
        int newIndex;
        do
        {
            newIndex = Random.Range(0, respawnPoints.Length);
        }
        while (newIndex == lastIndex && respawnPoints.Length > 1);

        lastIndex = newIndex;
        Transform spawn = respawnPoints[newIndex];

        // Teleport player
        player.position = spawn.position;
        player.rotation = spawn.rotation;

        // Reset physics if Rigidbody exists
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Respawned Player at {spawn.name}");
    }
}
