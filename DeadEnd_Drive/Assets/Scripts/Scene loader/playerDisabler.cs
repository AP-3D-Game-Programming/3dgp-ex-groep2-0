using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
    public GameObject player; // assign the additive scene's player
    public Camera cam;        // assign the additive scene's camera

    private AudioListener audioListener;

    void Awake()
    {
        // Disable player
        if (player != null)
            player.SetActive(false);

        // Disable camera and its AudioListener
        if (cam != null)
        {
            cam.enabled = false;
            audioListener = cam.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }

    public void EnablePlayerAndCamera(Transform spawnPoint = null)
    {
        if (player != null)
        {
            player.SetActive(true);
            if (spawnPoint != null)
                player.transform.position = spawnPoint.position;

            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = true;
        }

        if (cam != null)
        {
            cam.enabled = true;
            if (audioListener != null)
                audioListener.enabled = true;
        }
    }
}
