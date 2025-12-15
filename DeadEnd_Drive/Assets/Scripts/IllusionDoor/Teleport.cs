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

        // --- HIER BEGINT DE LOOP ---
        // We lopen door alle hoofd-objecten in de andere scene
        Scene targetScene = SceneManager.GetSceneByName(additiveSceneName);
        if (targetScene.IsValid()) // Check of scene geladen is voor de zekerheid
        {
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                // 1. Zoek de Player Controller (DisableAtStart)
                DisableAtStart controller = rootObj.GetComponentInChildren<DisableAtStart>();
                if (controller != null)
                {
                    controller.EnablePlayerAndCamera(spawnPoint);
                }

                // 2. Zoek het Dialoog script (DIT MOET OOK BINNEN DE LOOP)
                StartDialogueLevel1 dialogue = rootObj.GetComponentInChildren<StartDialogueLevel1>();
                if (dialogue != null)
                {
                    dialogue.ShowDialogue();
                }
            }
        }
        // --- HIER EINDIGT DE LOOP (pas hier mag het haakje staan) ---

        // Unload old scene
        SceneManager.UnloadSceneAsync(other.gameObject.scene);
    }
}