using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    public string sceneName = "";
    public CarEntry carEntryScript;
    void Start()
    {

        LoadSecondSceneAdditive();
    }
    public void LoadSecondSceneAdditive()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

    }

}
