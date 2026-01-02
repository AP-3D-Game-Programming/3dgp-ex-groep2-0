using System.Collections;
using UnityEngine;

public class DeurSequentie : MonoBehaviour
{
public Animator deurAnimator;
    public GameObject tekstObject; // "Press E to open"

    // NIEUW: Het slot. Staat standaard op FALSE (dicht).
    public bool magOpenen = false; 

    private bool spelerIsInDeBuurt = false;
    private bool deurIsAlOpen = false;

    void Start()
    {
        if (tekstObject != null) tekstObject.SetActive(false);
    }

    void Update()
    {
        // We voegen '&& magOpenen' toe aan de check
        if (spelerIsInDeBuurt && !deurIsAlOpen && magOpenen && Input.GetKeyDown(KeyCode.E))
        {
            OpenNuDeDeur();
        }
        // Optioneel: Als je op E drukt terwijl hij op slot zit (voor testen handig)
        else if (spelerIsInDeBuurt && !deurIsAlOpen && !magOpenen && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Deur zit nog op slot! Drink eerst je bier.");
        }
    }

    void OpenNuDeDeur()
    {
        deurIsAlOpen = true;
        deurAnimator.SetTrigger("Open");
        if (tekstObject != null) tekstObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !deurIsAlOpen)
        {
            spelerIsInDeBuurt = true;
            // We tonen de tekst alleen als hij ook echt open MAG
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
