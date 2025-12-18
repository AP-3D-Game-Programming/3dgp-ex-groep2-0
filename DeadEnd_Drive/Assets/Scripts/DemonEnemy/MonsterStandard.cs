using UnityEngine;
using UnityEngine.AI;

public class MonsterStandard : MonoBehaviour
{
    [Header("Instellingen Jagen")]
    public float lookRadius = 15f;
    public LayerMask obstacleMask;
    public Transform player;
    private float pathUpdateDelay = 0.2f; 
    private float pathUpdateTimer;

    public float modelRotationCorrection = 0f;

    [Header("Instellingen Dwalen (Zone)")]
    public float wanderRadius = 20f; // Iets kleiner gezet voor bij het huis
    public float wanderInterval = 10f;
    
    // NIEUW: Dit is het punt waar hij omheen moet blijven cirkelen
    public Transform wanderZoneCenter; 
    private Vector3 startPosition; // Fallback voor als je geen center instelt

    private NavMeshAgent agent;
    private float wanderTimer;
    private Vector3 lastKnownPlayerPosition;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderInterval;
        
        agent.updateRotation = false; 
        agent.updateUpAxis = false;

        // Sla de startpositie op. Als je 'wanderZoneCenter' vergeet in te vullen,
        // blijft hij rondom zijn spawn-plek lopen.
        startPosition = transform.position;

        if (player == null) Debug.LogError("VERGEET NIET DE SPELER TE KOPPELEN!");
    }

    void Update()
    {
        // Veiligheidscheck: als player niet bestaat, stop de functie
        if (player == null) return; 

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CheckLineOfSight(distanceToPlayer);

        // STATUS BEPALEN
        if (canSeePlayer)
        {
            isChasing = true;
            lastKnownPlayerPosition = player.position;
        }

        // GEDRAG UITVOEREN
        if (isChasing)
        {
            ChaseBehavior(canSeePlayer);
        }
        else
        {
            WanderBehavior();
        }

        FixModelRotation();
    }

    void ChaseBehavior(bool currentlySeeingPlayer)
    {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateDelay)
        {
            agent.SetDestination(lastKnownPlayerPosition);
            pathUpdateTimer = 0;
            // Als hij jaagt, mag hij wat sneller (optioneel)
            agent.speed = 3.5f; 
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!currentlySeeingPlayer)
            {
                isChasing = false;
                wanderTimer = wanderInterval; 
                agent.speed = 2.0f; // Rustig lopen tijdens dwalen
            }
        }
    }

    void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            // AANGEPAST: We gebruiken nu de vaste zone in plaats van huidige positie
            Vector3 newPos = RandomNavmeshLocation(wanderRadius);
            agent.SetDestination(newPos);
            wanderTimer = 0;
        }
    }

    void FixModelRotation()
    {
        Vector3 direction = Vector3.zero;

        if (agent.hasPath)
        {
            direction = (agent.steeringTarget - transform.position).normalized;
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            direction = agent.velocity.normalized;
        }

        if (direction != Vector3.zero && direction.magnitude > 0.1f)
        {
            direction.y = 0; 
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion correction = Quaternion.Euler(0, modelRotationCorrection, 0);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * correction, Time.deltaTime * 8f);
        }
    }

    bool CheckLineOfSight(float distance)
    {
        if (distance > lookRadius) return false;
        if (Physics.Linecast(transform.position, player.position, obstacleMask)) return false;
        return true;
    }

    // AANGEPAST: Berekent locatie vanuit het centrum, niet vanuit de zombie
    public Vector3 RandomNavmeshLocation(float radius)
    {
        // Bepaal het middelpunt: Is er een zoneCenter ingesteld? Gebruik die.
        // Zo niet? Gebruik de positie waar de zombie het spel begon.
        Vector3 origin = (wanderZoneCenter != null) ? wanderZoneCenter.position : startPosition;

        Vector3 randomDirection = Random.insideUnitSphere * radius;
        
        // Belangrijk: Tel het op bij de ORIGIN, niet bij transform.position
        randomDirection += origin; 
        
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, radius, -1);
        return hit.position;
    }
    
    // GIZMOS: Handig om in de editor te zien waar zijn gebied is
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Teken de dwaal-cirkel
        Vector3 center = (wanderZoneCenter != null) ? wanderZoneCenter.position : (Application.isPlaying ? startPosition : transform.position);
        Gizmos.DrawWireSphere(center, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}