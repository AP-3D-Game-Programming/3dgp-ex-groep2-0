using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Needed for UI

public class TreeTrigger : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject treeObject;
    public float fallDuration = 1.5f;
    public Vector3 fallRotation = new Vector3(0, 0, 90f);

    [Header("Black Screen Settings")]
    public CanvasGroup blackScreenGroup; // Drag your UI Panel here
    public float timeToImpact = 1.0f;    // How long after trigger until screen goes black?
    public float fadeSpeed = 2.0f;       // How fast it fades (Higher = Faster)

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("vehicle"))
        {
            triggered = true;
            Debug.Log("Tree Triggered! Impact imminent.");

            // Start the tree falling
            StartCoroutine(FallOverRoutine());

            // Start the countdown to the black screen
            StartCoroutine(FadeToBlackRoutine());
        }
    }

    IEnumerator FallOverRoutine()
    {
        Quaternion startRot = treeObject.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(fallRotation);

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            t = t * t; // Acceleration (gravity feel)

            treeObject.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
        treeObject.transform.rotation = endRot;
    }

    IEnumerator FadeToBlackRoutine()
    {
        // 1. Wait for the tree to swing down close to the car
        yield return new WaitForSeconds(timeToImpact);

        // 2. Fade the screen to black
        float fadeProgress = 0f;
        while (fadeProgress < 1f)
        {
            fadeProgress += Time.deltaTime * fadeSpeed;
            if (blackScreenGroup != null)
            {
                blackScreenGroup.alpha = fadeProgress;
            }
            yield return null;
        }

        // Ensure it's fully black at the end
        if (blackScreenGroup != null) blackScreenGroup.alpha = 1f;

        Debug.Log("Screen is Black. Load next level or stop game here.");
    }
}