using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public GameObject priest;
    public Transform spawnPoint;

    private bool priestIsActive = false;

    void Start()
    {
        priest.SetActive(false); // start hidden
    }

    public void SpawnPriest()
    {
        if (priestIsActive) return;

        priest.transform.position = spawnPoint.position;
        priest.transform.rotation = spawnPoint.rotation;
        priest.SetActive(true);
        priestIsActive = true;

        Debug.Log("Priest SPAWNED");
    }

    public void DespawnPriest()
    {
        if (!priestIsActive) return;

        priest.SetActive(false);
        priestIsActive = false;

        Debug.Log("Priest DESPAWNED");
    }
}
