using UnityEngine;
using System.Collections;

public class HybridPriestAIV4 : MonoBehaviour
{
    public enum State { Orbit, Haunt, Flee, JumpScare }
    public State currentState = State.Orbit;

    [Header("Targets (Injected by Spawner)")]
    public Transform player;
    public Transform playerCamera;
    public PlayerRespawnHandler playerRespawn;

    [Header("Spawner / Manager (Injected)")]
    public MonsterManager spawner;

    [Header("Optional Fallback Auto-Assign (OFF by default)")]
    public bool autoAssignIfMissing = false;
    public string playerTag = "Player";

    [Header("Fail-Safe")]
    public Transform respawnPoint;
    public float fallThresholdY = -1000f;

    // ===================== JUMPSCARE (V3 FACE-LOCK RESTORED) =====================

    [Header("Jump Scare (Trigger)")]
    public bool enableJumpScare = false;
    public AudioClip jumpScareClip;
    public bool isJumpScaring = false;

    [Tooltip("How precisely the player must look at the priest to trigger a jumpscare (degrees).")]
    public float jumpScareViewAngle = 10f;

    [Header("Jump Scare (Face-Lock Movement)")]
    [Tooltip("Optional point used as the 'origin' of the attack (like your eye/head pivot). If empty, uses transform.")]
    public Transform detectPoint;

    [Tooltip("Extra offset added to the camera position while attacking (fine-tune the 'face' target).")]
    public Vector3 jumpScareOffset = new Vector3(0f, 0f, 0f);

    [Tooltip("How fast the priest lunges toward the camera.")]
    public float attackMoveSpeed = 25f;

    [Tooltip("Max time the lunge is allowed to run (prevents infinite chase).")]
    public float maxAttackTime = 0.7f;

    [Tooltip("Stops the lunge when priest gets within this distance of the camera.")]
    public float stopDistance = 0.35f;

    [Tooltip("How fast he rotates to face the camera during lunge.")]
    public float attackTurnSpeed = 18f;

    [Tooltip("If true, keeps movement mostly flat (less vertical snapping).")]
    public bool lockY = true;

    [Header("Jump Scare (No-Collision During Attack)")]
    public bool makeCollidersTriggerDuringAttack = true;

    [Header("Jump Scare (Linger / Stick)")]
    [Tooltip("How long he freezes close to your face after reaching stopDistance.")]
    public float spookTime = 0.75f;

    // internal
    private Coroutine _jumpRoutine;
    private Vector3 _jumpStopPos;
    private Quaternion _jumpStopRot;

    private Collider[] _cols;
    private bool[] _colsWasTrigger;

    // ===================== AUDIO =====================

    [Header("Audio")]
    public AudioSource breathSource;
    public AudioSource whisperSource;

    public AudioClip hauntBreathingLoop;
    public AudioClip whisperLine1;
    public AudioClip whisperLine2;

    public float whisperMinDelay = 4f;
    public float whisperMaxDelay = 10f;
    public float specialWhisperCooldown = 6f;

    float whisperTimer;
    float lastSpecialWhisperTime = -999f;

    // ===================== MOVEMENT / STATES =====================

    [Header("Distances")]
    public float orbitRadius = 8f;
    public float toHauntDistance = 20f;
    public float toFleeDistance = 3f;

    [Header("Speeds")]
    public float orbitSpeed = 25f;
    public float hauntSpeed = 3f;
    public float fleeSpeed = 10f;

    [Header("Haunt Behaviour")]
    [Range(-1f, 1f)]
    [Tooltip("-1 = player looking away, 0 = 90 degr. left/right, 1 = player looking at priest")]
    public float lookDotThreshold = 0.6f;
    public float hauntStopDistance = 2f;
    public float overhang = 5f;

    [Header("Haunt Delay")]
    public float hauntDelay = 3f;
    float hauntDelayTimer = 0f;

    [Header("Flee Behaviour")]
    public bool enableFleeBehavior = true;
    public bool requirePlayerMoving = true;
    public float playerMoveThreshold = 0.15f;
    public float fleeDistance = 60f;

    [Header("Return / Come-back Settings")]
    public bool useFleeTimer = true;
    public float fleeTime = 5f;

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

    Vector3 lastPlayerPos;
    bool hasLastPlayerPos = false;
    float playerSpeed;

