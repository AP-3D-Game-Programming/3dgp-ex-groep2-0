using UnityEngine;
using System.Collections;

public class MonsterManager : MonoBehaviour
{
    public GameObject priest;
    public Transform spawnPoint;

    [Header("Respawn Settings")]
    public bool enableAutoRespawn = true;
    public float respawnDelay = 5f;

    private HybridPriestAI priestAI;
    private bool priestIsActive = false;

    void Start()
    {
        if (priest != null)
        {
            priestAI = priest.GetComponent<HybridPriestAI>();
            priest.SetActive(false);
        }
    }

    public void SpawnPriest()
    {
        if (priestIsActive) return;

        priest.transform.position = spawnPoint.position;
        priest.transform.rotation = spawnPoint.rotation;
        priest.SetActive(true);

        if (priestAI != null)
        {
            priestAI.currentState = HybridPriestAI.State.Flee; // Or Orbit
            priestAI.isJumpScaring = false;
        }

        priestIsActive = true;
        Debug.Log("Priest SPAWNED");
    }

    public void DespawnPriest()
    {
        if (!priestIsActive) return;

        priest.SetActive(false);
        priestIsActive = false;
        Debug.Log("Priest DESPAWNED");

        if (enableAutoRespawn)
            StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnPriest();
    }
}

