using UnityEngine;

public class SpawnHouseScript : MonoBehaviour
{
    public GameObject house;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        house.gameObject.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            house.gameObject.SetActive(true);
        }
    }
}
