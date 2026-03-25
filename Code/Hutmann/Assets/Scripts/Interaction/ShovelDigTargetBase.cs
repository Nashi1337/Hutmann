using UnityEngine;

public enum FillAxis
{
    X,
    Y,
    Z
}

public abstract class ShovelDigTargetBase : MonoBehaviour
{
    [Header("Visual Fill")]
    [SerializeField] private Transform fillVisual;
    [SerializeField] private FillAxis fillAxis = FillAxis.Y;
    [SerializeField] private float minimumAxisScale = 0.05f;
    [SerializeField] private Renderer fillRenderer;
    [SerializeField] private Color fullColor = new Color32(120, 72, 40, 255);
    [SerializeField] private Color emptyColor = new Color32(35, 20, 12, 255);

    private Vector3 initialScale;
    private MaterialPropertyBlock propertyBlock;

    protected float Progress01 { get; private set; }

    protected virtual void Awake()
    {
        if (fillVisual == null)
            fillVisual = transform;

        if (fillRenderer == null)
            fillRenderer = GetComponentInChildren<Renderer>();

        initialScale = fillVisual.localScale;
        propertyBlock = new MaterialPropertyBlock();

        SetProgress(0f);
    }

    protected void SetProgress(float progress01)
    {
        Progress01 = Mathf.Clamp01(progress01);
        float fill01 = 1f - Progress01;

        UpdateScale(fill01);
        UpdateColor(fill01);
    }

    private void UpdateScale(float fill01)
    {
        Vector3 scaled = initialScale;
        float axisScale = Mathf.Lerp(minimumAxisScale, 1f, fill01);

        switch (fillAxis)
        {
            case FillAxis.X:
                scaled.x = initialScale.x * axisScale;
                break;
            case FillAxis.Y:
                scaled.y = initialScale.y * axisScale;
                break;
            case FillAxis.Z:
                scaled.z = initialScale.z * axisScale;
                break;
        }

        fillVisual.localScale = scaled;
    }

    private void UpdateColor(float fill01)
    {
        if (fillRenderer == null)
            return;

        Color fillColor = Color.Lerp(emptyColor, fullColor, fill01);
        fillRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", fillColor);
        propertyBlock.SetColor("_BaseColor", fillColor);
        fillRenderer.SetPropertyBlock(propertyBlock);
    }
}

