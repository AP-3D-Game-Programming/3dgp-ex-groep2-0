using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class ShadowStalker : MonoBehaviour
{
    [Header("Instellingen")]
    public Transform player;
    public Camera playerCamera; // Sleep je camera hierin!
    public float minWaitTime = 10f;
    public float maxWaitTime = 30f;
    public float spawnDistance = 10f;

    [Header("Zichtbaarheid")]
    [Range(5f, 60f)]
    public float vanishAngle = 20f; // Hoe recht moet je kijken? (Kleiner = preciezer kijken)
    
    [Header("Geluidsbestanden")]
    public AudioClip[] psstSounds;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Renderer[] allRenderers;
    private Collider col; 
    private bool isStalking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
        allRenderers = GetComponentsInChildren<Renderer>();

        // 1. Zoek Player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 2. Zoek Camera (Fallback)
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) Debug.LogWarning("LET OP: Sleep je camera in het script, ik kan hem niet vinden!");
        }

        HideMonster();
        StartCoroutine(StalkRoutine());
    }

    void Update()
    {
        if (isStalking && player != null && playerCamera != null)
        {
            CheckIfPlayerSeesMe();
        }
    }

    IEnumerator StalkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            if (TrySpawnBehindPlayer())
            {
                while (isStalking)
                {
                    yield return null;
                }
            }
        }
    }

    bool TrySpawnBehindPlayer()
    {
        Vector3 randomOffset = player.right * Random.Range(-5f, 5f);
        Vector3 targetPos = player.position - (player.forward * spawnDistance) + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            
            // Kijk naar speler (alleen Y-as)
            Vector3 lookPos = player.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            ShowMonster();
            return true;
        }
        return false;
    }

    void CheckIfPlayerSeesMe()
    {
        // Bereken het punt waar we naar kijken (borsthoogte monster)
        Vector3 targetPoint = transform.position + Vector3.up * 1.5f;
        
        // --- DE NIEUWE WISKUNDE ---
        
        // 1. Richting: Waar is het monster ten opzichte van de camera?
        Vector3 directionToMonster = (targetPoint - playerCamera.transform.position).normalized;

        // 2. Hoek: Hoeveel graden zit er tussen "Recht vooruit kijken" en "Het monster"?
        // 0 graden = Je kijkt hem recht in de ogen.
        // 90 graden = Hij staat precies naast je.
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToMonster);

        // Debug: Zie de hoek in je Console om te testen
        // Debug.Log("Hoek naar monster: " + angle);

        // Als de hoek KLEINER is dan je instelling, kijk je hem recht genoeg aan
        if (angle < vanishAngle)
        {
            // Nu pas checken we of er muren zijn (Raycast)
            RaycastHit hit;
            
            // Start straal iets voor de camera zodat we onszelf niet raken
            Vector3 startPos = playerCamera.transform.position + (playerCamera.transform.forward * 0.5f);
            
            Debug.DrawRay(startPos, directionToMonster * spawnDistance, Color.red);

            if (Physics.Raycast(startPos, directionToMonster, out hit))
            {
                 // Check of we het monster raken
                if (hit.collider.gameObject == gameObject || hit.collider.transform.root == transform)
                {
                    Debug.Log("Oogcontact! Wegwezen.");
                    HideMonster();
                }
            }
        }
    }

    void ShowMonster()
    {
        foreach (var r in allRenderers) r.enabled = true;
        if (col) col.enabled = true;
        isStalking = true;

        if (psstSounds.Length > 0)
        {
            audioSource.clip = psstSounds[Random.Range(0, psstSounds.Length)];
            audioSource.Play();
        }
    }

    void HideMonster()
    {
        foreach (var r in allRenderers) r.enabled = false;
        if (col) col.enabled = false;
        isStalking = false;
    }
}