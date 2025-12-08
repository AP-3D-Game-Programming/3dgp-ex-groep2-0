using UnityEngine;

public class ProximityHeartbeat : MonoBehaviour
{
    [Header("Instellingen")]
    public string enemyTag = "Enemy"; // Zorg dat je zombie deze Tag heeft!
    public float maxDistance = 50f;   // Vanaf hier begint het geluid
    public float minDistance = 2f;    // Hier is het geluid op zijn hardst/snelst

    [Header("Audio Effect")]
    public float minPitch = 1.0f;     // Normale snelheid (ver weg)
    public float maxPitch = 3.0f;     // Snelle snelheid (dichtbij)
    public float maxVolume = 5f;    // Maximaal volume

    private AudioSource audioSource;
    private GameObject[] enemies;
    private float checkTimer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Zorg dat het geluid loopt (zodat we pitch kunnen aanpassen)
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0; // Begin stil
        
        if (!audioSource.isPlaying) audioSource.Play();
    }

    void Update()
    {
        // Optimalisatie: Zoek niet elke frame naar enemies, maar elke 0.5 seconde
        checkTimer += Time.deltaTime;
        if (checkTimer > 0.5f)
        {
            enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            checkTimer = 0;
        }

        // Zoek de dichtstbijzijnde zombie
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

        // Pas het geluid aan
        HandleAudio(closestDistance);
    }

    void HandleAudio(float distance)
    {
        // Als er geen zombie is of hij is te ver weg
        if (distance >= maxDistance)
        {
            // Laat het volume langzaam naar 0 zakken (Fade out)
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 2f);
            return;
        }

        // Bereken percentage: 0 = ver weg, 1 = heel dichtbij
        // We gebruiken Mathf.InverseLerp om de afstand om te zetten naar een getal tussen 0 en 1
        float proximity = Mathf.InverseLerp(maxDistance, minDistance, distance);

        // Zet volume (hoe dichterbij, hoe harder)
        audioSource.volume = proximity * maxVolume;

        // Zet snelheid/pitch (hoe dichterbij, hoe hoger/sneller)
        // Lerp berekent de waarde tussen minPitch en maxPitch op basis van proximity
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, proximity);
    }
}