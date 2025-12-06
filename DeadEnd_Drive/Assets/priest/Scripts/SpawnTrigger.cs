using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public MonsterManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.SpawnPriest();
        }
    }
}
