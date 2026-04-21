using UnityEngine;

public class ShovelDumpTarget : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private int maxLoads = 16;
    [SerializeField] private int currentLoads;

    [Header("Visual")]
    [SerializeField] private Transform fillVisual;
    [SerializeField] private float minHeightScale = 0.08f;
    [SerializeField] private bool growUpwards = true;

    [SerializeField] private bool debugLogDump;

    private Vector3 initialScale;
    private Vector3 initialPosition;

    public bool CanAddLoad => currentLoads < maxLoads;
    public bool CanTakeLoad => currentLoads > 0;

    private void Awake()
    {
        maxLoads = Mathf.Max(1, maxLoads);
        currentLoads = Mathf.Clamp(currentLoads, 0, maxLoads);

        if (fillVisual == null)
            fillVisual = transform;

        initialScale = fillVisual.localScale;
        initialPosition = fillVisual.localPosition;

        UpdateVisual();
    }

    public bool TryAddLoad()
    {
        if (!CanAddLoad)
            return false;

        currentLoads++;
        UpdateVisual();

        if (debugLogDump)
            Debug.Log($"[ShovelDumpTarget] Added dirt to '{name}' ({currentLoads}/{maxLoads}).");

        return true;
    }

    public bool TryTakeLoad()
    {
        if (!CanTakeLoad)
            return false;

        currentLoads--;
        UpdateVisual();

        if (debugLogDump)
            Debug.Log($"[ShovelDumpTarget] Took dirt from '{name}' ({currentLoads}/{maxLoads}).");

        return true;
    }

    private void UpdateVisual()
    {
        if (fillVisual == null)
            return;

        float fill01 = currentLoads / (float)maxLoads;
        float heightScale = Mathf.Lerp(minHeightScale, 1f, fill01);

        Vector3 scaled = initialScale;
        scaled.y = initialScale.y * heightScale;
        fillVisual.localScale = scaled;

        if (!growUpwards)
            return;

        Vector3 pos = initialPosition;
        float scaleDifference = heightScale - 1f;
        pos.y += (scaleDifference * initialScale.y) * 0.5f;
        fillVisual.localPosition = pos;
    }
}


