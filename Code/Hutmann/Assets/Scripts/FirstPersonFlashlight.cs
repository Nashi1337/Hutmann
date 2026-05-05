using ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class FirstPersonFlashlight : MonoBehaviour
{
    [SerializeField] private Light flashlightLight;

    public void Initialize(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
            return;

        EnsureLight();
        ConfigureLight(itemDefinition);
    }

    private void EnsureLight()
    {
        if (flashlightLight != null)
            return;

        flashlightLight = GetComponentInChildren<Light>(true);
        if (flashlightLight != null)
            return;

        var lightObject = new GameObject("FlashlightBeam");
        lightObject.transform.SetParent(transform, false);
        flashlightLight = lightObject.AddComponent<Light>();
    }

    private void ConfigureLight(ItemDefinition itemDefinition)
    {
        flashlightLight.type = LightType.Spot;
        flashlightLight.enabled = true;
        flashlightLight.color = itemDefinition.flashlightColor;
        flashlightLight.intensity = Mathf.Max(0f, itemDefinition.flashlightIntensity);
        flashlightLight.range = Mathf.Max(0.1f, itemDefinition.flashlightRange);
        flashlightLight.spotAngle = Mathf.Clamp(itemDefinition.flashlightOuterSpotAngle, 1f, 179f);
        flashlightLight.innerSpotAngle = Mathf.Clamp(itemDefinition.flashlightInnerSpotAngle, 1f, flashlightLight.spotAngle);
        flashlightLight.shadows = itemDefinition.flashlightCastsShadows ? LightShadows.Soft : LightShadows.None;
        flashlightLight.renderMode = LightRenderMode.ForcePixel;
        flashlightLight.cullingMask = ~0;

        Transform lightTransform = flashlightLight.transform;
        lightTransform.localPosition = itemDefinition.flashlightLocalPosition;
        lightTransform.localRotation = Quaternion.Euler(itemDefinition.flashlightLocalEuler);
        lightTransform.localScale = Vector3.one;
    }

    public bool IsOn => flashlightLight != null && flashlightLight.enabled;

    public void Toggle()
    {
        if (flashlightLight == null) return;
        flashlightLight.enabled = !flashlightLight.enabled;
    }

    public void SetOn(bool on)
    {
        if (flashlightLight == null) return;
        flashlightLight.enabled = on;
    }

    private void OnDisable()
    {
        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }
}

