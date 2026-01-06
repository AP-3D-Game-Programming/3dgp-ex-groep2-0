using System.Collections;
using UnityEngine;

public class DeurSequentie : MonoBehaviour
{
    public Animator deurAnimator;
    public GameObject tekstObject; // "Press E to open"

    // Het slot
    public bool magOpenen = false; 

    private bool spelerIsInDeBuurt = false;
    private bool deurIsAlOpen = false;

    void Start()
    {
        if (tekstObject != null) tekstObject.SetActive(false);
    }

    void Update()
    {
        // --- DEZE REGELS ZIJN NIEUW (De fix) ---
        // Als de speler er staat, de deur dicht is, en hij MAG open, 
        // maar de tekst staat uit? Zet hem dan alsnog aan.
        if (spelerIsInDeBuurt && !deurIsAlOpen && magOpenen)
        {
            if (tekstObject != null && !tekstObject.activeSelf)
            {
                tekstObject.SetActive(true);
            }
        }
        // ---------------------------------------

        // De interactie check
        if (spelerIsInDeBuurt && !deurIsAlOpen && magOpenen && Input.GetKeyDown(KeyCode.E))
        {
            OpenNuDeDeur();
        }
        else if (spelerIsInDeBuurt && !deurIsAlOpen && !magOpenen && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Deur zit nog op slot! Drink eerst je bier en wacht op de tekst.");
        }
    }

    void OpenNuDeDeur()
    {
        deurIsAlOpen = true;
        if (deurAnimator != null) deurAnimator.SetTrigger("Open");
        if (tekstObject != null) tekstObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !deurIsAlOpen)
        {
            spelerIsInDeBuurt = true;
            // We proberen de tekst alvast te tonen, maar Update vangt het op als het nu nog niet mag
            if (tekstObject != null && magOpenen) tekstObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spelerIsInDeBuurt = false;
            if (tekstObject != null) tekstObject.SetActive(false);
        }
    }
}