using UnityEngine;

public class SafeTrigger : MonoBehaviour
{
    public PriestManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.DespawnPriest();
        }
    }
}
