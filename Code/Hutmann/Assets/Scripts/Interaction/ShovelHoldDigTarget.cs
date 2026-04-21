using UnityEngine;

public class ShovelHoldDigTarget : ShovelDigTargetBase
{
    [SerializeField] private float secondsToComplete = 4f;
    [SerializeField] private int requiredLoads = 4;

    public bool IsComplete => completedLoads >= requiredLoads;
    public bool CanDig => completedLoads < requiredLoads;
    public bool CanReceiveDirt => completedLoads > 0;

    private float heldSeconds;
    private int completedLoads;

    protected override void Awake()
    {
        base.Awake();
        secondsToComplete = Mathf.Max(0.01f, secondsToComplete);
        requiredLoads = Mathf.Max(1, requiredLoads);
    }

    public bool TryDigHold(float deltaTime)
    {
        if (!CanDig)
            return false;

        float secondsPerLoad = secondsToComplete / requiredLoads;
        heldSeconds += Mathf.Max(0f, deltaTime);

        if (heldSeconds < secondsPerLoad)
            return false;

        heldSeconds = 0f;
        completedLoads = Mathf.Min(completedLoads + 1, requiredLoads);
        SetProgress(completedLoads / (float)requiredLoads);
        return true;
    }

    public bool TryAddBackLoad()
    {
        if (!CanReceiveDirt)
            return false;

        heldSeconds = 0f;
        completedLoads = Mathf.Max(completedLoads - 1, 0);
        SetProgress(completedLoads / (float)requiredLoads);
        return true;
    }

    public void InteractHold(float deltaTime)
    {
        TryDigHold(deltaTime);
    }

    protected override void OnComplete()
    {
        // Keep grave in scene so dirt can be moved back from dump piles.
    }
}


