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
    
    // NIEUW: De koppeling naar de deur
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

        if (drukOpETekst != null) drukOpETekst.SetActive(false);
        if (loopScript != null) loopScript.enabled = false;
        if (kijkScript != null) kijkScript.enabled = false;

        if (anim != null) anim.SetTrigger("StartMijnAnimatie");
        if (subtitelTekst != null) subtitelTekst.SetActive(true);

        yield return new WaitForSeconds(animatieDuur);

        if (loopScript != null) loopScript.enabled = true;
        if (kijkScript != null) kijkScript.enabled = true;
        if (subtitelTekst != null) subtitelTekst.SetActive(false);

        // Wachten...
        yield return new WaitForSeconds(wachttijdNaBier);

        // Tekst tonen
        if (werkTekst != null) werkTekst.SetActive(true);

        // --- HIER GAAT DE DEUR VAN HET SLOT ---
        if (deDeur != null)
        {
            deDeur.magOpenen = true; 
            // Als de speler toevallig al bij de deur staat, toon dan nu de "Press E" tekst
            // (Dit is een detail, maar maakt het netter)
            deDeur.tekstObject.SetActive(true); 
        }
        // --------------------------------------

        yield return new WaitForSeconds(tekstDuur);
        if (werkTekst != null) werkTekst.SetActive(false);
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
