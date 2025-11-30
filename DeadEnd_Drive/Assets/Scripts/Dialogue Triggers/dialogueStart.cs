using UnityEngine;
using TMPro;
using System.Collections;

public class StartDialogue : MonoBehaviour
{
    public TMP_Text dialogueText;     // Assign your TMP text in Inspector
    public float displayTime = 3f;    // How long the text stays
    public float fadeSpeed = 1f;      // Fade-out speed

    void Start()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "Gas is empty… dammit. Maybe I can find something usefull in there";
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