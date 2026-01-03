using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
[Header("Sleep hier je UI Tekst object in")]
    public GameObject tekstObject;

    // Zorg ervoor dat de tekst zeker uit staat als de game begint
    void Start()
    {
        if(tekstObject != null)
        {
            tekstObject.SetActive(false);
        }
    }

    // Dit gebeurt er als iets (de speler) de kubus binnenloopt
    void OnTriggerEnter(Collider other)
    {
        // We checken of het object dat binnenloopt de tag "Player" heeft
        if (other.CompareTag("Player"))
        {
            tekstObject.SetActive(true);
        }
    }

    // Dit gebeurt er als de speler de kubus weer verlaat
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tekstObject.SetActive(false);
        }
    }
}
