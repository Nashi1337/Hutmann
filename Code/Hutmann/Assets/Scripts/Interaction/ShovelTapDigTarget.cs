using UnityEngine;

public class ShovelTapDigTarget : ShovelDigTargetBase
{
    [SerializeField] private int requiredInteractions = 4;

    public bool IsComplete => completedInteractions >= requiredInteractions;
    public bool CanDig => completedInteractions < requiredInteractions;
    public bool CanReceiveDirt => completedInteractions > 0;

    private int completedInteractions;

    protected override void Awake()
    {
        base.Awake();
        requiredInteractions = Mathf.Max(1, requiredInteractions);
    }

    public bool TryDigOnce()
    {
        if (!CanDig)
            return false;

        completedInteractions++;
        SetProgress(completedInteractions / (float)requiredInteractions);
        return true;
    }

    public bool TryAddBackOnce()
    {
        if (!CanReceiveDirt)
            return false;

        completedInteractions--;
        SetProgress(completedInteractions / (float)requiredInteractions);
        return true;
    }

    public void InteractOnce()
    {
        TryDigOnce();
    }

    protected override void OnComplete()
    {
        // Keep grave in scene so dirt can be moved back from dump piles.
    }
}


