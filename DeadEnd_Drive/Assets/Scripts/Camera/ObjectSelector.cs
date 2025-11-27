using UnityEngine;                     // nodig voor Unity functies

public class ObjectSelector : MonoBehaviour
{
    public float detectDistance = 5f;      // afstand waarop speler kan selecteren
    public LayerMask selectableMask;       // layer van selecteerbare objecten

    private QuickOutline currentSelected;  // verwijzing naar huidig geselecteerd object
    private Camera cam;                    // verwijzing naar de camera

    void Start()
    {
        cam = GetComponent<Camera>();      // haalt de Camera op waarop dit script zit
    }

    void Update()
    {
        DetectLookedObject();              // elke frame checken wat speler aankijkt
    }

    void DetectLookedObject()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        // ↑ maakt een ray vanuit het midden van het scherm

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectDistance, selectableMask))
        {
            // ↓ haalt QuickOutline component op (IPV Outline!)
            QuickOutline outline = hit.collider.GetComponent<QuickOutline>();

            // als object een outline component heeft EN het niet al geselecteerd is
            if (outline != null && outline != currentSelected)
            {
                ClearCurrent();            // oude selectie verwijderen
                currentSelected = outline; // nieuwe selectie opslaan
                currentSelected.EnableOutline(); // outline aanzetten
            }
        }
        else
        {
            ClearCurrent();                // niks in zicht → deselecteer
        }
    }

    void ClearCurrent()
    {
        if (currentSelected != null)
        {
            currentSelected.DisableOutline(); // zet outline uit
            currentSelected = null;           // wis selectie
        }
    }
}
