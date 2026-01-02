using System.Collections;
using UnityEngine;

public class DeurSequentie : MonoBehaviour
{
    private Animator mijnAnimator;
    private bool spelerIsInDeBuurt = false;
    private bool deurIsAlOpen = false;

    void Start()
    {
        // We pakken automatisch de animator van dit object
        mijnAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Is de speler er? 2. Is de deur nog dicht? 3. Drukt hij op E?
        if (spelerIsInDeBuurt == true && deurIsAlOpen == false && Input.GetKeyDown(KeyCode.E))
        {
            OpenNuDeDeur();
        }
    }

    void OpenNuDeDeur()
    {
        deurIsAlOpen = true; // Zodat we niet nog eens kunnen drukken
        mijnAnimator.SetTrigger("Open"); // Dit moet exact matchen met je parameter in Stap 3
    }

    // Dit gebeurt als er 'iets' in de Trigger zone loopt
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check of het wel de speler is
        {
            spelerIsInDeBuurt = true;
        }
    }

    // Dit gebeurt als dat 'iets' weer wegloopt
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spelerIsInDeBuurt = false;
        }
    }
}
