using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MonsterStandard : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject mainCamera;
    public GameObject scareCamera;

    [Header("Game Over UI")]
    public GameObject gameOverScreen; 
    public TMP_Text youDiedText;      
    public Button respawnButton;      

    [Header("Timing Instellingen")]
    public float timeToStareAtMonster = 2.0f; 
    public float textFadeInDuration = 4.0f;   
    public float waitBeforeButton = 2.0f;     

    [Header("Snelheid Instellingen")] // <--- NIEUW: Hier pas je de snelheid aan
    public float chaseSpeed = 5.0f;   // Snelheid tijdens achtervolgen
    public float wanderSpeed = 2.0f;  // Snelheid tijdens rustig rondlopen

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

        // Zet de start snelheid
        agent.speed = wanderSpeed; // <--- NIEUW

        startPosition = transform.position;

        if (scareCamera != null) scareCamera.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        
        if (respawnButton != null) 
        {
            respawnButton.gameObject.SetActive(false);
            respawnButton.onClick.RemoveAllListeners();
            respawnButton.onClick.AddListener(RestartScene);
        }
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
        Time.timeScale = 1f;
        hasAttacked = true;
        isChasing = false;

        if (agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (mainCamera != null) mainCamera.SetActive(false);
        if (scareCamera != null) scareCamera.SetActive(true);

        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(timeToStareAtMonster);

        if (gameOverScreen != null) gameOverScreen.SetActive(true);

        if (youDiedText != null)
        {
            youDiedText.canvasRenderer.SetAlpha(0f); 
            youDiedText.CrossFadeAlpha(1f, textFadeInDuration, true); 
        }

        yield return new WaitForSeconds(textFadeInDuration + waitBeforeButton);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (respawnButton != null) respawnButton.gameObject.SetActive(true);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Standaard Functies ---
    void ChaseBehavior(bool currentlySeeingPlayer) {
        // Zorg dat hij de rensnelheid gebruikt
        agent.speed = chaseSpeed; // <--- AANGEPAST (was hardcoded 5f)

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateDelay) { 
            agent.SetDestination(lastKnownPlayerPosition); 
            pathUpdateTimer = 0; 
        }
        
        // Als hij de speler kwijt is bij de laatste positie
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !currentlySeeingPlayer) { 
            isChasing = false; 
            wanderTimer = wanderInterval; 
            agent.speed = wanderSpeed; // <--- AANGEPAST (was hardcoded 2.0f)
        }
    }

    void WanderBehavior() {
        // Zorg dat hij de wandelsnelheid gebruikt (voor de zekerheid)
        agent.speed = wanderSpeed; // <--- NIEUW

        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval) { 
            agent.SetDestination(RandomNavmeshLocation(wanderRadius)); 
            wanderTimer = 0; 
        }
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