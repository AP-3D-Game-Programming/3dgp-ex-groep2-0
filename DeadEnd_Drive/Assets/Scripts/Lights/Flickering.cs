using UnityEngine;

public class Flickering : MonoBehaviour
{
    private Light lamp;
    public float minIntensity = .5f;
    public float maxIntensity = 5.0f;
    public float flickerSpeed = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lamp = GetComponent<Light>();
        InvokeRepeating("Flicker", 0f, flickerSpeed);
    }

    private void Flicker()
    {
        float randomIntensity = Random.Range(minIntensity, maxIntensity);
        lamp.intensity = randomIntensity;
    }
}
