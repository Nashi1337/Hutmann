using UnityEngine;

public class ShovelHoldDigTarget : ShovelDigTargetBase
{
    [SerializeField] private float secondsToComplete = 4f;

    public bool IsComplete => heldSeconds >= secondsToComplete;

    private float heldSeconds;

    protected override void Awake()
    {
        base.Awake();
        secondsToComplete = Mathf.Max(0.01f, secondsToComplete);
    }

    public void InteractHold(float deltaTime)
    {
        if (IsComplete)
            return;

        heldSeconds = Mathf.Min(heldSeconds + deltaTime, secondsToComplete);
        SetProgress(heldSeconds / secondsToComplete);
    }
}


