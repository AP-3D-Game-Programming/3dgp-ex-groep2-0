using System.Collections;
using TMPro;
using UnityEngine;

public class NewHouseScript : MonoBehaviour
{
    public GameObject house;
    public GameObject jerryCan;
    public GameObject newTrigger;
    private bool triggered;
    public TextMeshProUGUI dialog;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        triggered = false;
    }

    private void Update()
    {
        if (!triggered)
            newTrigger.gameObject.SetActive(false);
    }

    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            DeleteHouseAndJerrycan();
            AddNewTrigger();
            dialog.alpha = 1f;
            dialog.gameObject.SetActive(true);
            dialog.text = "Hey! Where did the jerrycan go? I really don't like this place...";
            dialog.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }
    }

    private void DeleteHouseAndJerrycan()
    {
        jerryCan.gameObject.SetActive(false);
        house.gameObject.SetActive(false);
    }

    private void AddNewTrigger()
    {
        newTrigger.gameObject.SetActive(true);

    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(5f);

        while (dialog.alpha > 0)
        {
            dialog.alpha -= Time.deltaTime * 2f;
            yield return null;
        }

        dialog.gameObject.SetActive(false);
    }
}