    // -------------------- PUBLIC API FOR SPAWNER --------------------
    public void Configure(Transform playerTransform, Transform cameraTransform, PlayerRespawnHandler respawnHandler, MonsterManager manager)
    {
        player = playerTransform;
        playerCamera = cameraTransform;
        playerRespawn = respawnHandler;
        spawner = manager;
    }

    public void ResetBrain(bool startInFlee = true)
    {
        isJumpScaring = false;

        if (_jumpRoutine != null)
        {
            StopCoroutine(_jumpRoutine);
            _jumpRoutine = null;
        }

        hauntDelayTimer = 0f;
        floatPhase = Random.Range(0f, 10f);

        hasLastPlayerPos = false;
        playerSpeed = 0f;

        if (startInFlee)
        {
            currentState = State.Flee;
            fleeTimer = fleeTime;
            PickFleeTarget();
        }
        else
        {
            currentState = State.Orbit;
            fleeTimer = fleeTime;
        }

        StopHauntAudio();
        whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);
    }

    // -------------------- UNITY --------------------
    void Start()
    {
        orbitAngle = Random.Range(0f, 360f);
        floatPhase = Random.Range(0f, 10f);
        whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);

        CacheColliders();

        if (autoAssignIfMissing)
            TryAutoAssignOnce();
    }

    void OnDisable()
    {
        // safety: restore collider trigger states if object gets disabled mid-attack
        RestoreCollidersAfterAttack();
    }

    void Update()
    {
        if ((!player || !playerCamera) && autoAssignIfMissing)
            TryAutoAssignOnce();

        if (!player || !playerCamera) return;

        if (transform.position.y < fallThresholdY)
        {
            RespawnToSafePoint(startInFlee: true);
            return;
        }

        UpdatePlayerSpeed();

        if (enableJumpScare && !isJumpScaring)
            TryJumpScareTrigger();

        // Jump scare is coroutine-driven now.
        if (currentState == State.JumpScare)
            return;

        // ---- Freeze logic (ONLY in Orbit & Haunt) ----
        Vector3 toPriest = (GetLookPoint() - playerCamera.position).normalized;
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

    // ================== TARGETING / FALLBACK ==================
    void TryAutoAssignOnce()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }

        if (!playerCamera)
        {
            if (player)
            {
                var cam = player.GetComponentInChildren<Camera>(true);
                if (cam) playerCamera = cam.transform;
            }

            if (!playerCamera && Camera.main != null)
                playerCamera = Camera.main.transform;
        }

        if (!playerRespawn)
            playerRespawn = Object.FindFirstObjectByType<PlayerRespawnHandler>();
    }

    float FlatDistanceToPlayer()
    {
        Vector3 a = player.position;
        Vector3 b = transform.position;
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

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
        PickFleeTarget();
    }

    void PickFleeTarget()
    {
        if (!player)
        {
            fleeTarget = transform.position + transform.forward * fleeDistance;
            return;
        }

        Vector3 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.01f)
            awayDir = -player.forward;

        Vector3 randomSide = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        awayDir = (awayDir + randomSide).normalized;

        fleeTarget = transform.position + awayDir * fleeDistance;
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
        Vector3 toPriest = (GetLookPoint() - playerCamera.position).normalized;
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

    // ================== AUDIO ==================
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
        Vector3 toPriest = (GetLookPoint() - playerCamera.position).normalized;
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

    // ================== JUMPSCARE (FACE-LOCK) ==================
    void TryJumpScareTrigger()
    {
        if (!enableJumpScare || isJumpScaring) return;
        if (!playerCamera) return;

        float dist = FlatDistanceToPlayer();

        if (dist > toHauntDistance + 1f) return;
        if (dist < 2f) return;

        Vector3 toPriest = (GetLookPoint() - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward.normalized, toPriest);
        float cosThreshold = Mathf.Cos(jumpScareViewAngle * Mathf.Deg2Rad);

        if (dot < cosThreshold) return;

        StartJumpScare();
    }

    void StartJumpScare()
    {
        if (_jumpRoutine != null)
            StopCoroutine(_jumpRoutine);

        isJumpScaring = true;
        currentState = State.JumpScare;

        if (breathSource) breathSource.Stop();
        if (whisperSource && jumpScareClip)
            whisperSource.PlayOneShot(jumpScareClip);

        _jumpRoutine = StartCoroutine(JumpScareRoutine_FaceLock());
    }

    IEnumerator JumpScareRoutine_FaceLock()
    {
        SetCollidersTriggerForAttack(true);

        float t = 0f;

        while (t < maxAttackTime)
        {
            t += Time.deltaTime;

            if (!playerCamera) break;

            Vector3 targetPos = playerCamera.position + jumpScareOffset;

            // IMPORTANT: origin used for aim + stop check (V3 behavior)
            Vector3 originPos = GetMoveOriginWorldPos();
            Vector3 aimDir = targetPos - originPos;

            if (aimDir.sqrMagnitude < 0.0005f) break;

            // Face lock
            Quaternion look = Quaternion.LookRotation(aimDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * attackTurnSpeed);

            // Move toward camera (with optional Y locking)
            Vector3 moveDir = aimDir;

            if (lockY)
            {
                float yDelta = moveDir.y;
                moveDir.y = 0f;

                if (moveDir.sqrMagnitude < 0.0005f) break;

                moveDir.Normalize();
                moveDir.y = yDelta * 0.35f;
            }
            else
            {
                moveDir.Normalize();
            }

            transform.position += moveDir * attackMoveSpeed * Time.deltaTime;

            //STOP CHECK (V3-style): measure using originPos (detectPoint), not root transform
            float distFromOrigin = Vector3.Distance(originPos, targetPos);
            if (distFromOrigin <= stopDistance)
            {
                _jumpStopPos = transform.position;
                _jumpStopRot = transform.rotation;
                break;
            }

            yield return null;
        }

        // If we exited by timeout, still cache a stop pose
        if (_jumpStopPos == Vector3.zero)
        {
            _jumpStopPos = transform.position;
            _jumpStopRot = transform.rotation;
        }

        // ✅ SPOOK (V3): freeze at the exact stop point for spookTime
        float linger = Mathf.Max(0f, spookTime);
        while (linger > 0f)
        {
            linger -= Time.deltaTime;

            if (!playerCamera) break;

            transform.position = _jumpStopPos;
            transform.rotation = _jumpStopRot;

            yield return null;
        }

        RestoreCollidersAfterAttack();

        _jumpRoutine = null;

        FinishJumpScare();
    }


    void FinishJumpScare()
    {
        // -------- RESET INTERNAL STATE --------
        isJumpScaring = false;
        hauntDelayTimer = 0f;
        fleeTimer = fleeTime;
        floatPhase = Random.Range(0f, 10f);

        PickFleeTarget();

        // -------- DESPAWN + PLAYER RESPAWN --------
        if (spawner) spawner.DespawnPriest();
        if (playerRespawn) playerRespawn.RespawnPlayer();
        else Debug.LogWarning("PlayerRespawnHandler not assigned!");

        currentState = State.Flee;
    }

    Vector3 GetLookPoint()
    {
        // Use detectPoint if you have it (face/head pivot), else fallback to transform
        return detectPoint ? detectPoint.position : transform.position;
    }


    Vector3 GetMoveOriginWorldPos()
    {
        if (detectPoint) return detectPoint.position;
        return transform.position;
    }

    void CacheColliders()
    {
        _cols = GetComponentsInChildren<Collider>(true);
        _colsWasTrigger = new bool[_cols.Length];
        for (int i = 0; i < _cols.Length; i++)
            _colsWasTrigger[i] = _cols[i] != null && _cols[i].isTrigger;
    }

    void SetCollidersTriggerForAttack(bool trigger)
    {
        if (!makeCollidersTriggerDuringAttack) return;
        if (_cols == null || _cols.Length == 0) CacheColliders();

        for (int i = 0; i < _cols.Length; i++)
        {
            if (_cols[i]) _cols[i].isTrigger = trigger ? true : _colsWasTrigger[i];
        }
    }

    void RestoreCollidersAfterAttack()
    {
        if (_cols == null || _colsWasTrigger == null) return;

        for (int i = 0; i < _cols.Length; i++)
        {
            if (_cols[i]) _cols[i].isTrigger = _colsWasTrigger[i];
        }
    }

    // ================== RESPAWN ==================
    public void RespawnToSafePoint(bool startInFlee = false)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("No respawn point set for HybridPriestAI.");
            return;
        }

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        isJumpScaring = false;
        hauntDelayTimer = 0f;

        if (_jumpRoutine != null)
        {
            StopCoroutine(_jumpRoutine);
            _jumpRoutine = null;
            RestoreCollidersAfterAttack();
        }

        if (startInFlee)
        {
            currentState = State.Flee;
            fleeTimer = fleeTime;
            PickFleeTarget();
        }
        else
        {
            currentState = State.Orbit;
        }
    }
}
