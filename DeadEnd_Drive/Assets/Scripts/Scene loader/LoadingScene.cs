using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public Image blackOverlay;
    public string sceneName;
    public float fadeDuration = 0.5f;
    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;

        Transform root = other.transform.root;

        if (HasAnyChildWithTag(root.gameObject, "vehicle"))
        {
            isLoading = true;
            StartCoroutine(FadeAndLoad());
        }
    }


    private bool HasAnyChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag(tag))
                return true;
        }
        return false;
    }

    private IEnumerator FadeAndLoad()
    {
        float elapsed = 0f;
        Color c = blackOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            blackOverlay.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
