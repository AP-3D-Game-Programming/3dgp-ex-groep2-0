using UnityEngine;
using UnityEngine.SceneManagement;

public class AddSceneLoader : MonoBehaviour
{
    public string sceneName = "";
    void Start()
    {
        LoadSecondSceneAdditive();
    }
    public void LoadSecondSceneAdditive()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

    }

}
