using UnityEngine;

public class EndAnimation : MonoBehaviour
{
[Header("Instellingen")]
    public Animator objectAnimator;      // De animator die moet afspelen
    public string animatieNaam;          // De exacte naam van de animatie-state
    public MonoBehaviour spelerScript;   // Het script dat de speler laat bewegen
    
    // Zorgt ervoor dat het maar 1 keer gebeurt
    private bool isAlAfgespeeld = false; 

    private void OnTriggerEnter(Collider other)
    {
        // Check of de speler de trigger raakt
        if (other.CompareTag("Player") && !isAlAfgespeeld)
        {
            StartCutscene();
        }
    }

    void StartCutscene()
    {
        isAlAfgespeeld = true;

        // 1. Bevries de speler
        // We zetten het script uit dat inputs regelt (lopen/kijken)
        if (spelerScript != null)
        {
            spelerScript.enabled = false;
        }

        // 2. Start de animatie
        if (objectAnimator != null)
        {
            objectAnimator.Play(animatieNaam);
        }
    }
}
