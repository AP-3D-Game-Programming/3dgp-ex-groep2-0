using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 10f;                    // loopsnelheid
    public float jumpForce = 5f;                 // springkracht
    public Transform cameraPos;                  // alleen voor FOV/positie, niet voor movement

    private Rigidbody rb;
    private bool isGrounded;
    private bool hasJumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();          // haalt rigidbody op
        rb.freezeRotation = true;                // voorkomt dat physics de speler kan draaien
    }

    void Update()
    {
        HandleJump();                            // check sprong per frame
    }

    void FixedUpdate()
    {
        HandleMovement();                        // fysiek correcte beweging
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");   // A/D input
        float v = Input.GetAxis("Vertical");     // W/S input

        // ⛔ NIET camera.forward gebruiken → veroorzaakt spin bug
        // ✔ Gebruikt spelerrotatie (player draait door muis ↔ camera niet door physics)
        Vector3 move = (transform.forward * v + transform.right * h);

        move.y = 0;                              // geen verticale beweging

        if (move.magnitude > 1f)
            move.Normalize();                    // voorkomt sprint exploit diagonaal

        // behoud verticale snelheid (gravity)
        rb.linearVelocity = move * speed + new Vector3(0, rb.linearVelocity.y, 0);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !hasJumped)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            hasJumped = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                hasJumped = false;
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
