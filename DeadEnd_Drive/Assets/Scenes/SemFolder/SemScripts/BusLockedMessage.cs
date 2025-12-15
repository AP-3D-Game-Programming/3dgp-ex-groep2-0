using UnityEngine;
using TMPro;

public class BusLockedMessage : MonoBehaviour
{
    public CarEntry carEntry;
    public TextMeshProUGUI Prompt;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check of er überhaupt IETS de trigger raakt
        Debug.Log("Iets heeft de trigger geraakt: " + other.name);

        if (other.CompareTag("Player"))
        {
            // 2. Check of de tag klopt
            Debug.Log("Het is de speler!");

            // 3. Check de status van de CarEntry
            if (carEntry == null)
            {
                Debug.LogError("FOUT: Je bent vergeten CarEntry in het vakje te slepen in de Inspector!");
            }
            else if (carEntry.enabled == false)
            {
                Debug.Log("De auto is op slot, tekst wordt nu getoond.");
                Prompt.text = "Shit, its locked. The key should be around";
                Prompt.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("De auto is al open (CarEntry is true), dus we tonen geen tekst.");
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