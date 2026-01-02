using System.Collections;
using UnityEngine;

public class BeerInteraction : MonoBehaviour
{
 [Header("Instellingen")]
    public Animator anim;
    public GameObject tekstObject;
    public float animatieDuur = 4.0f; // VUL HIER IN: Hoe lang duurt je animatie (in seconden)?

    [Header("Speler Bevriezen")]
    public MonoBehaviour loopScript;
    public MonoBehaviour kijkScript;

    private bool staatInDeBuurt = false;
    private bool actieGestart = false;

    void Start()
    {
        if (tekstObject != null) tekstObject.SetActive(false);
    }

    void Update()
    {
        if (staatInDeBuurt && !actieGestart && Input.GetKeyDown(KeyCode.E))
        {
            // Start de routine die wacht
            StartCoroutine(SpeelSceneAf());
        }
    }

    // Dit is een speciale functie die de tijd kan pauzeren
    IEnumerator SpeelSceneAf()
    {
        actieGestart = true;

        // 1. Verberg tekst & Bevries speler
        if (tekstObject != null) tekstObject.SetActive(false);
        if (loopScript != null) loopScript.enabled = false;
        if (kijkScript != null) kijkScript.enabled = false;

        // 2. Start de animatie
        if (anim != null) anim.SetTrigger("StartMijnAnimatie");

        // 3. WACHT hier precies zolang als je hebt ingesteld
        yield return new WaitForSeconds(animatieDuur);

        // 4. Alles is klaar: Geef controle terug aan de speler!
        if (loopScript != null) loopScript.enabled = true;
        if (kijkScript != null) kijkScript.enabled = true;

        // Optioneel: Zet actieGestart op false als je het nog eens wilt kunnen doen
        // actieGestart = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !actieGestart)
        {
            staatInDeBuurt = true;
            if (tekstObject != null) tekstObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            staatInDeBuurt = false;
            if (tekstObject != null) tekstObject.SetActive(false);
        }
    }
}
