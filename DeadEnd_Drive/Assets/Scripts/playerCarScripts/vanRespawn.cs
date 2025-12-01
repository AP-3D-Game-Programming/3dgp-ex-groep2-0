using UnityEngine;

public class VanRespawner : MonoBehaviour
{
    [SerializeField] private GameObject van;
    [SerializeField] private Transform player;

    private Vector3 spawn = new Vector3(96.25f, 104.74f, 106.02f);

    private Rigidbody vanRb;

    void Start()
    {
        if (van == null)
        {
            Debug.LogError("Van reference is missing!");
            return;
        }

        vanRb = van.GetComponent<Rigidbody>();
        if (vanRb == null)
        {
            Debug.LogWarning("Van has no Rigidbody. Velocity reset will be skipped.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnVan();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            respawnPlayer();
        }
    }

    void RespawnVan()
    {
        if (vanRb != null)
        {
            vanRb.linearVelocity = Vector3.zero;
            vanRb.angularVelocity = Vector3.zero;
        }

        Vector3 offset = player.transform.right * -3f;
        van.transform.position = player.position + offset;

        Vector3 euler = van.transform.eulerAngles;
        euler.y = player.eulerAngles.y;
        van.transform.rotation = Quaternion.Euler(euler);
    }
    public Vector3 playerOffset = new Vector3(5f, 5f, 0f);
    void respawnPlayer()
    {
        van.transform.position = spawn;
        van.transform.rotation = Quaternion.identity;

        Vector3 worldOffset = van.transform.TransformDirection(playerOffset);
        player.transform.position = spawn + worldOffset;
    }
}
