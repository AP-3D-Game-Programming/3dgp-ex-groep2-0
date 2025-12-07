using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

public class GraphicsSettings : MonoBehaviour
{
    [Header("Dropdowns")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown shadowDistanceDropdown;
    public TMP_Dropdown shadowCascadeDropdown;


    [Header("URP Asset")]
    public UniversalRenderPipelineAsset urpAsset;

    [Header("MenuLoading")]
    public Canvas graphics;
    public Canvas titleScreen;

    public string sceneToLoad;
    void Start()
    {
        graphics.gameObject.SetActive(false);
        qualityDropdown.onValueChanged.AddListener(SetQualityPreset);
        shadowDistanceDropdown.onValueChanged.AddListener(SetShadowDistance);
        shadowCascadeDropdown.onValueChanged.AddListener(SetCascadeCount);

        InitializeDropdowns();
    }

    void InitializeDropdowns()
    {
        qualityDropdown.value = 1;

        shadowDistanceDropdown.value = 1;

        shadowCascadeDropdown.value = 1;

        urpAsset.shadowDistance = 50f;
        urpAsset.shadowCascadeCount = 2;

        QualitySettings.SetQualityLevel(1);
    }
    void SetQualityPreset(int index)
    {
        switch (index)
        {
            case 0: // Low
                urpAsset.shadowDistance = 20f;
                urpAsset.shadowCascadeCount = 1;
                shadowDistanceDropdown.value = 0;
                shadowCascadeDropdown.value = 0;
                QualitySettings.SetQualityLevel(0);
                break;
            case 1: // Medium
                urpAsset.shadowDistance = 50f;
                urpAsset.shadowCascadeCount = 2;
                shadowDistanceDropdown.value = 1;
                shadowCascadeDropdown.value = 1;
                QualitySettings.SetQualityLevel(1);
                break;
            case 2: // High
                urpAsset.shadowDistance = 100f;
                urpAsset.shadowCascadeCount = 4;
                shadowDistanceDropdown.value = 2;
                shadowCascadeDropdown.value = 3;
                QualitySettings.SetQualityLevel(2);
                break;
        }
    }

    void SetShadowDistance(int index)
    {
        // Map dropdown index to distance
        float[] distances = { 20f, 50f, 100f, 200f };
        urpAsset.shadowDistance = distances[Mathf.Clamp(index, 0, distances.Length - 1)];
    }

    void SetCascadeCount(int index)
    {
        int[] cascades = { 1, 2, 3, 4 };
        urpAsset.shadowCascadeCount = cascades[Mathf.Clamp(index, 0, cascades.Length - 1)];
    }
    public void openOptions()
    {
        if (!graphics)
        {
            titleScreen.gameObject.SetActive(false);
            graphics.gameObject.SetActive(true);
        }
        else
        {
            titleScreen.gameObject.SetActive(true);
            graphics.gameObject.SetActive(false);
        }

    }
}
