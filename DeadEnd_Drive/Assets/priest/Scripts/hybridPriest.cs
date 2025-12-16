using UnityEngine;
using System.Collections;

public class HybridPriestAI : MonoBehaviour
{
    public enum State { Orbit, Haunt, Flee, JumpScare }
    public State currentState = State.Orbit;

    [Header("Targets")]
    public Transform player;
    public Transform playerCamera;

    [Header("Fail-Safe")]
    public Transform respawnPoint; // Drag your respawn location here in Inspector
    public float fallThresholdY = 1000f; // If priest falls below this, we respawn


    [Header("Audio Volumes")]
    [Range(0f, 1f)] public float breathVolume = 1f;     // Volume voor ademen
    [Range(0f, 1f)] public float whisperVolume = 1f;    // Volume voor fluisteren
    [Range(0f, 1f)] public float jumpScareVolume = 1f;  // Volume voor schrikmoment
    [Range(0f, 1f)] public float spawnVolume = 1f;

    [Header("Jump Scare")]
    public bool enableJumpScare = false;
    public AudioClip jumpScareClip;
    public bool isJumpScaring = false;
    public float jumpScareSpeed = 10f;
    public float jumpScareDistance = 0f;
    public Vector3 jumpScareOffset = new Vector3(0, 0.25f, 0); // adjust face height
    private Vector3 jumpScareTarget;
    [Header("Respawn After JumpScare")]

    //private float jumpScareRespawnTimer = -1f;



    [Tooltip("How precisely the player must look at the priest to trigger a jumpscare (degrees).")]
    public float jumpScareViewAngle = 10f;   // smaller = more accurate center view
    public MonsterManager spawner; // drag your spawner here in Inspector

    [Header("Audio")]
    public AudioSource breathSource;          // breathing loop source
    public AudioSource whisperSource;         // whispers source

    public AudioClip hauntBreathingLoop;      // breathing loop sound
    public AudioClip whisperLine1;            // "You shouldn't have come here..."
    public AudioClip whisperLine2;            // "We have been waiting..."
    // --- NIEUW: Spawn geluid ---
    public AudioClip spawnSound;              // Geluid bij spawnen

    public float whisperMinDelay = 4f;
    public float whisperMaxDelay = 10f;
    public float specialWhisperCooldown = 6f;

    float whisperTimer;
    float lastSpecialWhisperTime = -999f;



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
    [Tooltip("-1 = player looking away, 0 = 90 degr. left/right, 1 = player looking at priest")]
    public float lookDotThreshold = 0.6f; // 1 = straight at priest
    public float hauntStopDistance = 2f;  // how close Haunt is allowed to get
    public float overhang = 5f;

    [Header("Haunt Delay")]
    public float hauntDelay = 3f;  // time from orbit to haunt
    float hauntDelayTimer = 0f;    // delay from orbit to haunt

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

    [Header("State Flags")]
    public bool hasAttacked = false;


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

        whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);

        PlaySpawnSoundNearPlayer();
    }

    void Update()
    {
        if (!player || !playerCamera) return;

        UpdatePlayerSpeed();


        if (enableJumpScare && !isJumpScaring)
            TryJumpScareTrigger();

        // Jump scare movement
        if (currentState == State.JumpScare)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                jumpScareTarget,
                jumpScareSpeed * Time.deltaTime
            );

            transform.LookAt(playerCamera.position);

            if (Vector3.Distance(transform.position, jumpScareTarget) < 0.1f)
            {
                spawner.DespawnPriest();
                currentState = State.Flee; // Reset state so we don't get stuck
            }

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

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers))
        {
            Debug.LogWarning("Priest lost ground. Respawning...");
            RespawnToSafePoint(startInFlee: true); // 👈 or false, depending on what you want
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

        // pick between line1 and line2
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

        // avoid ridiculous overlap spam
        if (!whisperSource.isPlaying)
        {
            whisperSource.pitch = 1f;
            whisperSource.PlayOneShot(whisperLine1);
        }
    }

    void DoWhisperLogic()
    {
        if (!whisperSource) return;

        // only whisper when player is NOT staring at him
        if (!PlayerLookingAtPriest())
        {
            // --- random whispers every X–Y seconds ---
            whisperTimer -= Time.deltaTime;
            if (whisperTimer <= 0f)
            {
                PlayRandomWhisper();
                whisperTimer = Random.Range(whisperMinDelay, whisperMaxDelay);
            }

            // --- special line when close ("you shouldn't have come here") ---
            float dist = FlatDistanceToPlayer();
            if (dist < hauntStopDistance + 1.5f &&
                Time.time - lastSpecialWhisperTime > specialWhisperCooldown)
            {
                PlayWhisperLine1();
                lastSpecialWhisperTime = Time.time;
            }
        }
    }
    void TryJumpScareTrigger()
    {
        if (!enableJumpScare || isJumpScaring) return;
        if (!playerCamera) return;

        Vector3 toPriest = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward.normalized, toPriest);
        float cosThreshold = Mathf.Cos(jumpScareViewAngle * Mathf.Deg2Rad);

        if (dot < cosThreshold) return;

        StartJumpScare();
    }

    void StartJumpScare()
    {
        isJumpScaring = true;
        hasAttacked = true;
        currentState = State.JumpScare;


        Vector3 camPos = playerCamera.position;
        Vector3 priestPos = transform.position;

        // Direction: Priest -> Camera on flat ground
        Vector3 dir = (camPos - priestPos);
        dir.y = 0f;
        dir.Normalize();

        // JumpScare target: behind camera on same straight line
        jumpScareTarget = camPos + dir * jumpScareDistance;

        // Vertical alignment of face
        jumpScareTarget.y = priestPos.y + jumpScareOffset.y;

        // Face camera while flying
        transform.LookAt(new Vector3(camPos.x, priestPos.y, camPos.z));

        if (breathSource) breathSource.Stop();
        if (whisperSource && jumpScareClip)
            whisperSource.PlayOneShot(jumpScareClip);
    }

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

        // --- NIEUW: Gebruik de nieuwe slimme functie ---
        PlaySpawnSoundNearPlayer();
        // ----------------------------------------------

        if (startInFlee)
        {
            currentState = State.Flee;
            fleeTimer = fleeTime;

            // Pick flee direction
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

    void PlaySpawnSoundNearPlayer()
    {
        // Debug 1: Check of de variabelen gevuld zijn
        if (spawnSound == null)
        {
            Debug.LogError("FOUT: Er zit geen geluid in het vakje 'Spawn Sound' op de Priest!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("FOUT: Je bent vergeten de 'Player' in het script te slepen!");
            return;
        }

        // Debug 2: Berekening
        Vector3 directionToPriest = (transform.position - player.position).normalized;

        // We voegen Vector3.up toe zodat het geluid niet IN de grond spawnt
        Vector3 fakeSoundPos = player.position + (directionToPriest * 2f) + Vector3.up;

        Debug.Log("Geluid wordt afgespeeld op positie: " + fakeSoundPos);

        // Debug 3: Afspelen
        AudioSource.PlayClipAtPoint(spawnSound, playerCamera.position, 1f);
    }



}
