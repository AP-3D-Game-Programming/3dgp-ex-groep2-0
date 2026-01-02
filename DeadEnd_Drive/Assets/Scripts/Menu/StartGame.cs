using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public float moveSpeed = 5f;
    public string nextSceneName;
    public GameObject TopCamera;
    public Canvas menuCanvas;
    public Image blackScreen;

    public void startCall()
    {
        StartCoroutine(MoveAndLoad());
    }

    private System.Collections.IEnumerator MoveAndLoad()
    {
        if (menuCanvas != null)
            menuCanvas.enabled = false;

        Vector3 startPos = TopCamera.transform.position;
        Vector3 targetPos = startPos + TopCamera.transform.forward * 20f;

        while (Vector3.Distance(TopCamera.transform.position, targetPos) > 0.01f)
        {
            TopCamera.transform.position = Vector3.MoveTowards(
                TopCamera.transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
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
