using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class ShadowStalker : MonoBehaviour
{
    [Header("Instellingen")]
    public Transform player;
    public Camera playerCamera;
    public float minWaitTime = 10f;
    public float maxWaitTime = 30f;
    public float spawnDistance = 10f;
    
    // NIEUW: Hoe lang blijft hij staan als de speler niet kijkt?
    public float despawnTime = 10f; 

    [Header("Zichtbaarheid")]
    [Range(5f, 60f)]
    public float vanishAngle = 20f;
    
    [Header("Geluidsbestanden")]
    public AudioClip[] psstSounds;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Renderer[] allRenderers;
    private Collider col; 
    private bool isStalking = false;
    
    // NIEUW: Om de timer te kunnen stoppen als de speler wél kijkt
    private Coroutine despawnTimerRoutine; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
        allRenderers = GetComponentsInChildren<Renderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) Debug.LogWarning("LET OP: Sleep je camera in het script!");
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
                // Wacht tot de 'stalk' sessie voorbij is (door kijken of door tijd)
                while (isStalking)
                {
                    yield return null;
                }
            }
        }
    }

    // NIEUW: Deze routine wacht 10 seconden en haalt het monster dan weg
    IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(despawnTime);

        // Als we hier komen en hij is nog steeds aan het stalken...
        if (isStalking)
        {
            Debug.Log("Speler reageerde niet. Monster verdwijnt uit zichzelf.");
            HideMonster();
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
        Vector3 targetPoint = transform.position + Vector3.up * 1.5f;
        Vector3 directionToMonster = (targetPoint - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToMonster);

        if (angle < vanishAngle)
        {
            RaycastHit hit;
            Vector3 startPos = playerCamera.transform.position + (playerCamera.transform.forward * 0.5f);
            
            if (Physics.Raycast(startPos, directionToMonster, out hit))
            {
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

        // NIEUW: Start de timer zodra het monster verschijnt
        if (despawnTimerRoutine != null) StopCoroutine(despawnTimerRoutine);
        despawnTimerRoutine = StartCoroutine(AutoDespawn());
    }

    void HideMonster()
    {
        foreach (var r in allRenderers) r.enabled = false;
        if (col) col.enabled = false;
        isStalking = false;

        // NIEUW: Als de speler hem heeft gezien, hoeft de timer niet meer af te lopen
        if (despawnTimerRoutine != null)
        {
            StopCoroutine(despawnTimerRoutine);
            despawnTimerRoutine = null;
        }
    }
}