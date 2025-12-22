using TMPro;
using UnityEngine;

public class KeyStartCar : MonoBehaviour
{
    public CarEntry carEntry;
    public TextMeshProUGUI Prompt;

    bool playerInside = false;

    void Start()
    {
        if (Prompt)
        {
            Prompt.gameObject.SetActive(false);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if(Prompt)
            {
                Prompt.text = "Press 'E' to pick up car key";
                Prompt.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Prompt.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            if(carEntry) carEntry.UnlockCar();
            
            if(Prompt) Prompt.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
