using UnityEngine;
using System.Collections;

public class HybridPriestAI_V2 : MonoBehaviour
{
    public enum State { Orbit, Haunt, Flee, JumpScare }
    public State currentState = State.Orbit;

    [Header("Targets")]
    public Transform player;
    public Transform playerCamera;

    [Header("Fail-Safe")]
    public Transform respawnPoint;
    public float fallThresholdY = 1000f;

    [Header("Jump Scare")]
    public bool enableJumpScare = true;
    public AudioClip jumpScareClip;
    public bool isJumpScaring = false;

    [Tooltip("How fast the priest lunges.")]
    public float jumpScareSpeed = 25f;

    [Tooltip("Camera-facing offset (moves target slightly up/down/side).")]
    public Vector3 jumpScareOffset = new Vector3(0f, -0.7f, 0f);

    [Tooltip("How precisely the player must look at the priest to trigger a jumpscare (degrees).")]
    public float jumpScareViewAngle = 20f;

    [Header("Jump Scare Movement (Eggpit-style)")]
    [Tooltip("How close the priest must get to the target before finishing jumpscare. Smaller = more screen-filling.")]
    public float jumpScareStopDistance = 0.10f;

    [Tooltip("How strongly the priest turns to face the camera during the lunge.")]
    public float jumpScareTurnSpeed = 18f;

    [Tooltip("How far in front of the camera the priest aims. Very small values = right in your face.")]
    public float jumpScareFaceOffset = 0.05f;

    [Tooltip("If true, the target is always aligned to the camera forward (recommended).")]
    public bool jumpScareUseCameraForward = true;

    [Tooltip("Optional: temporarily set all colliders to trigger during jumpscare to avoid snagging on geometry.")]
    public bool makeCollidersTriggerDuringJumpScare = true;

    [Tooltip("How long the priest stays stuck in your face before finishing the jumpscare.")]
    public float jumpScareSpookTime = 0.75f;

    private bool _jumpScareReachedFace = false;
    private float _jumpScareSpookTimer = 0f;


    private Vector3 jumpScareTarget;

    [Header("Spawner / Respawn")]
    public PriestManager spawner;
    public PlayerRespawnHandler playerRespawn;

    [Header("Audio")]
    public AudioSource breathSource;
    public AudioSource whisperSource;

    public AudioClip hauntBreathingLoop;
    public AudioClip whisperLine1;
    public AudioClip whisperLine2;

    public float whisperMinDelay = 10f;
    public float whisperMaxDelay = 20f;
    public float specialWhisperCooldown = 15f;

    float whisperTimer;
    float lastSpecialWhisperTime = -999f;

    [Header("Distances")]
    public float orbitRadius = 20f;
    public float toHauntDistance = 18f;
    public float toFleeDistance = 9f;

    [Header("Speeds")]
    public float orbitSpeed = 25f;
    public float hauntSpeed = 4f;
    public float fleeSpeed = 30f;

    [Header("Haunt Behaviour")]
    [Range(-1f, 1f)]
    public float lookDotThreshold = 0.4f;
    public float hauntStopDistance = 10f;
    public float overhang = 4f;

    [Header("Haunt Delay")]
    public float hauntDelay = 1f;
    float hauntDelayTimer = 0f;

    [Header("Flee Behaviour")]
    public bool enableFleeBehavior = false;
    public bool requirePlayerMoving = false;
    public float playerMoveThreshold = 0.15f;
    public float fleeDistance = 150f;

    [Header("Return / Come-back Settings")]
    public bool useFleeTimer = true;
    public float fleeTime = 10f;

    [Header("Floating / Ground")]
    public float hoverHeight = 1.5f;
    public float floatAmplitude = 0.4f;
    public float floatFrequency = 1.5f;
    public float raycastHeight = 10f;
    public float raycastDistance = 30f;
    public LayerMask groundLayers = ~0;

    float orbitAngle;
    float floatPhase;
    Vector3 fleeTarget;
    float fleeTimer;

    Vector3 lastPlayerPos;
    bool hasLastPlayerPos = false;
    float playerSpeed;

    // collider trigger swap (optional)
    Collider[] _cols;
    bool[] _colsWasTrigger;
    bool _jumpTriggerApplied;

    float FlatDistanceToPlayer()
    {
        Vector3 a = player.position;
        Vector3 b = transform.position;
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
        floatPhase = Random.Range(0f, 10f);
        whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);

        if (playerRespawn == null)
            playerRespawn = Object.FindFirstObjectByType<PlayerRespawnHandler>();

