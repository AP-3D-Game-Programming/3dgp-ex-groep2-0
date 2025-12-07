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
    public float stepIntervalWalk = 0.5f;
    public float stepIntervalRun = 0.35f;

    private float stepTimer = 0f;

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
        Vector3 moveDir = transform.TransformDirection(new Vector3(h, 0, v));

        float currentMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

        if (onLadderStairs)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(
                moveDir.x * maxSpeed,
                climbSpeed,
                moveDir.z * maxSpeed
            );
            return;
        }
        else
        {
            rb.useGravity = true;
        }

        rb.AddForce(moveDir * moveForce * currentMultiplier);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed * currentMultiplier)
        {
            Vector3 limited = horizontalVelocity.normalized * maxSpeed * currentMultiplier;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }
        HandleFootsteps(horizontalVelocity.magnitude, currentMultiplier);
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

}
