using UnityEngine;

public class TriggerBlock : MonoBehaviour
{
    public Transform player;
    public GameObject jumpScareObject;
    public float speed = 6f;
    public float stopDistance = .2f;
    public float destroyDelay = 0.5f;

    public AudioClip scareSound;      // 👈 Voeg dit toe in de Inspector!

    private bool triggered = false;

    private void Start()
    {
        jumpScareObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(JumpScareAttack());
        }
    }

    private System.Collections.IEnumerator JumpScareAttack()
    {
        // Spawn ver voor de speler
        jumpScareObject.transform.position = player.position + player.forward * 20f;

        // Kijk naar speler
        jumpScareObject.transform.LookAt(player);

        // Draai 180° zodat Quad zichtbaar is
        jumpScareObject.transform.Rotate(0f, 180f, 0f);

        // Maak zichtbaar
        jumpScareObject.SetActive(true);

        // 🎵 Speel jumpscare geluid af
        if (scareSound != null)
            AudioSource.PlayClipAtPoint(scareSound, player.position, 1f);

        // Vlieg richting speler
        while (Vector3.Distance(jumpScareObject.transform.position, player.position) > stopDistance)
        {
            jumpScareObject.transform.position = Vector3.MoveTowards(
                jumpScareObject.transform.position,
                player.position,
                speed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(jumpScareObject);
    }
}