        // cache colliders for optional trigger swap
        _cols = GetComponentsInChildren<Collider>(true);
        _colsWasTrigger = new bool[_cols.Length];
        for (int i = 0; i < _cols.Length; i++)
            _colsWasTrigger[i] = _cols[i].isTrigger;
    }

    void Update()
    {
        if (!player || !playerCamera) return;

        UpdatePlayerSpeed();

        if (enableJumpScare && !isJumpScaring)
            TryJumpScareTrigger();

        // Eggpit-style jumpscare movement
        if (currentState == State.JumpScare)
        {
            DoJumpScareMoveEggpitStyle();
            return;
        }

        // ---- Freeze logic (ONLY in Orbit & Haunt) ----
        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        bool playerLooking = Vector3.Dot(playerCamera.forward.normalized, toPriest) > lookDotThreshold;
        float distToPlayer = FlatDistanceToPlayer();

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
                hauntDelayTimer += Time.deltaTime;

                if (enableFleeBehavior && distToPlayer < toFleeDistance && ShouldFleeFromPlayer())
                    EnterFleeState();

                if (hauntDelayTimer >= hauntDelay)
                {
                    currentState = State.Haunt;
                    hauntDelayTimer = 0f;
                }
                StopHauntAudio();
                break;

            case State.Haunt:
                StartHauntAudio();
                DoWhisperLogic();

                if (enableFleeBehavior && distToPlayer < toFleeDistance && ShouldFleeFromPlayer())
                    EnterFleeState();
                else if (distToPlayer > toHauntDistance * 1.3f)
                {
                    currentState = State.Orbit;
                    hauntDelayTimer = 0f;
                }
                break;

            case State.Flee:
                StopHauntAudio();
                if (useFleeTimer)
                {
                    fleeTimer -= Time.deltaTime;
                    if (fleeTimer <= 0f)
                    {
                        currentState = State.Orbit;
                        hauntDelayTimer = 0f;
                    }
                }
                break;
        }

        // ---------- STATE BEHAVIOUR ----------
        switch (currentState)
        {
            case State.Orbit: DoOrbit(); break;
            case State.Haunt: DoHaunt(); break;
            case State.Flee: DoFlee(); break;
        }

        ApplyFloating();
    }

    // ================== JUMPSCARE ==================

    void DoJumpScareMoveEggpitStyle()
    {
        if (!playerCamera) return;

        if (makeCollidersTriggerDuringJumpScare && !_jumpTriggerApplied)
            SetAllCollidersTrigger(true);

        Vector3 camPos = playerCamera.position;

        // Stick target to camera every frame (this is the "stuff the screen" behaviour)
        Vector3 forward = playerCamera.forward;
        if (!jumpScareUseCameraForward)
            forward = (camPos - transform.position).normalized;

        jumpScareTarget = camPos + forward * jumpScareFaceOffset + jumpScareOffset;

        // Turn aggressively to face camera
        Vector3 aimDir = (camPos - transform.position);
        if (aimDir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(aimDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * jumpScareTurnSpeed);
        }

        // Move in
        transform.position = Vector3.MoveTowards(transform.position, jumpScareTarget, jumpScareSpeed * Time.deltaTime);

        // Finish
        float d = Vector3.Distance(transform.position, jumpScareTarget);

        if (!_jumpScareReachedFace)
        {
            if (d <= jumpScareStopDistance)
            {
                _jumpScareReachedFace = true;
                _jumpScareSpookTimer = jumpScareSpookTime;
            }
        }
        else
        {
            // STUCK IN YOUR FACE: keep him glued to the camera target
            transform.position = jumpScareTarget;

            _jumpScareSpookTimer -= Time.deltaTime;
            if (_jumpScareSpookTimer <= 0f)
            {
                FinishJumpScare();
            }
        }

    }

    void TryJumpScareTrigger()
    {
        if (!enableJumpScare || isJumpScaring) return;
        if (!playerCamera) return;

        float dist = FlatDistanceToPlayer();

        // Too far? No jumpscare!
        if (dist > toHauntDistance + 1f) return;

        // Too close? Don't glitch into the player's face from inside them
        if (dist < 2f) return;

        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward.normalized, toPriest);
        float cosThreshold = Mathf.Cos(jumpScareViewAngle * Mathf.Deg2Rad);

        if (dot < cosThreshold) return;

        StartJumpScare();
    }

    void StartJumpScare()
    {
        isJumpScaring = true;
        currentState = State.JumpScare;

        // reset linger / stuck-in-face state
        _jumpScareReachedFace = false;
        _jumpScareSpookTimer = jumpScareSpookTime;

        if (breathSource) breathSource.Stop();
        if (whisperSource && jumpScareClip)
            whisperSource.PlayOneShot(jumpScareClip);
    }


    void FinishJumpScare()
    {
        // -------- RESET INTERNAL STATE --------
        isJumpScaring = false;
        hauntDelayTimer = 0f;
        fleeTimer = fleeTime;
        floatPhase = Random.Range(0f, 10f);

        // Reset & choose flee target (so he doesn’t hover in place)
        Vector3 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.01f)
            awayDir = -player.forward;

        Vector3 randomSide = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        awayDir = (awayDir + randomSide).normalized;
        fleeTarget = transform.position + awayDir * fleeDistance;

        if (makeCollidersTriggerDuringJumpScare)
            SetAllCollidersTrigger(false);

        // -------- DESPAWN + PLAYER RESPAWN --------
        if (spawner) spawner.DespawnPriest();
        if (playerRespawn) playerRespawn.RespawnPlayer();
        else Debug.LogWarning("PlayerRespawnHandler not assigned!");

        currentState = State.Flee;
    }

    void SetAllCollidersTrigger(bool trigger)
    {
        if (_cols == null) return;

        if (trigger)
        {
            for (int i = 0; i < _cols.Length; i++)
            {
                if (!_cols[i]) continue;
                _colsWasTrigger[i] = _cols[i].isTrigger;
                _cols[i].isTrigger = true;
            }
            _jumpTriggerApplied = true;
        }
        else
        {
            for (int i = 0; i < _cols.Length; i++)
            {
                if (!_cols[i]) continue;
                _cols[i].isTrigger = _colsWasTrigger[i];
            }
            _jumpTriggerApplied = false;
        }
    }

    // ================== GENERAL LOGIC ==================

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

        Vector3 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.01f)
            awayDir = -player.forward;

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
        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        Vector3 camForward = playerCamera.forward.normalized;
        bool playerLooking = Vector3.Dot(camForward, toPriest) > lookDotThreshold;

        float dist = FlatDistanceToPlayer();

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

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers))
        {
            Debug.LogWarning("Priest lost ground. Respawning...");
            RespawnToSafePoint(startInFlee: true);
            return;
        }

        float groundY = hit.point.y;
        float bob = Mathf.Sin((Time.time + floatPhase) * floatFrequency) * floatAmplitude;

        pos.y = groundY + hoverHeight + bob;
        transform.position = pos;
    }

    void StartHauntAudio()
    {
        if (!breathSource || !hauntBreathingLoop) return;
        if (breathSource.clip == hauntBreathingLoop && breathSource.isPlaying) return;

        breathSource.clip = hauntBreathingLoop;
        breathSource.loop = true;
        breathSource.Play();
    }

    void StopHauntAudio()
    {
        if (!breathSource) return;
        if (breathSource.clip == hauntBreathingLoop)
            breathSource.Stop();
    }

    bool PlayerLookingAtPriest()
    {
        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        return Vector3.Dot(playerCamera.forward.normalized, toPriest) > lookDotThreshold;
    }

    void PlayRandomWhisper()
    {
        if (!whisperSource) return;

        AudioClip clip = null;
        if (whisperLine1 != null && whisperLine2 != null)
            clip = (Random.value > 0.5f) ? whisperLine1 : whisperLine2;
        else if (whisperLine1 != null)
            clip = whisperLine1;
        else if (whisperLine2 != null)
            clip = whisperLine2;

        if (clip == null) return;

        whisperSource.pitch = Random.Range(0.95f, 1.05f);
        whisperSource.PlayOneShot(clip);
    }

    void PlayWhisperLine1()
    {
        if (!whisperSource || whisperLine1 == null) return;

        if (!whisperSource.isPlaying)
        {
            whisperSource.pitch = 1f;
            whisperSource.PlayOneShot(whisperLine1);
        }
    }

    void DoWhisperLogic()
    {
        if (!whisperSource) return;

        if (!PlayerLookingAtPriest())
        {
            whisperTimer -= Time.deltaTime;
            if (whisperTimer <= 0f)
            {
                PlayRandomWhisper();
                whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);
            }

            float dist = FlatDistanceToPlayer();
            if (dist < hauntStopDistance + 1.5f &&
                Time.time - lastSpecialWhisperTime > specialWhisperCooldown)
            {
                PlayWhisperLine1();
                lastSpecialWhisperTime = Time.time;
            }
        }
    }

    public void RespawnToSafePoint(bool startInFlee = false)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("No respawn point set for HybridPriestAI_V2.");
            return;
        }

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        isJumpScaring = false;
        hauntDelayTimer = 0f;

        if (makeCollidersTriggerDuringJumpScare)
            SetAllCollidersTrigger(false);

        if (startInFlee)
        {
            currentState = State.Flee;
            fleeTimer = fleeTime;

            Vector3 awayDir = (transform.position - player.position).normalized;
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            awayDir = (awayDir + randomOffset).normalized;

            fleeTarget = transform.position + awayDir * fleeDistance;

            Debug.Log("Priest respawned into Flee state.");
        }
        else
        {
            currentState = State.Orbit;
            Debug.Log("Priest respawned into Orbit state.");
        }
    }
}
