using UnityEngine;
using TMPro;

public class SimpleTreeRemover : MonoBehaviour
{
    public float range = 4f;
    public GameObject sawPickup; // Het zaag object
    public TextMeshProUGUI hintText;
    
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if(p != null) player = p.transform;
        if(hintText) hintText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) < range)
        {
            // Hebben we de zaag gepakt? (Is hij uit de wereld verdwenen?)
            bool hasSaw = (sawPickup == null || !sawPickup.activeSelf);

            if (!hasSaw)
            {
                if(hintText) { hintText.text = "I need a saw..."; hintText.gameObject.SetActive(true); }
            }
            else
            {
                if(hintText) { hintText.text = "[E] Cut Tree"; hintText.gameObject.SetActive(true); }
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Boom weg!
                    if(hintText) hintText.gameObject.SetActive(false);
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if(hintText) hintText.gameObject.SetActive(false);
        }
    }
}