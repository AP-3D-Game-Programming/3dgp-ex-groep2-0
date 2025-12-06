using UnityEngine;

public class deurController : MonoBehaviour
{
[Header("Instellingen")]
    public float openHoek = 90f;
    public float draaiSnelheid = 2f;
    
    // NIEUW: Hier slepen we straks de tekst in
    public GameObject uiTekst; 

    private bool isOpen = false;
    private bool spelerInBuurt = false;
    private Quaternion beginRotatie;
    private Quaternion eindRotatie;

    void Start()
    {
        beginRotatie = transform.localRotation;
        eindRotatie = beginRotatie * Quaternion.Euler(0, openHoek, 0);

        // Voor de zekerheid: tekst verbergen bij start
        if (uiTekst != null)
        {
            uiTekst.SetActive(false);
        }
    }

    void Update()
    {
        if (spelerInBuurt && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
            
            // Optioneel: Je kan hier de tekst veranderen als de deur open is
            // Maar voor nu houden we het simpel.
        }

        Quaternion doel = isOpen ? eindRotatie : beginRotatie;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, doel, Time.deltaTime * draaiSnelheid);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spelerInBuurt = true;
            // NIEUW: Zet de tekst AAN
            if (uiTekst != null) uiTekst.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spelerInBuurt = false;
            // NIEUW: Zet de tekst UIT
            if (uiTekst != null) uiTekst.SetActive(false);
        }
    }
}
