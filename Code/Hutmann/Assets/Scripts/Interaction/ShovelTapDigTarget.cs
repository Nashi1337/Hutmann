using UnityEngine;

public class ShovelTapDigTarget : ShovelDigTargetBase
{
    [SerializeField] private int requiredInteractions = 4;

    public bool IsComplete => completedInteractions >= requiredInteractions;

    private int completedInteractions;

    protected override void Awake()
    {
        base.Awake();
        requiredInteractions = Mathf.Max(1, requiredInteractions);
    }

    public void InteractOnce()
    {
        if (IsComplete)
            return;

        completedInteractions++;
        SetProgress(completedInteractions / (float)requiredInteractions);
    }
}


