using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("Wat moet er gebeuren?")]
    public DoorController doorController;
    public GameObject shadowMan; // Zodat we hem weer weg kunnen halen
    
    [Header("Geluid (Optioneel)")]
    public AudioSource soundSource; // Mag ook op dit object zitten
    public AudioClip slamSound;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Check of het de speler is (en niet een zombie of bal)
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerEvent();
        }
    }

    void TriggerEvent()
    {
        // 1. Deur KEIHARD dicht
        if (doorController != null)
        {
            doorController.SlamShut(0f); // Meteen dicht
        }

        // 2. Monster weg (alsof hij een geest was)
        if (shadowMan != null)
        {
            shadowMan.SetActive(false);
        }

        // 3. Geluid
        if (soundSource != null && slamSound != null)
        {
            soundSource.PlayOneShot(slamSound);
        }

        // 4. Vernietig deze trigger zodat het niet nog eens gebeurt
        Destroy(gameObject, 2f); // Wacht 2 sec zodat geluid kan afspelen
    }
}