using UnityEngine;
using UnityEngine.AI;

public class MonsterStandard : MonoBehaviour
{
    [Header("Instellingen Jagen")]
    public float lookRadius = 15f;
    public LayerMask obstacleMask;
    public Transform player;
    // Update de route niet elke frame, maar elke 0.2 seconden (voorkomt haperen)
    private float pathUpdateDelay = 0.2f; 
    private float pathUpdateTimer;

    public float modelRotationCorrection = 0f;

    [Header("Instellingen Dwalen")]
    public float wanderRadius = 40f;
    public float wanderInterval = 10f;

    private NavMeshAgent agent;
    private float wanderTimer;
    private Vector3 lastKnownPlayerPosition;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderInterval;
        
        // We regelen de rotatie zelf, dus Unity mag het niet doen
        agent.updateRotation = false; 
        agent.updateUpAxis = false;

        if (player == null) Debug.LogError("VERGEET NIET DE SPELER TE KOPPELEN!");
    }

    void Update()
    {
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

        // ALTIJD: Rotatie fixen
        FixModelRotation();
    }

    void ChaseBehavior(bool currentlySeeingPlayer)
    {
        // Optimalisatie: Roep SetDestination niet elke frame aan (voorkomt stotteren)
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateDelay)
        {
            agent.SetDestination(lastKnownPlayerPosition);
            pathUpdateTimer = 0;
            agent.speed = 5;
        }

        // Check of we er zijn EN de speler kwijt zijn
        // !agent.pathPending is belangrijk: check niet als hij nog aan het rekenen is
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!currentlySeeingPlayer)
            {
                // We zijn op de laatste plek, speler is weg. Ga weer dwalen.
                isChasing = false;
                wanderTimer = wanderInterval; // Zorgt dat hij direct een nieuwe dwaal-plek zoekt
            }
        }
    }

    void WanderBehavior()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            Vector3 newPos = RandomNavmeshLocation(wanderRadius);
            agent.SetDestination(newPos);
            wanderTimer = 0;
        }
    }

    // --- VERBETERDE ROTATIE FUNCTIE ---
    void FixModelRotation()
    {
        // We kijken naar de steeringTarget (het volgende punt op het pad)
        // Dit is veel stabieler dan 'velocity'
        Vector3 direction = Vector3.zero;

        if (agent.hasPath)
        {
            direction = (agent.steeringTarget - transform.position).normalized;
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            // Fallback voor als er even geen pad is maar wel snelheid
            direction = agent.velocity.normalized;
        }

        // Alleen draaien als er een duidelijke richting is (voorkomt trillen op 1 plek)
        if (direction != Vector3.zero && direction.magnitude > 0.1f)
        {
            direction.y = 0; // Zorg dat hij niet omhoog/omlaag kijkt
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion correction = Quaternion.Euler(0, modelRotationCorrection, 0);
            
            // Iets snellere rotatie (8f) voor responsiviteit
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * correction, Time.deltaTime * 8f);
        }
    }

    bool CheckLineOfSight(float distance)
    {
        if (distance > lookRadius) return false;
        if (Physics.Linecast(transform.position, player.position, obstacleMask)) return false;
        return true;
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        // Gebruik 1 als mask zodat hij overal mag lopen
        NavMesh.SamplePosition(randomDirection, out hit, radius, -1);
        return hit.position;
    }
}