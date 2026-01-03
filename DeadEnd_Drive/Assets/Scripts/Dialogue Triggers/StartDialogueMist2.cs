using System.Collections;
using UnityEngine;

public class StartDialogueMist2 : MonoBehaviour
{
[Header("Instellingen")]
    public GameObject teTonenTekst; // Sleep hier je UI Tekst in
    public float aantalSeconden = 3f; // Hoe lang moet de tekst blijven staan?

    void Start()
    {
        // 1. Zorg dat de tekst meteen zichtbaar wordt bij het laden
        if (teTonenTekst != null)
        {
            teTonenTekst.SetActive(true);
            
            // 2. Start de timer om hem weer weg te halen
            StartCoroutine(VerbergTekstNaTijd());
        }
        else
        {
            Debug.LogWarning("Vergeet niet je tekst object in het script te slepen!");
        }
    }

    // Dit is een 'Coroutine', een functie die kan wachten
    IEnumerator VerbergTekstNaTijd()
    {
        // Wacht het aantal seconden dat we hebben ingesteld
        yield return new WaitForSeconds(aantalSeconden);

        // Zet de tekst uit
        if (teTonenTekst != null)
        {
            teTonenTekst.SetActive(false);
        }
    }
}
