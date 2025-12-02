using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Pickup : MonoBehaviour
{
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    public TMP_Text pickupText;
    public Image jumpscareImage;

    private Transform player;
    private bool pickedUp = false;

    public AudioClip clip;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (pickupText) pickupText.gameObject.SetActive(false);
        if (jumpscareImage) jumpscareImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pickedUp) return; // stop logic after pickup

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= pickupRange)
        {
            if (pickupText) pickupText.gameObject.SetActive(true);

            if (Input.GetKeyDown(pickupKey))
            {
                pickedUp = true;

                if (pickupText) pickupText.gameObject.SetActive(false);

                // Hide the pickup visually but keep script alive
                foreach (Transform child in transform)
                    child.gameObject.SetActive(false);

                StartCoroutine(ShowJumpscare());
            }
        }
        else
        {
            if (pickupText) pickupText.gameObject.SetActive(false);
        }
    }

    IEnumerator ShowJumpscare()
    {
        AudioSource src = gameObject.GetComponent<AudioSource>();
        src.PlayOneShot(this.clip);
        jumpscareImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        jumpscareImage.gameObject.SetActive(false);

        Destroy(gameObject); // NOW safe to destroy
    }
}
