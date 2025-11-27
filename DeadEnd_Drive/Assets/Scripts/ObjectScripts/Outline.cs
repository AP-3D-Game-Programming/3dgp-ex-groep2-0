using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    public Color outlineColor = Color.yellow;      // kleur van de outline
    public float outlineWidth = 0.1f;              // breedte van de outline in wereld-units
    public bool testEnableOnStart = false;         // nooit automatisch aan bij start

    private Material outlineMaterial;             // outline materiaal
    private Material[] originalMaterials;         // originele materials opslaan
    private Renderer rend;                        // renderer cache

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError($"Outline: geen Renderer gevonden op {gameObject.name}");
            return;
        }

        originalMaterials = rend.materials;

        Shader s = Shader.Find("Custom/QuickOutline");
        if (s == null)
        {
            Debug.LogError("Outline: shader 'Custom/QuickOutline' niet gevonden!");
            return;
        }

        // maak outline materiaal één keer
        outlineMaterial = new Material(s);
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaterial.hideFlags = HideFlags.HideAndDontSave;

        DisableOutline(); // start altijd uit
    }

    public void EnableOutline()
    {
        if (rend == null || outlineMaterial == null) return;

        Material[] mats = new Material[rend.materials.Length + 1];
        rend.materials.CopyTo(mats, 0);
        mats[mats.Length - 1] = outlineMaterial;
        rend.materials = mats;
    }

    public void DisableOutline()
    {
        if (rend == null || originalMaterials == null) return;

        rend.materials = originalMaterials; // zet originele materials terug
        // outlineMaterial behouden zodat EnableOutline() later werkt
    }

    public void SetOutlineColor(Color c)
    {
        outlineColor = c;
        if (outlineMaterial != null) outlineMaterial.SetColor("_OutlineColor", c);
    }

    public void SetOutlineWidth(float w)
    {
        outlineWidth = w;
        if (outlineMaterial != null) outlineMaterial.SetFloat("_OutlineWidth", w);
    }
}
