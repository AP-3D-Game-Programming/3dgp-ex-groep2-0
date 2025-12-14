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
    public DoorController doorController;

    public Transform player;
    private bool pickedUp = false;
    private Rigidbody rb;

    public AudioClip clip;
    public GameObject teleporter;

    // Voeg deze toe om het object onzichtbaar te maken
    private MeshRenderer meshRenderer;
    private Collider objCollider;

    void Start()
    {
        teleporter.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody>();
        
        // We pakken de renderer en collider om ze straks uit te zetten
        meshRenderer = GetComponent<MeshRenderer>();
        objCollider = GetComponent<Collider>();

        // Jouw veilige player check
        if (player == null) 
        {
            var foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (pickupText) pickupText.gameObject.SetActive(false);
        if (jumpscareImage) jumpscareImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Als we hem al hebben opgepakt, doen we NIETS meer in Update.
        // De Coroutine handelt de rest af.
        if (pickedUp) return; 

        if (player != null) // Extra check om crash te voorkomen
        {
            float dist = Vector3.Distance(player.position, transform.position);

            if (dist <= pickupRange)
            {
                if (pickupText) pickupText.gameObject.SetActive(true);

                if (Input.GetKeyDown(pickupKey))
                {
                    HandlePickup(); // We maken een aparte functie voor netheid
                }
            }
            else
            {
                if (pickupText) pickupText.gameObject.SetActive(false);
            }
        }
    }

    void HandlePickup()
    {
        pickedUp = true;

        // 1. Tekst weg
        if (pickupText) pickupText.gameObject.SetActive(false);

        // 2. Teleporter aan
        if (teleporter) teleporter.gameObject.SetActive(true);

        // 3. Deur dicht
        if (doorController) doorController.SlamShut(1.5f);

        // 4. BELANGRIJK: Maak object onzichtbaar, maar zet het NIET uit!
        if (meshRenderer) meshRenderer.enabled = false; // Plaatje weg
        if (objCollider) objCollider.enabled = false;   // Botsing weg
        if (rb) rb.isKinematic = true; // Zorg dat hij niet meer valt

        // 5. Start de jumpscare correct
        StartCoroutine(ShowJumpscare());
    }

    IEnumerator ShowJumpscare()
    {
        // Geluid afspelen
        AudioSource src = GetComponent<AudioSource>();
        if (src != null && clip != null)
        {
            src.PlayOneShot(clip);
        }

        // Plaatje tonen
        if (jumpscareImage) jumpscareImage.gameObject.SetActive(true);

        // Wacht 1 seconde (terwijl het script nog leeft!)
        yield return new WaitForSeconds(1f);

        // Plaatje weer weg
        if (jumpscareImage) jumpscareImage.gameObject.SetActive(false);

        // NU pas vernietigen we het object definitief
        Destroy(gameObject); 
    }
}