using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportOnTouch : MonoBehaviour
{
    public string additiveSceneName; // Name of the already loaded additive scene
    public Transform spawnPoint;     // Optional spawn point in the additive scene

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Disable old player
        other.gameObject.SetActive(false);

        // Optionally disable old camera if separate
        Camera oldCam = GameObject.FindWithTag("PlayerCamera")?.GetComponent<Camera>();
        if (oldCam != null)
            oldCam.enabled = false;

        // Find the scene controller in the additive scene
        foreach (GameObject rootObj in SceneManager.GetSceneByName(additiveSceneName).GetRootGameObjects())
        {
            DisableAtStart controller = rootObj.GetComponentInChildren<DisableAtStart>();
            if (controller != null)
            {
                controller.EnablePlayerAndCamera(spawnPoint);
                break;
            }
        }

        // Unload old scene
        SceneManager.UnloadSceneAsync(other.gameObject.scene);
    }
}
