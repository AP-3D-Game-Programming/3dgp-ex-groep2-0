using UnityEngine;
using TMPro;
using System.Collections;

public class StartDialogueLevel1 : MonoBehaviour
{
    public TMP_Text dialogueText;
    public float displayTime = 3f;
    public float fadeSpeed = 1f;

    // Verander 'void Start' naar deze publieke functie
    // Zorg dat het object in de Inspector wel gewoon AAN staat
    public void ShowDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true); // Voor de zekerheid
            dialogueText.text = "I have to find the car, I don't feel right staying here";
            dialogueText.alpha = 1f;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(displayTime);

        while (dialogueText.alpha > 0)
        {
            dialogueText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        dialogueText.gameObject.SetActive(false);
    }
}