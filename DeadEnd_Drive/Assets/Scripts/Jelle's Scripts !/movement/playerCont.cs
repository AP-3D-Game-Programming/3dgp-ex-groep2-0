using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCont : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public Vector3 jump = new Vector3(0, 2f, 0);
    public float jumpForce = 2f;

    [Header("Ground Check")]
    public bool isGrounded;
    private bool hasJumped = false;

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

        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            Vector3 worldMove = transform.TransformDirection(moveDir);
            rb.MovePosition(rb.position + worldMove * speed * Time.fixedDeltaTime);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !hasJumped)
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
}
