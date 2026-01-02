using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Rendering.BoolParameter;

public class AddDialogTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI dialogueText;
    private bool triggered;
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check of de player de trigger raakt en we nog niet gestart zijn
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            StartCoroutine(FadeOut());
            
        }
    }

    IEnumerator FadeOut()
    {
        dialogueText.alpha = 1f;

        dialogueText.gameObject.SetActive(true);

        dialogueText.text = "How did that house get here? This is really confusing... But hey, a jerrycan!!";
        yield return new WaitForSeconds(5);

        while (dialogueText.alpha > 0)
        {
            dialogueText.alpha -= Time.deltaTime * 2;
            yield return null;
        }

        dialogueText.gameObject.SetActive(false);
    }
}
