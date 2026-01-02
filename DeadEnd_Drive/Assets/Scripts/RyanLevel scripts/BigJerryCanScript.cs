using System.Collections;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class BigJerryCanScript : MonoBehaviour
{
    private bool triggered = false;
    public GameObject Jerrycan;
    public TextMeshProUGUI dialog;
    public Image blackscreen;
    private AudioSource src;

    void Start()
    {
        if (Jerrycan != null)
            Jerrycan.SetActive(false);
        src = gameObject.GetComponent<AudioSource>();

    }
    private void Update()
    {
        if (triggered)
            dialog.alpha = 1f;
    }

    // De Update methode is hier verwijderd omdat deze de Coroutine tegenwerkte!

    private void OnTriggerEnter(Collider other)
    {
        // Check of de player de trigger raakt en we nog niet gestart zijn
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // Zoek de controller en zet deze uit
            PlayerCont controller = other.GetComponent<PlayerCont>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            StartCoroutine(WaitAndRotate(controller));
        }
    }

    IEnumerator WaitAndRotate(PlayerCont controller)
    {
        if (Jerrycan != null)
            Jerrycan.SetActive(true);


        dialog.gameObject.SetActive(true);

        dialog.text = @"WHAT";
        yield return new WaitForSeconds(1f);
        dialog.text = @"WHAT    THE";


        yield return new WaitForSeconds(2f);

        // Definieer de start- en eindrotatie
        Quaternion startRotation = Jerrycan.transform.rotation;
        // We voegen 90 graden toe aan de huidige Z-as
        Quaternion endRotation = Jerrycan.transform.rotation * Quaternion.Euler(0, 0, 90f);

        float duration = 1.5f; // Hoeveel seconden de animatie duurt
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Bereken hoe ver we zijn (tussen 0 en 1)
            float percent = elapsed / duration;

            // Draai soepel van start naar eind
            Jerrycan.transform.rotation = Quaternion.Lerp(startRotation, endRotation, percent);


            yield return null; // Wacht tot het volgende frame
        }
            blackscreen.gameObject.SetActive(true);
        src.Play();


        UnityEngine.SceneManagement.SceneManager.LoadScene("5. Level2Mist");
    }
}