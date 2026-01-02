using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EndAnimation : MonoBehaviour
{
    [Header("De Speler")]
    public Transform spelerRoot;
    public Animator spelerAnim;
    public MonoBehaviour loopScript;
    public MonoBehaviour kijkScript;

    [Header("De Route")]
    public Transform[] routePunten;
    public float loopSnelheid = 3.0f;
    public float draaiSnelheid = 5.0f;

    [Header("Het Verhaal (Teksten)")]
    public GameObject tekstStart;  // Tekst zodra je begint te lopen
    public GameObject tekstPunt1;  // Tekst als je bij punt 1 bent
    public GameObject tekstPunt2;  // Tekst als je bij punt 2 bent

    [Header("Het Monster")]
    public MonsterStandard monsterScript;
    public Animator monsterAnim;

    private bool sceneIsGestart = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !sceneIsGestart)
        {
            StartCoroutine(StartHetEinde());
        }
    }

    IEnumerator StartHetEinde()
    {
        sceneIsGestart = true;

        // Zorg dat alle teksten uit staan voor de zekerheid
        VerbergAlleTeksten();

        // 1. Speler besturing uit & Animatie aan
        if (loopScript != null) loopScript.enabled = false;
        if (kijkScript != null) kijkScript.enabled = false;
        if (spelerAnim != null) spelerAnim.SetTrigger("TriggerSpelerLoop");

        // --- MOMENT A: DE START ---
        // Toon de eerste tekst direct
        if (tekstStart != null) tekstStart.SetActive(true);

        // 2. DE ROUTE LOPEN
        int puntTeller = 0; // We houden bij bij welk punt we zijn

        foreach (Transform punt in routePunten)
        {
            // Loop naar het punt toe
            while (Vector3.Distance(spelerRoot.position, punt.position) > 2.0f)
            {
                Vector3 richting = (punt.position - spelerRoot.position).normalized;
                if (richting != Vector3.zero)
                {
                    Quaternion kijkRotatie = Quaternion.LookRotation(new Vector3(richting.x, 0, richting.z));
                    spelerRoot.rotation = Quaternion.Slerp(spelerRoot.rotation, kijkRotatie, Time.deltaTime * draaiSnelheid);
                }
                spelerRoot.position = Vector3.MoveTowards(spelerRoot.position, punt.position, loopSnelheid * Time.deltaTime);
                yield return null;
            }

            // --- AANGEKOMEN BIJ EEN PUNT ---
            puntTeller++; // We hebben 1 punt afgerond

            if (puntTeller == 1)
            {
                // We zijn bij Punt 1
                VerbergAlleTeksten(); // Vorige tekst weg
                if (tekstPunt1 != null) tekstPunt1.SetActive(true); // Nieuwe tekst aan
            }
            else if (puntTeller == 2)
            {
                // We zijn bij Punt 2
                VerbergAlleTeksten();
                if (tekstPunt2 != null) tekstPunt2.SetActive(true);
            }
        }

        // 3. EINDE (Bij monster)
        VerbergAlleTeksten(); // Laatste tekst weg (of laat staan, wat jij wil)
        Debug.Log("Bij monster aangekomen!");

        // Monster Setup & Aanval (De NavMesh Fix)
        if (monsterScript != null)
        {
            Vector3 killPositie = spelerRoot.position + (spelerRoot.forward * 1.2f);
            NavMeshAgent agent = monsterScript.GetComponent<NavMeshAgent>();

            if (agent != null)
            {
                agent.enabled = true;
                killPositie.y = agent.transform.position.y;
                agent.Warp(killPositie);
            }
            else
            {
                killPositie.y = monsterScript.transform.position.y;
                monsterScript.transform.position = killPositie;
            }

            monsterScript.transform.LookAt(spelerRoot);
            monsterScript.enabled = true;
        }

        if (monsterAnim != null) monsterAnim.SetTrigger("Aanvallen");
    }

    void VerbergAlleTeksten()
    {
        if (tekstStart != null) tekstStart.SetActive(false);
        if (tekstPunt1 != null) tekstPunt1.SetActive(false);
        if (tekstPunt2 != null) tekstPunt2.SetActive(false);
    }
}
