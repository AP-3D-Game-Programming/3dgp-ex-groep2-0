using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class QuadController : MonoBehaviour
{

    GameObject player;
    PlayerCont playerControllerScript;
    Rigidbody rb;
    public float speed = 200000f;       // snelheid
    public float rotationSpeed = 150f;   //draai snelheid
    public bool isOnQuad;
    public Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        playerControllerScript = player.GetComponent<PlayerCont>();
        rb = gameObject.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {


        if (CheckForPlayer() && Input.GetKeyDown(KeyCode.E))
        {
            if (isOnQuad)
                GetOffQuad();
            else if (!isOnQuad)
                GetOnQuad();
        }
    }

    private void FixedUpdate()
    {
              if (isOnQuad)
        {
            HandleMovement();
        }  
    }

    void GetOnQuad()
    {
        isOnQuad = true;
        playerControllerScript.AddQuad(gameObject);
    }
    void GetOffQuad()
    {
        isOnQuad = false;
        playerControllerScript.DeleteQuad();
    }

    private bool CheckForPlayer()
    {
        float distance = Vector3.Distance(transform.position, playerControllerScript.transform.position);

        if (distance < 4f)
        {
            if (!isOnQuad)
            {
                text.gameObject.SetActive(true);
                text.text = "Press E to mount quad";
            }
            else
            {
                text.gameObject.SetActive(true);
                text.text = "Press E to dismount quad";
            }
                return true;
        }
        text.gameObject.SetActive(false);
        return false;
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 1. Beweging: Gebruik VelocityChange
        // speed mag nu terug naar 10 of 20 (probeer in de Inspector)
        Vector3 targetVelocity = -transform.up * v * speed;

        // Bereken het verschil met de huidige snelheid en pas toe.
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;

        // Pas de kracht toe om dit snelheidsverschil te bereiken
        // (Negeert massa van 100)
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // 2. Rotatie
        float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, 0f, turnAmount);
    }
}

#region PlayerController
/*
    private GameObject quad;


in update
-----------------------------------------------------------------------------------------------
        if (!quad) { 
            HandleJump();                        // check sprong per frame
        }
-----------------------------------------------------------------------------------------------


in fixed update
-----------------------------------------------------------------------------------------------

        if (!quad)
        {
            HandleMovement();               // fysiek correcte beweging
        }
        else
        {
            transform.position = quad.transform.position + (Vector3.up * 1.5f);
        }
-----------------------------------------------------------------------------------------------


extra methods
-----------------------------------------------------------------------------------------------

    public void AddQuad(GameObject quad)
    {
        this.quad = quad;
        // 1. Schakel de Rigidbody uit
        rb.isKinematic = true;
        // 2. Schakel de Collider uit
        GetComponent<Collider>().enabled = false;
    }

    public void DeleteQuad()
    {
        this.quad = null;
        // 1. Schakel de Rigidbody weer in
        rb.isKinematic = false;
        // 2. Schakel de Collider weer in
        GetComponent<Collider>().enabled = true;

        // Optioneel: Plaats de speler een stukje boven de quad om meteen uit de collider te zijn
        transform.position += Vector3.up * 0.5f;
    }
-----------------------------------------------------------------------------------------------



*/
#endregion