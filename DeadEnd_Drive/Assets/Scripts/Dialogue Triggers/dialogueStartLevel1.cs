using UnityEngine;
using TMPro;
using System.Collections;

public class StartDialogueLevel1 : MonoBehaviour
{
[Header("Sleep hier het TextMeshPro object met je tekst in")]
    public TMP_Text dialogueText;

    [Header("Instellingen")]
    public float displayTime = 3f;
    public float fadeSpeed = 1f;

    // We gebruiken geen Start() meer, want TeleportSecScene roept dit nu aan.
    
    // Deze functie wordt aangeroepen door jouw TeleportSecScene script
    public void ShowDialogue()
    {
        if (dialogueText != null)
        {
            // Zet de tekst aan en reset de zichtbaarheid
            dialogueText.gameObject.SetActive(true);
            dialogueText.alpha = 1f;

            // Start de timer om te faden
            StartCoroutine(FadeOut());
        }
        else
        {
            Debug.LogWarning("StartDialogueLevel1: Geen TextMeshPro object gekoppeld!");
        }
    }

    IEnumerator FadeOut()
    {
        // 1. Wacht
        yield return new WaitForSeconds(displayTime);

        // 2. Fade uit
        while (dialogueText.alpha > 0)
        {
            dialogueText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // 3. Zet uit
        dialogueText.gameObject.SetActive(false);
    }
}