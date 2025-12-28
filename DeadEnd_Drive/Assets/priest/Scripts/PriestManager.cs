using UnityEngine;
using System.Collections;

public class PriestManager : MonoBehaviour
{
    public GameObject priest;
    public Transform spawnPoint;
    [Header("Player Target Settings")]
    public string playerTag = "Player";

    // Cached targets (found once, reused)
    private Transform cachedPlayer;
    private Transform cachedPlayerCamera;


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

        CacheTargetsIfNeeded();

        if (priestAI != null)
        {
            if (cachedPlayer != null) priestAI.player = cachedPlayer;
            if (cachedPlayerCamera != null) priestAI.playerCamera = cachedPlayerCamera;
        }

        priest.SetActive(true);

        if (priestAI != null)
        {
            priestAI.currentState = HybridPriestAI.State.Flee;
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
    private void CacheTargetsIfNeeded()
    {
        if (cachedPlayer == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) cachedPlayer = go.transform;
        }

        if (cachedPlayerCamera == null)
        {
            // Prefer camera under player (best for rigs)
            if (cachedPlayer != null)
            {
                var cam = cachedPlayer.GetComponentInChildren<Camera>(true);
                if (cam) cachedPlayerCamera = cam.transform;
            }

            // Fallback: main camera
            if (cachedPlayerCamera == null && Camera.main != null)
                cachedPlayerCamera = Camera.main.transform;
        }
    }

}

