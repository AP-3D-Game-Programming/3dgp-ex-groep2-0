using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCont : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 50f;
    public float maxSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public bool isGrounded = false;

    [Header("Ladder/Stairs")]
    public bool onLadderStairs = false;
    public float climbSpeed = 3f;

    [Header("footstep audio")]
    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip runClip;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        footstepSource.loop = true;
        footstepSource.clip = walkClip;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        HandleJump();
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // direction relative to player
        Vector3 moveDir = transform.TransformDirection(new Vector3(h, 0, v)).normalized;

        float currentMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

        if (onLadderStairs)
        {
            rb.useGravity = false;

            // vertical movement based on input
            float verticalInput = Input.GetAxis("Vertical"); // W/S or Up/Down

            Vector3 climbVelocity = new Vector3(
                moveDir.x * maxSpeed,        // horizontal X
                verticalInput * climbSpeed,  // vertical Y
                moveDir.z * maxSpeed         // horizontal Z
            );

            rb.linearVelocity = climbVelocity;
            return;
        }
        else
        {
            rb.useGravity = true;
        }

        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
        else
        {
            // Velocity-based movement
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 targetVelocity = moveDir * maxSpeed * currentMultiplier;
            Vector3 velocityChange = targetVelocity - horizontalVelocity;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
        }

        HandleFootsteps(moveDir.magnitude * maxSpeed * currentMultiplier, currentMultiplier);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !onLadderStairs)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint c in collision.contacts)
        {
            if (Vector3.Dot(c.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    void HandleFootsteps(float speed, float multiplier)
    {
        if (!isGrounded || speed < 0.2f || onLadderStairs)
        {
            footstepSource.Pause();
            return;
        }

        bool isRunning = multiplier > 1f;
        AudioClip clip = isRunning ? runClip : walkClip;

        if (footstepSource.clip != clip)
        {
            footstepSource.clip = clip;
            footstepSource.Play();
        }

        footstepSource.pitch = Random.Range(0.95f, 1.05f);
        if (!footstepSource.isPlaying)
            footstepSource.Play();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadderStairs = true;
            rb.linearVelocity = Vector3.zero; // stop any existing momentum
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
