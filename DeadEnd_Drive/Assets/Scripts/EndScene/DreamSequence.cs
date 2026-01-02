using System.Collections;
using TMPro;
using UnityEngine;

public class DreamSequence : MonoBehaviour
{
[Header("Componenten")]
    public MonoBehaviour playerMovementScript; // Sleep je movement script hierin
    public Animator cameraAnimator;            // Sleep je camera hierin
    public TMP_Text tekstVak;                  // Sleep je TextMeshPro tekst hierin

    [Header("Tijden")]
    public float animatieDuur = 4.0f; // Hoe lang duurt het rondkijken?
    public float leesTijd = 3.0f;     // Hoe lang blijft de tekst staan?

    void Start()
    {
        StartCoroutine(SpeelDroomAf());
    }

    IEnumerator SpeelDroomAf()
    {
        // 1. Zet speler uit
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // 2. Start de animatie & Toon TEKST 1 (Tijdens animatie)
        tekstVak.text = "Huh, where am I?";
        tekstVak.gameObject.SetActive(true);

        // Wacht zolang de animatie duurt
        yield return new WaitForSeconds(animatieDuur);

        // 3. Animatie is klaar -> Toon TEKST 2
        tekstVak.text = "I am just in my bed. That means it was all just a dream.";
        
        // Wacht even zodat de speler kan lezen
        yield return new WaitForSeconds(leesTijd);

        // 4. Toon TEKST 3
        tekstVak.text = "I am going to look if there is a beer in the kitchen, I need it.";

        // Wacht iets langer omdat deze zin langer is
        yield return new WaitForSeconds(leesTijd + 2.0f); 

        // 5. KLAAR: Dit is de fix voor jouw probleem!
        
        tekstVak.gameObject.SetActive(false); // Tekst weg
        
        cameraAnimator.enabled = false; // <--- BELANGRIJK: Zet de animator UIT zodat de muis het weer overneemt!
        
        if (playerMovementScript != null) playerMovementScript.enabled = true; // Speler aan
    }
}
