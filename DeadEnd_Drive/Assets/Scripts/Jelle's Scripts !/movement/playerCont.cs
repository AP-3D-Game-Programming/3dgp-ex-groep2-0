using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCont : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public bool isGrounded;

    [Header("Ladder/Stairs")]
    public bool onLadderStairs;
    public float climbSpeed = 3f;

    [Header("Footstep Audio")]
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

        Vector3 moveDir = transform.TransformDirection(new Vector3(h, 0f, v)).normalized;
        float multiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

        // -------- LADDER --------
        if (onLadderStairs)
        {
            rb.useGravity = false;

            Vector3 climbVelocity = new Vector3(
                moveDir.x * maxSpeed,
                v * climbSpeed,
                moveDir.z * maxSpeed
            );

            rb.linearVelocity = climbVelocity;
            return;
        }
        else
        {
            rb.useGravity = true;
        }

        // -------- FIX: STOP PORTAL FLYING --------
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                Mathf.Min(rb.linearVelocity.y, 0f),
                rb.linearVelocity.z
            );
        }

        // -------- HORIZONTAL MOVEMENT --------
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(0f, rb.linearVelocity.y, 0f),
                0.2f
            );
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 targetVelocity = moveDir * maxSpeed * multiplier;
            Vector3 velocityChange = targetVelocity - horizontalVelocity;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        HandleFootsteps(moveDir.magnitude * maxSpeed * multiplier, multiplier);
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

        AudioClip clip = multiplier > 1f ? runClip : walkClip;

        if (footstepSource.clip != clip)
        {
            footstepSource.clip = clip;
            footstepSource.Play();
        }

        footstepSource.pitch = Random.Range(0.95f, 1.05f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadderStairs = true;
            rb.linearVelocity = Vector3.zero;
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
