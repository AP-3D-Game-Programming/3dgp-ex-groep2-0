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
    public GameObject gameOverScreen; // Dit moet het 'GameOverScreen' object zijn in je Canvas
    public TMP_Text youDiedText;      // Dit is de 'Text (TMP)' ONDER het GameOverScreen
    public Button respawnButton;      // Dit is de 'Respawn' knop ONDER het GameOverScreen

    [Header("Timing Instellingen")]
    public float timeToStareAtMonster = 2.0f; // Tijd voor de jumpscare
    public float textFadeInDuration = 4.0f;   // Hoe langzaam de tekst verschijnt
    public float waitBeforeButton = 2.0f;     // Extra wachttijd voordat je kunt klikken

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

        // Zorg dat alles goed staat bij het begin
        if (scareCamera != null) scareCamera.SetActive(false);
        
        // Verberg het Game Over scherm bij de start
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        
        // Verberg de knop bij de start
        if (respawnButton != null) 
        {
            respawnButton.gameObject.SetActive(false);
            // Verwijder oude listeners om dubbele clicks te voorkomen en voeg nieuwe toe
            respawnButton.onClick.RemoveAllListeners();
            respawnButton.onClick.AddListener(RestartScene);
        }
    }

    void Update()
    {
        // Als speler dood is of er is geen speler, doe niets
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
        // VEILIGHEID: Zorg dat de tijd loopt, anders werken Coroutines niet
        Time.timeScale = 1f;

        hasAttacked = true;
        isChasing = false;

        // 1. Stop Monster
        if (agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 2. Camera Wissel
        if (mainCamera != null) mainCamera.SetActive(false);
        if (scareCamera != null) scareCamera.SetActive(true);

        // 3. Geluid
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        // Start de animatie reeks
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        // Stap 1: Staren naar monster
        yield return new WaitForSeconds(timeToStareAtMonster);

        // Stap 2: Scherm AANZETTEN
        if (gameOverScreen != null) 
        {
            gameOverScreen.SetActive(true);
        }

        // Stap 3: Tekst Fade Effect
        if (youDiedText != null)
        {
            // Zet alpha direct op 0 (onzichtbaar)
            youDiedText.canvasRenderer.SetAlpha(0f); 
            // Fade naar 1 (zichtbaar). De 'true' negeert timescale.
            youDiedText.CrossFadeAlpha(1f, textFadeInDuration, true); 
        }

        // Wacht terwijl de tekst infade + de extra wachttijd
        yield return new WaitForSeconds(textFadeInDuration + waitBeforeButton);

        // Stap 4: Muis en Knop
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