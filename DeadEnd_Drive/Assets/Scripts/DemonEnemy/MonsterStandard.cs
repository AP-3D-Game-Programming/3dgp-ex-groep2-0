using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class MonsterStandard : MonoBehaviour
{
    [Header("Cameras (Sleep ze hierin!)")]
    public GameObject mainCamera;  // Je normale speler camera
    public GameObject scareCamera; // De camera op het hoofd van het monster

    [Header("Instellingen Jagen")]
    public float lookRadius = 15f;
    public LayerMask obstacleMask;
    public Transform player;
    private float pathUpdateDelay = 0.2f; 
    private float pathUpdateTimer;
    public float modelRotationCorrection = 0f;

    [Header("Instellingen Dwalen")]
    public float wanderRadius = 20f;
    public float wanderInterval = 10f;
    public Transform wanderZoneCenter; 
    private Vector3 startPosition;

    [Header("Instellingen Aanval")]
    public float attackDistance = 1.5f;
    public AudioClip attackSound;
    
    private bool hasAttacked = false;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private float wanderTimer;
    private Vector3 lastKnownPlayerPosition;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        wanderTimer = wanderInterval;
        
        agent.updateRotation = false; 
        agent.updateUpAxis = false;
        agent.stoppingDistance = attackDistance - 0.2f; 

        startPosition = transform.position;

        // Veiligheidscheck
        if (scareCamera != null) scareCamera.SetActive(false); // Zeker weten dat hij uit begint
        if (mainCamera == null) Debug.LogWarning("Vergeet je Main Camera niet te koppelen!");
    }

    void Update()
    {
        if (player == null || hasAttacked) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CheckLineOfSight(distanceToPlayer);

        if (distanceToPlayer <= attackDistance)
        {
            AttackPlayer();
        }
        else
        {
            if (canSeePlayer)
            {
                isChasing = true;
                lastKnownPlayerPosition = player.position;
            }

            if (isChasing) ChaseBehavior(canSeePlayer);
            else WanderBehavior();

            FixModelRotation();
        }
    }

    void AttackPlayer()
    {
        hasAttacked = true;
        isChasing = false;

        // 1. Stop het monster
        agent.isStopped = true;
        agent.ResetPath();

        // 2. CAMERA SWAP TRUC
        if (mainCamera != null) mainCamera.SetActive(false); // Zet speler ogen uit
        if (scareCamera != null) scareCamera.SetActive(true); // Zet monster camera aan

        // 3. Geluid afspelen
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        Debug.Log("JUMPSCARE! Camera switch!");
    }

    // --- De standaard functies (ongewijzigd) ---
    void ChaseBehavior(bool currentlySeeingPlayer) {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateDelay) { agent.SetDestination(lastKnownPlayerPosition); pathUpdateTimer = 0; agent.speed = 3.5f; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !currentlySeeingPlayer) { isChasing = false; wanderTimer = wanderInterval; agent.speed = 2.0f; }
    }
    void WanderBehavior() {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval) { agent.SetDestination(RandomNavmeshLocation(wanderRadius)); wanderTimer = 0; }
    }
    void FixModelRotation() {
        Vector3 dir = Vector3.zero;
        if (agent.hasPath) dir = (agent.steeringTarget - transform.position).normalized;
        else if (agent.velocity.sqrMagnitude > 0.1f) dir = agent.velocity.normalized;
        if (dir != Vector3.zero) { dir.y = 0; transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir) * Quaternion.Euler(0, modelRotationCorrection, 0), Time.deltaTime * 8f); }
    }
    bool CheckLineOfSight(float d) {
        if (d > lookRadius) return false;
        return !Physics.Linecast(transform.position, player.position, obstacleMask);
    }
    public Vector3 RandomNavmeshLocation(float r) {
        Vector3 o = (wanderZoneCenter != null) ? wanderZoneCenter.position : startPosition;
        Vector3 rd = Random.insideUnitSphere * r + o; 
        NavMeshHit h; NavMesh.SamplePosition(rd, out h, r, -1); return h.position;
    }
    void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, lookRadius); Gizmos.DrawSphere(transform.position, attackDistance); }
}