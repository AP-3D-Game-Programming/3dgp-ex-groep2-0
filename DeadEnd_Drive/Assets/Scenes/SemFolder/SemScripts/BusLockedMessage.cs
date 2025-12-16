using UnityEngine;
using TMPro;

public class BusLockedMessage : MonoBehaviour
{
    public CarEntry carEntry;
    public TextMeshProUGUI Prompt;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Als de auto entry uit staat (dus auto op slot)
            if (carEntry != null && carEntry.enabled == false)
            {
                Debug.Log("De auto is op slot, tekst wordt nu getoond.");
                
                Prompt.text = "Shit, its locked. The key should be around";
                
                // --- DE OPLOSSING ---
                Prompt.alpha = 1f;  // Zet de tekst weer op 'zichtbaar' (1 = ondoorzichtig)
                // --------------------

                Prompt.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Prompt.gameObject.SetActive(false);
        }
    }
}