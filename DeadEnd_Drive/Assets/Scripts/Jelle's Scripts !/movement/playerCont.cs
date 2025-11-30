using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCont : MonoBehaviour
{
    public Vector3 jump;
    public float jumpForce = 2.0f;
    public bool isGround;
    private bool hasJumped = false;
    Rigidbody rb;

    public float speed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
        float h = Input.GetAxis("Horizontal"); // left/right
        float v = Input.GetAxis("Vertical");   // forward/back

        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            // Only move, no player rotation based on camera
            Vector3 worldMove = transform.TransformDirection(moveDir);
            rb.MovePosition(rb.position + worldMove * speed * Time.fixedDeltaTime);
        }
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
