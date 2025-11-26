using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Vector3 jump;
    public float jumpForce = 2.0f;
    public bool isGround;
    private bool hasJumped = false; // voorkomt dubbel jump
    Rigidbody rb;

    public float speed = 5f;
    public Transform cameraPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // voorkom dat de speler valt of rolt
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
            hasJumped = true; // zet flag zodat je niet opnieuw kan springen
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0, v).normalized;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraPos.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = rotation;

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            // Physics-safe movement
            rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // check of we echt op de grond staan
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f) // vlakke grond check
            {
                isGround = true;
                hasJumped = false; // reset jump flag zodra we de grond raken
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGround = false;
    }
}
