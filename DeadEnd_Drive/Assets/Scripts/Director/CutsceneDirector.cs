using UnityEngine;

public class CutsceneDirector : MonoBehaviour
{

    [Header("Objects to swap")]
    public GameObject cutsceneCar;
    public GameObject carCamera;
    public GameObject realPlayer;

    [Header("UI")]
    public GameObject exitPromptUI;

    private bool isReadyToExit = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (realPlayer) realPlayer.SetActive(false);
        if (cutsceneCar) cutsceneCar.SetActive(true);
        if (carCamera) carCamera.SetActive(true);
        if (exitPromptUI) exitPromptUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isReadyToExit && Input.GetKeyDown(KeyCode.F))
        {
            PerformSwap();
        }
    }

    public void EnableExitPrompt()
    {
        isReadyToExit = true;
        if (exitPromptUI) exitPromptUI.SetActive(true);
    }

    public void PerformSwap()
    {
        if (exitPromptUI) exitPromptUI.SetActive(false);

        if (carCamera) carCamera.SetActive(false);

        if (cutsceneCar) cutsceneCar.SetActive(true);

        if (realPlayer) realPlayer.SetActive(true);

        this.enabled = false;
    }
}
