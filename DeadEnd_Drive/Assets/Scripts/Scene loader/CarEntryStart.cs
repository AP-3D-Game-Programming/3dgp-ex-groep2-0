using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupCarEntry : MonoBehaviour
{
    public CarEntry carEntryScript;
    void Start()
    {
        carEntryScript.EnterVehicle();
    }

}
