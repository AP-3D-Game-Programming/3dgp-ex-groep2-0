using UnityEngine;

public class ShooseClass : MonoBehaviour
{
    public Canvas graphics;
    public Canvas titleScreen;
    public StartGame startScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ToggleGraphicsMenu()
    {
        bool isActive = graphics.gameObject.activeSelf;
        graphics.gameObject.SetActive(!isActive);
        titleScreen.gameObject.SetActive(isActive);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartButton(string sceneName)
    {
        if (startScript != null)
        {
            startScript.startCall(); // triggers the coroutine
        }
    }
}
