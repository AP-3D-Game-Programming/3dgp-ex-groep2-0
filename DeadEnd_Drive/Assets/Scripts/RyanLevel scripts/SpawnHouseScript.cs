using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnHouseScript : MonoBehaviour
{
    public GameObject house;
    public TextMeshProUGUI dialog;
    public GameObject newTrigger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        house.gameObject.SetActive(false);


    }

    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            house.gameObject.SetActive(true);
            newTrigger.gameObject.SetActive(true);
            dialog.alpha = 1f;
            dialog.text = "Wait... I'm back in the same house? Something feels off...";
            dialog.gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }
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
