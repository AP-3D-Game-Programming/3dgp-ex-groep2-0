using UnityEngine;

public class ProximityHeartbeat : MonoBehaviour
{
    [Header("Instellingen")]
    public string enemyTag = "Enemy"; 
    public float maxDistance = 50f;   
    public float minDistance = 2f;    

    [Header("Audio Effect")]
    public float minPitch = 1.0f;     
    public float maxPitch = 3.0f;     
    [Range(0f, 1f)]                   // Zorgt voor een slider in Unity (veiliger)
    public float maxVolume = 10f;      // Aangepast naar 1 (Unity volume gaat normaal van 0 tot 1)

    [Header("Sleep hier je Audio Source in")]
    public AudioSource audioSource;   // <--- DEZE IS NU PUBLIC

    private GameObject[] enemies;
    private float checkTimer;

    void Start()
    {
        // De regel 'audioSource = GetComponent<AudioSource>();' is WEGGEHAALD.
        // We gebruiken nu degene die jij in de inspector sleept.

        // Veiligheidscheck: als je vergeten bent te slepen, geven we een error.
        if (audioSource == null)
        {
            Debug.LogError("VERGEET NIET de Audio Source in het script te slepen!");
            return;
        }
        
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0; 
        
        if (!audioSource.isPlaying) audioSource.Play();
    }

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer > 0.5f)
        {
            enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            checkTimer = 0;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        if (enemies != null)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;

                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closestEnemy = enemy;
                }
            }
        }

        HandleAudio(closestDistance);
    }

    void HandleAudio(float distance)
    {
        if (audioSource == null) return;

        if (distance >= maxDistance)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 2f);
            return;
        }

        float proximity = Mathf.InverseLerp(maxDistance, minDistance, distance);

        audioSource.volume = proximity * maxVolume;
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, proximity);
    }
}