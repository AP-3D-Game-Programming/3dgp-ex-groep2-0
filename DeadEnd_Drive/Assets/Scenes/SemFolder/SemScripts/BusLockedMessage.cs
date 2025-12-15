using UnityEngine;
using TMPro;

public class BusLockedMessage : MonoBehaviour
{
    public CarEntry carEntry;       // Sleep hier hetzelfde CarEntry object in als bij je sleutel
    public TextMeshProUGUI Prompt;  // Sleep hier je tekstvak in

    private void OnTriggerEnter(Collider other)
    {
        // Check of het de speler is
        if (other.CompareTag("Player"))
        {
            // We checken: Is het script CarEntry UITgeschakeld?
            // Zo ja, dan heeft de speler de sleutel nog niet gevonden.
            if (carEntry != null && carEntry.enabled == false)
            {
                Prompt.text = "Shit, its locked. The key should be around";
                Prompt.gameObject.SetActive(true);
            }
            // Als carEntry.enabled wél true is, doen we niks 
            // (want dan mag de speler waarschijnlijk instappen via het CarEntry script zelf)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Zorg dat de tekst altijd verdwijnt als je wegloopt
            Prompt.gameObject.SetActive(false);
        }
    }
}