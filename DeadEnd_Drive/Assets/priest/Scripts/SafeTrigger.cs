using UnityEngine;

public class SafeTrigger : MonoBehaviour
{
    public MonsterManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.DespawnPriest();
        }
    }
}
