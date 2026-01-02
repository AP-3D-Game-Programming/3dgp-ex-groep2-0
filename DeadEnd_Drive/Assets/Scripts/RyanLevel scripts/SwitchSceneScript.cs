using UnityEngine;

public class SwitchSceneScript : MonoBehaviour
{
    public string SceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        // Check of de player de trigger raakt en we nog niet gestart zijn
        if (other.CompareTag("Player"))
        {

        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);


        }
    }
}
