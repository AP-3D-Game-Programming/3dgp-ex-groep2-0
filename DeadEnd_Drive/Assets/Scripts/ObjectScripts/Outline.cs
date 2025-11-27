using UnityEngine;

[DisallowMultipleComponent]
public class QuickOutline : MonoBehaviour
{
    public Color outlineColor = Color.white;   // de kleur van de outline
    public float outlineWidth = 4f;            // dikte van de outline

    private Material outlineMaterial;          // opslag voor outline materiaal
    private Material[] originalMaterials;      // om originele materialen te bewaren

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();  
        // ↑ haalt de renderer op zodat we toegang hebben tot de materials

        originalMaterials = rend.materials;  
        // ↑ slaat de materials op zodat we ze kunnen terugzetten als outline uit gaat

        outlineMaterial = new Material(Shader.Find("Outlined/Uniform")); 
        // ↑ maakt een outline materiaal aan (shader geef ik hieronder)

        outlineMaterial.SetColor("_OutlineColor", outlineColor); 
        // ↑ kleur instellen

        outlineMaterial.SetFloat("_Outline", outlineWidth);      
        // ↑ breedte instellen

        DisableOutline(); 
        // ↑ standaard outline uit
    }

    public void EnableOutline()
    {
        Renderer rend = GetComponent<Renderer>();  
        // ↑ opnieuw renderer ophalen

        Material[] mats = new Material[rend.materials.Length + 1]; 
        // ↑ nieuw array maken met 1 extra slot voor de outline

        rend.materials.CopyTo(mats, 0); 
        // ↑ originele materials kopiëren naar nieuw array

        mats[mats.Length - 1] = outlineMaterial; 
        // ↑ laatste slot vullen met onze outline laag

        rend.materials = mats;  
        // ↑ renderer krijgt nu materialen inclusief outline
    }

    public void DisableOutline()
    {
        Renderer rend = GetComponent<Renderer>();  
        // ↑ renderer ophalen

        rend.materials = originalMaterials;     
        // ↑ originele materialen terugzetten
    }
}
