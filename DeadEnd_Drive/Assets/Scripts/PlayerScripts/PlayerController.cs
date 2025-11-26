using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Vector3 jump;
    public float jumpForce = 2.0f;
    public bool isGround;
    private bool hasJumped = false;
    Rigidbody rb;

    public float speed = 5f;
    public Transform cameraPos; // sleep hier de Camera in

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jump = new Vector3(0.0f, 2.0f, 0.0f);
    }

    void Update()
    {
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && !hasJumped)
        {
            rb.AddForce(jump * jumpForce, ForceMode.Impulse);
            hasJumped = true;
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // Richting gebaseerd op camera waar je naartoe kijkt
        Vector3 move = (cameraPos.forward * v + cameraPos.right * h);
        move.y = 0; // Geen verticale beweging

        if (move.magnitude > 1f)
            move.Normalize();

        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGround = true;
                hasJumped = false;
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGround = false;
    }
}
