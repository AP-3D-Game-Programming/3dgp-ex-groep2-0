using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public float diveSpeed = 100f;
    public string nextSceneName;
    public GameObject TopCamera;
    public Canvas menuCanvas;
    public Image blackScreen;

    public void StartButton()
    {
        StartCoroutine(DiveAndLoad());
    }

    private System.Collections.IEnumerator DiveAndLoad()
    {
        if (menuCanvas != null)
            menuCanvas.enabled = false;

        while (TopCamera.transform.position.y > 45f)
        {
            TopCamera.transform.position += Vector3.down * diveSpeed * Time.deltaTime;
            yield return null;
        }

        if (blackScreen != null)
        {
            float alpha = 0f;
            blackScreen.gameObject.SetActive(true);

            while (alpha < 1f)
            {
                alpha += Time.deltaTime * 2f;
                blackScreen.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
