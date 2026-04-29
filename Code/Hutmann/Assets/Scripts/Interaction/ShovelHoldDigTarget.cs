using UnityEngine;

public class ShovelHoldDigTarget : ShovelDigTargetBase
{
    [SerializeField] private float secondsToComplete = 4f;
    [SerializeField] private int requiredLoads = 4;

    public bool IsComplete => completedLoads >= requiredLoads;
    public bool CanDig => !IsComplete;
    public bool CanReceiveDirt => completedLoads > 0;

    private float currentLoadSeconds;
    private int completedLoads;

    protected override void Awake()
    {
        base.Awake();
        secondsToComplete = Mathf.Max(0.01f, secondsToComplete);
        requiredLoads = Mathf.Max(1, requiredLoads);
    }

    public bool TryDigHold(float deltaTime)
    {
        if (IsComplete)
            return false;

        float secondsPerLoad = secondsToComplete / requiredLoads;
        currentLoadSeconds += deltaTime;

        if (currentLoadSeconds < secondsPerLoad)
            return false;

        currentLoadSeconds = 0f;
        completedLoads = Mathf.Min(completedLoads + 1, requiredLoads);
        SetProgress(completedLoads / (float)requiredLoads);
        return true;
    }

    public void InteractHold(float deltaTime)
    {
        TryDigHold(deltaTime);
    }

    public bool TryAddBackLoad()
    {
        if (!CanReceiveDirt)
            return false;

        completedLoads--;
        currentLoadSeconds = 0f;
        SetProgress(completedLoads / (float)requiredLoads);
        return true;
    }

    protected override void OnComplete()
    {
        // Keep the grave target alive so dirt can be moved back into it.
    }
}


