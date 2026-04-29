using UnityEngine;

public class ShovelLoadVisual : MonoBehaviour
{
    [Header("Visual Variants")]
    [SerializeField] private GameObject emptyShovelModel;
    [SerializeField] private GameObject loadedShovelModel;
    [SerializeField] private bool startsLoaded;

    public bool IsLoaded { get; private set; }

    private void Awake()
    {
        SetLoaded(startsLoaded);
    }

    public void SetLoaded(bool loaded)
    {
        IsLoaded = loaded;

        if (emptyShovelModel != null)
            emptyShovelModel.SetActive(!loaded);

        if (loadedShovelModel != null)
            loadedShovelModel.SetActive(loaded);
    }
}

