using System.Collections;
using UnityEngine;

public class BeerInteraction : MonoBehaviour
{
    [Header("Instellingen")]
    public Animator anim;
    public GameObject drukOpETekst;    
    public GameObject subtitelTekst;   
    public float animatieDuur = 4.0f; 

    [Header("Nieuwe Tekst & Deur")]
    public GameObject werkTekst;       
    public float wachttijdNaBier = 5.0f; 
    public float tekstDuur = 4.0f;     
    
    // De koppeling naar de deur
    public DeurSequentie deDeur; 

    [Header("Speler Bevriezen")]
    public MonoBehaviour loopScript;
    public MonoBehaviour kijkScript;

    private bool staatInDeBuurt = false;
    private bool actieGestart = false;

    void Start()
    {
        if (drukOpETekst != null) drukOpETekst.SetActive(false);
        if (subtitelTekst != null) subtitelTekst.SetActive(false);
        if (werkTekst != null) werkTekst.SetActive(false);
    }

    void Update()
    {
        if (staatInDeBuurt && !actieGestart && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(SpeelSceneAf());
        }
    }

    IEnumerator SpeelSceneAf()
    {
        actieGestart = true;

        // Verberg de "Druk op E" prompt en zet speler vast
        if (drukOpETekst != null) drukOpETekst.SetActive(false);
        if (loopScript != null) loopScript.enabled = false;
        if (kijkScript != null) kijkScript.enabled = false;

        // Start animatie en ondertiteling
        if (anim != null) anim.SetTrigger("StartMijnAnimatie");
        if (subtitelTekst != null) subtitelTekst.SetActive(true);

        yield return new WaitForSeconds(animatieDuur);

        // Speler weer vrijgeven
        if (loopScript != null) loopScript.enabled = true;
        if (kijkScript != null) kijkScript.enabled = true;
        if (subtitelTekst != null) subtitelTekst.SetActive(false);

        // Wachten... (5 seconden)
        yield return new WaitForSeconds(wachttijdNaBier);

        // 1. Toon de "Ik moet werken" tekst
        if (werkTekst != null) werkTekst.SetActive(true);

        // 2. Wacht terwijl deze tekst leesbaar is (4 seconden)
        yield return new WaitForSeconds(tekstDuur);

        // 3. Verberg de "Ik moet werken" tekst
        if (werkTekst != null) werkTekst.SetActive(false);

        // 4. NU PAS mag de deur open
        if (deDeur != null)
        {
            deDeur.magOpenen = true; 
            
            // BELANGRIJK: Ik heb de regel 'deDeur.tekstObject.SetActive(true)' WEGGEHAALD.
            // Reden: Je wilt niet dat de deurtekst verschijnt terwijl je nog bij het bier staat.
            // Het script op de deur zelf moet regelen dat de tekst verschijnt 
            // zodra de speler naar de deur toe loopt (via OnTriggerEnter op de deur).
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !actieGestart)
        {
            staatInDeBuurt = true;
            if (drukOpETekst != null) drukOpETekst.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            staatInDeBuurt = false;
            if (drukOpETekst != null) drukOpETekst.SetActive(false);
        }
    }
}   