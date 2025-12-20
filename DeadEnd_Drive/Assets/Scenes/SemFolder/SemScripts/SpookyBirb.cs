using System.Collections;
using UnityEngine;

public class SpookyBird : MonoBehaviour
{
    [Header("Sleep je 5 geluidsbestanden hierin")]
    public AudioClip[] vogelGeluiden;

    [Header("Instellingen voor timing (in seconden)")]
    public float minimumWachttijd = 30f; // Bijv. minimaal halve minuut stilte
    public float maximumWachttijd = 120f; // Bijv. maximaal 2 minuten stilte

    private AudioSource audioSource;

    void Start()
    {
        // Zoek de AudioSource op dit object
        audioSource = GetComponent<AudioSource>();
        
        // Start de oneindige loop
        StartCoroutine(SpeelGeluidAfEnToe());
    }

    IEnumerator SpeelGeluidAfEnToe()
    {
        while (true) // Blijf dit voor altijd herhalen
        {
            // Stap 1: Bepaal een willekeurige wachttijd
            float wachttijd = Random.Range(minimumWachttijd, maximumWachttijd);
            
            // Wacht...
            yield return new WaitForSeconds(wachttijd);

            // Stap 2: Kies een willekeurig geluid uit de lijst
            if (vogelGeluiden.Length > 0)
            {
                int willekeurigeIndex = Random.Range(0, vogelGeluiden.Length);
                audioSource.clip = vogelGeluiden[willekeurigeIndex];
                
                // Stap 3: Varieer eventueel de toonhoogte een heel klein beetje voor extra engheid
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                
                audioSource.Play();
            }
        }
    }
}