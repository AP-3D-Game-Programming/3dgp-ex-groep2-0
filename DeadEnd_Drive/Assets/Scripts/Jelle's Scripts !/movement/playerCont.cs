using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCont : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public Vector3 jump = new Vector3(0, 2f, 0);
    public float jumpForce = 2f;
    public float climbSpeed = 3f;

    [Header("Ground Check")]
    public bool isGrounded;
    private bool hasJumped = false;

    [Header("Ladder/Stairs")]
    public bool onLadderStairs = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Movement relative to player forward
        Vector3 moveDir = new Vector3(h, 0, v);
        Vector3 worldMove = transform.TransformDirection(moveDir).normalized;

        if (onLadderStairs)
        {
            rb.useGravity = false;
            // Horizontal moves normally, vertical controlled by climbSpeed
            rb.linearVelocity = new Vector3(worldMove.x * speed, climbSpeed, worldMove.z * speed);
        }
        else
        {
            rb.useGravity = true;
            // Horizontal moves normally, vertical comes from gravity/jump
            rb.linearVelocity = new Vector3(worldMove.x * speed, rb.linearVelocity.y, worldMove.z * speed);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !hasJumped && !onLadderStairs)
        {
            rb.AddForce(jump * jumpForce, ForceMode.Impulse);
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadderStairs = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadderStairs = false;
        }
    }
}
