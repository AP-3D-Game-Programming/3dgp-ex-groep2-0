using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 10f;         // beweging snelheid
    public float jumpForce = 5f;      // sprongkracht
    public Transform cameraPos;       // sleep hier de Camera in

    private Rigidbody rb;
    private bool isGrounded;
    private bool hasJumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // Richting gebaseerd op camera
        Vector3 move = (cameraPos.forward * v + cameraPos.right * h);
        move.y = 0;

        if (move.magnitude > 1f)
            move.Normalize();

        // Vloeiende beweging via velocity, behoud Y-velocity voor springen
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
