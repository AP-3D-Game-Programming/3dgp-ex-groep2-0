using UnityEngine;

public class SafeTrigger : MonoBehaviour
{
    public MonsterManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("entered collider");
            manager.enableAutoRespawn = false;
            manager.DespawnPriest();
        }
    }
}
