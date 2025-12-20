using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Pickup : MonoBehaviour
{
    [Header("Instellingen")]
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Referenties")]
    public TMP_Text pickupText;
    public GameObject teleporter;
    
    // De engerd die direct verschijnt
    public GameObject shadowMan; 
    
    // NIEUW: De onzichtbare trigger die we AANZETTEN na oppakken
    public GameObject doorTriggerObject; 

    public Transform player;
    private bool pickedUp = false;
    
    private MeshRenderer meshRenderer;
    private Collider objCollider;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        objCollider = GetComponent<Collider>();
        
        if (teleporter) teleporter.SetActive(false);
        if (shadowMan) shadowMan.SetActive(false);
        
        // Zorg dat de deur-trigger ook UIT staat in het begin
        if (doorTriggerObject) doorTriggerObject.SetActive(false);

        if (player == null) 
        {
            var foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (pickupText) pickupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pickedUp || player == null) return; 

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= pickupRange)
        {
            if (pickupText) pickupText.gameObject.SetActive(true);
            if (Input.GetKeyDown(pickupKey)) HandlePickup();
        }
        else
        {
            if (pickupText) pickupText.gameObject.SetActive(false);
        }
    }

    void HandlePickup()
    {
        pickedUp = true;

        if (pickupText) pickupText.gameObject.SetActive(false);
        if (teleporter) teleporter.gameObject.SetActive(true);

        // 1. Laat het monster DIRECT zien
        if (shadowMan != null) 
        {
            shadowMan.SetActive(true);
            shadowMan.transform.LookAt(player); 
            Vector3 e = shadowMan.transform.rotation.eulerAngles;
            shadowMan.transform.rotation = Quaternion.Euler(0, e.y, 0);
        }

        // 2. Activeer de onzichtbare valstrik bij de deur
        if (doorTriggerObject != null)
        {
            doorTriggerObject.SetActive(true);
        }

        // Maak item onzichtbaar
        if (meshRenderer) meshRenderer.enabled = false;
        if (objCollider) objCollider.enabled = false;
        if (rb) rb.isKinematic = true;
        
        // Vernietig dit object pas na een tijdje (zodat teleporter aan blijft)
        Destroy(gameObject, 5f);
    }
}