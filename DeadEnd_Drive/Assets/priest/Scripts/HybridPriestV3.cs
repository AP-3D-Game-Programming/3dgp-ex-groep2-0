using UnityEngine;
using System.Collections;

public class HybridPriestAI_V3 : MonoBehaviour
{
    public enum State { Orbit, Haunt, Flee, JumpScare }
    public State currentState = State.Orbit;

    [Header("Targets")]
    public Transform player;
    public Transform playerCamera;

    [Header("Fail-Safe")]
    public Transform respawnPoint;
    public float fallThresholdY = 1000f;

    // ================== JUMPSCARE TRIGGER (KEEP YOUR WORKING PART) ==================

    [Header("Jump Scare Trigger")]
    public bool enableJumpScare = true;
    public AudioClip jumpScareClip;
    public bool isJumpScaring = false;

    [Tooltip("How precisely the player must look at the priest to trigger a jumpscare (degrees).")]
    public float jumpScareViewAngle = 20f;

    // ================== EGG PIT STYLE MOVEMENT (REPLACES YOUR MOVE-TO-CAMERA) ==================

    [Header("Jump Scare Movement (Eggpit literal)")]
    [Tooltip("Drag a head/face bone or an empty above priest's head. Used as move/aim origin.")]
    public Transform detectPoint;

    [Tooltip("Optional detection offset (NOT used for movement origin; only used for look checks if you want).")]
    public Vector3 detectOffset = new Vector3(0f, 0.25f, 0f);

    [Tooltip("Attack move speed.")]
    public float attackMoveSpeed = 25f;

    [Tooltip("Max time the lunge is allowed to run.")]
    public float maxAttackTime = 0.7f;

    [Tooltip("Stop when priest root gets within this distance of the player's head.")]
    public float stopDistance = 0.35f;

    [Tooltip("How strongly the priest turns to face the target during lunge.")]
    public float attackTurnSpeed = 18f;

    [Tooltip("Keep movement mostly horizontal (Eggpit style).")]
    public bool lockY = true;

    [Header("No Collision During Attack")]
    public bool makeCollidersTriggerDuringAttack = true;

    [Header("Spook / Linger")]
    [Tooltip("How long the priest stays in your face before despawn/respawn logic runs.")]
    public float spookTime = 0.75f;

    [Tooltip("Extra offset applied to the target head position during the 'stuck in face' linger.")]
    public Vector3 faceStickOffset = new Vector3(0f, -0.05f, 0f);

    // ================== YOUR EXISTING SYSTEM (KEEP) ==================

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

    Vector3 _jumpStopPos;
    Quaternion _jumpStopRot;


    Collider[] _cols;
    bool[] _colsWasTrigger;

    Coroutine _jumpRoutine;

    // ---------------- Helpers ----------------

    float FlatDistanceToPlayer()
    {
        Vector3 a = player.position;
        Vector3 b = transform.position;
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    Vector3 GetMoveOriginWorldPos()
    {
        // Eggpit uses detectPoint position with NO offset for movement origin.
        if (detectPoint) return detectPoint.position;
        return transform.position;
    }

    void CacheColliders()
    {
        _cols = GetComponentsInChildren<Collider>(true);
        _colsWasTrigger = new bool[_cols.Length];
        for (int i = 0; i < _cols.Length; i++)
            _colsWasTrigger[i] = _cols[i].isTrigger;
    }

    // ---------------- Unity ----------------

    void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
        floatPhase = Random.Range(0f, 10f);
        whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);

        if (playerRespawn == null)
            playerRespawn = Object.FindFirstObjectByType<PlayerRespawnHandler>();

