using UnityEngine;

public class HybridPriestAI : MonoBehaviour
{
    public enum State { Orbit, Haunt, Flee }
    public State currentState = State.Orbit;

    [Header("Targets")]
    public Transform player;
    public Transform playerCamera;

    [Header("Distances")]
    public float orbitRadius = 8f;
    public float toHauntDistance = 20f;   // closer than this -> Haunt
    public float toFleeDistance = 3f;     // TOO close -> Flee

    [Header("Speeds")]
    public float orbitSpeed = 25f;
    public float hauntSpeed = 3f;
    public float fleeSpeed = 10f;

    [Header("Haunt Behaviour")]
    [Range(-1f, 1f)]
    public float lookDotThreshold = 0.6f; // 1 = straight at priest
    public float hauntStopDistance = 2f;  // how close Haunt is allowed to get
    public float overhang = 5f;

    [Header("Haunt Delay")]
    public float hauntDelay = 3f;  // time from orbit to haunt
    float hauntDelayTimer = 0f;    // ✅ start from 0 so first haunt is delayed

    [Header("Flee Behaviour")]
    public bool enableFleeBehavior = true;   // master toggle
    public bool requirePlayerMoving = true;  // only flee if player moves
    public float playerMoveThreshold = 0.15f;
    public float fleeDistance = 60f;         // how far it runs away

    [Header("Return / Come-back Settings")]
    public bool useFleeTimer = true;
    public float fleeTime = 5f;              // how long it stays in Flee before coming back

    [Header("Floating / Ground")]
    public float hoverHeight = 2f;
    public float floatAmplitude = 0.4f;
    public float floatFrequency = 1.5f;
    public float raycastHeight = 10f;
    public float raycastDistance = 30f;
    public LayerMask groundLayers = ~0;

    float orbitAngle;
    float floatPhase;
    Vector3 fleeTarget;
    float fleeTimer;

    // --- NEW: track player movement based on position, not Rigidbody ---
    Vector3 lastPlayerPos;
    bool hasLastPlayerPos = false;
    float playerSpeed;

    float FlatDistanceToPlayer()
    {
        Vector3 a = player.position;
        Vector3 b = transform.position;
        a.y = b.y = 0f; // ignore height
        return Vector3.Distance(a, b);
    }

    void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
        floatPhase = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (!player || !playerCamera) return;

        UpdatePlayerSpeed();

        // ---- GLOBAL FREEZE CHECK ----
        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        bool playerLooking = Vector3.Dot(playerCamera.forward.normalized, toPriest) > lookDotThreshold;
        float distToPlayer = FlatDistanceToPlayer();

        // ✅ Only freeze in Orbit & Haunt when NOT too close (so flee can still trigger)
        if ((currentState == State.Orbit || currentState == State.Haunt) &&
            playerLooking && distToPlayer > toFleeDistance)
        {
            transform.LookAt(player.position + Vector3.up * 1.5f);
            transform.rotation *= Quaternion.Euler(overhang, 0f, 0f);
            ApplyFloating();
            return;
        }

        // ---------- STATE SWITCHING ----------
        switch (currentState)
        {
            case State.Orbit:
                // Count up timer every frame in Orbit mode
                hauntDelayTimer += Time.deltaTime;

                // Timer reached haunt delay → switch state
                if (hauntDelayTimer >= hauntDelay)
                {
                    currentState = State.Haunt;
                    hauntDelayTimer = 0f; // reset for next time
                }
                break;

            case State.Haunt:
                if (enableFleeBehavior && distToPlayer < toFleeDistance && ShouldFleeFromPlayer())
                {
                    EnterFleeState();
                }
                else if (distToPlayer > toHauntDistance * 1.3f)
                {
                    currentState = State.Orbit;
                    hauntDelayTimer = 0f;   // ✅ ensure new haunt delay whenever we return to Orbit
                }
                break;

            case State.Flee:
                if (useFleeTimer)
                {
                    fleeTimer -= Time.deltaTime;
                    if (fleeTimer <= 0f)
                    {
                        currentState = State.Orbit;
                        hauntDelayTimer = 0f;   // ✅ after fleeing, wait again before haunting
                    }
                }
                break;
        }

        // ---------- STATE BEHAVIOUR ----------
        switch (currentState)
        {
            case State.Orbit: DoOrbit(); break;
            case State.Haunt: DoHaunt(); break;
            case State.Flee:  DoFlee();  break;
        }

        ApplyFloating();
    }

    // ================== LOGIC ==================

    void UpdatePlayerSpeed()
    {
        if (!hasLastPlayerPos)
        {
            lastPlayerPos = player.position;
            hasLastPlayerPos = true;
            playerSpeed = 0f;
            return;
        }

        float dist = Vector3.Distance(player.position, lastPlayerPos);
        playerSpeed = dist / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPlayerPos = player.position;
    }

    bool ShouldFleeFromPlayer()
    {
        if (!requirePlayerMoving) return true;
        return playerSpeed > playerMoveThreshold;
    }

    void EnterFleeState()
    {
        currentState = State.Flee;
        fleeTimer = fleeTime;

        // run away from the player, opposite direction
        Vector3 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.01f)
            awayDir = -player.forward; // fallback

        // little random spread so it doesn't always go in exact same line
        Vector3 randomSide = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        awayDir = (awayDir + randomSide).normalized;

        fleeTarget = player.position + awayDir * fleeDistance;
    }

    void DoOrbit()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad) * orbitRadius, 0f, Mathf.Sin(rad) * orbitRadius);
        Vector3 targetPos = player.position + offset;
        targetPos.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3f);
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    void DoHaunt()
    {
        // Is player looking at us?
        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        Vector3 camForward = playerCamera.forward.normalized;
        bool playerLooking = Vector3.Dot(camForward, toPriest) > lookDotThreshold;

        float dist = FlatDistanceToPlayer();

        // if player is moving, don't creep closer (just stare)
        if (requirePlayerMoving && playerSpeed > playerMoveThreshold)
        {
            transform.LookAt(player.position + Vector3.up * 1.5f);
            return;
        }

        if (playerLooking)
        {
            transform.LookAt(player.position + Vector3.up * 1.5f);
            return;
        }

        if (dist > hauntStopDistance)
        {
            Vector3 targetPos = player.position;
            targetPos.y = transform.position.y;

            Vector3 dir = (targetPos - transform.position).normalized;
            transform.position += dir * hauntSpeed * Time.deltaTime;
            transform.LookAt(player.position + Vector3.up * 1.5f);
        }
    }

    void DoFlee()
    {
        Vector3 dir = (fleeTarget - transform.position).normalized;
        transform.position += dir * fleeSpeed * Time.deltaTime;
    }

    void ApplyFloating()
    {
        Vector3 pos = transform.position;
        Vector3 rayOrigin = new Vector3(pos.x, pos.y + raycastHeight, pos.z);

        float groundY = pos.y;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers))
            groundY = hit.point.y;

        float bob = Mathf.Sin((Time.time + floatPhase) * floatFrequency) * floatAmplitude;

        pos.y = groundY + hoverHeight + bob;
        transform.position = pos;
    }
}
