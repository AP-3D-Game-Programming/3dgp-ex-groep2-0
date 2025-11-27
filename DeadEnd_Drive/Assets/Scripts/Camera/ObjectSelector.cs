using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    public float detectDistance = 5f;      // afstand waarop speler kan selecteren
    public LayerMask selectableMask;       // layer van selecteerbare objecten

    private Outline currentSelected;       // huidig geselecteerd object
    private Camera cam;                     // camera

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        DetectLookedObject();
    }

    void DetectLookedObject()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, detectDistance, selectableMask))
        {
            Outline outline = hit.collider.GetComponent<Outline>();
            if (outline != null && outline != currentSelected)
            {
                ClearCurrent();
                currentSelected = outline;
                currentSelected.EnableOutline();
            }
        }
        else
        {
            ClearCurrent();
        }
    }

    void ClearCurrent()
    {
        if (currentSelected != null)
        {
            currentSelected.DisableOutline();
            currentSelected = null;
        }
    }
}