        CacheColliders();
    }

    void Update()
    {
        if (!player || !playerCamera) return;

        UpdatePlayerSpeed();

        // Trigger stays the same idea
        if (enableJumpScare && !isJumpScaring)
            TryJumpScareTrigger();

        // During JumpScare we do NOT run orbit/haunt/flee OR ApplyFloating,
        // because floating/raycast Y snaps can feel like "teleport".
        if (currentState == State.JumpScare)
            return;

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

    void TryJumpScareTrigger()
    {
        if (!enableJumpScare || isJumpScaring) return;
        if (!playerCamera) return;

        float dist = FlatDistanceToPlayer();

        // (keep your existing “range gating” feel)
        if (dist > toHauntDistance + 1f) return;
        if (dist < 2f) return;

        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward.normalized, toPriest);
        float cosThreshold = Mathf.Cos(jumpScareViewAngle * Mathf.Deg2Rad);

        if (dot < cosThreshold) return;

        StartJumpScare();
    }

    void StartJumpScare()
    {
        if (_jumpRoutine != null) StopCoroutine(_jumpRoutine);

        isJumpScaring = true;
        currentState = State.JumpScare;

        if (breathSource) breathSource.Stop();
        if (whisperSource && jumpScareClip)
            whisperSource.PlayOneShot(jumpScareClip);

        _jumpRoutine = StartCoroutine(JumpScareRoutine_EggpitLiteral());
    }

    IEnumerator JumpScareRoutine_EggpitLiteral()
    {
        // No-collision mode (Eggpit style)
        if (makeCollidersTriggerDuringAttack && _cols != null)
        {
            for (int i = 0; i < _cols.Length; i++)
                if (_cols[i]) _cols[i].isTrigger = true;
        }

        // Lunge phase (Eggpit style)
        float t = 0f;
        while (t < maxAttackTime)
        {
            t += Time.deltaTime;
            if (!playerCamera) break;

            Vector3 targetPos = playerCamera.position; // treat camera/head as target
            Vector3 originPos = GetMoveOriginWorldPos();

            Vector3 aimDir = targetPos - originPos;
            if (aimDir.sqrMagnitude < 0.001f) break;

            // Face lock
            Quaternion look = Quaternion.LookRotation(aimDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * attackTurnSpeed);

            Vector3 moveDir = aimDir;

            if (lockY)
            {
                float yDelta = moveDir.y;
                moveDir.y = 0f;

                if (moveDir.sqrMagnitude < 0.001f) break;
                moveDir.Normalize();

                // Eggpit "feel" (keep some vertical)
                moveDir.y = yDelta * 0.35f;
            }
            else
            {
                moveDir.Normalize();
            }

            transform.position += moveDir * attackMoveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
            {
                // store where we stopped so we can freeze here during spook time
                _jumpStopPos = transform.position;
                _jumpStopRot = transform.rotation;
                break;
            }


            yield return null;
        }
        _jumpStopPos = transform.position;
        _jumpStopRot = transform.rotation;

        // Linger / stuck-in-face phase
        float linger = Mathf.Max(0f, spookTime);
        while (linger > 0f)
        {
            linger -= Time.deltaTime;

            if (!playerCamera) break;

            // Freeze at the exact stop point for spookTime
            transform.position = _jumpStopPos;

            // Option A: keep the rotation he had when he stopped (most stable)
            transform.rotation = _jumpStopRot;

            // Option B (if you prefer): comment the line above and keep him looking at you instead
            // Vector3 aimDir = (playerCamera.position - transform.position);
            // if (aimDir.sqrMagnitude > 0.0001f)
            // {
            //     Quaternion look = Quaternion.LookRotation(aimDir.normalized, Vector3.up);
            //     transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * attackTurnSpeed);
            // }


            yield return null;
        }

        // restore colliders
        if (_cols != null)
        {
            for (int i = 0; i < _cols.Length; i++)
                if (_cols[i]) _cols[i].isTrigger = _colsWasTrigger[i];
        }

        _jumpRoutine = null;

        FinishJumpScare();
    }

    void FinishJumpScare()
    {
        isJumpScaring = false;
        hauntDelayTimer = 0f;
        fleeTimer = fleeTime;
        floatPhase = Random.Range(0f, 10f);

        // Pick flee target
        Vector3 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.01f)
            awayDir = -player.forward;

        Vector3 randomSide = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        awayDir = (awayDir + randomSide).normalized;
        fleeTarget = transform.position + awayDir * fleeDistance;

        // Despawn + player respawn
        if (spawner) spawner.DespawnPriest();
        if (playerRespawn) playerRespawn.RespawnPlayer();
        else Debug.LogWarning("PlayerRespawnHandler not assigned!");

        currentState = State.Flee;
    }

    // ================== GENERAL LOGIC (UNCHANGED FEEL) ==================

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
            Debug.LogWarning("No respawn point set for HybridPriestAI_V3.");
            return;
        }

        if (_jumpRoutine != null)
        {
            StopCoroutine(_jumpRoutine);
            _jumpRoutine = null;
        }

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        isJumpScaring = false;
        hauntDelayTimer = 0f;

        // restore colliders (safety)
        if (_cols != null)
        {
            for (int i = 0; i < _cols.Length; i++)
                if (_cols[i]) _cols[i].isTrigger = _colsWasTrigger[i];
        }

        if (startInFlee)
        {
            currentState = State.Flee;
            fleeTimer = fleeTime;

            Vector3 awayDir = (transform.position - player.position).normalized;
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            awayDir = (awayDir + randomOffset).normalized;

            fleeTarget = transform.position + awayDir * fleeDistance;
        }
        else
        {
            currentState = State.Orbit;
        }
    }
}
