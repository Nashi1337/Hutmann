using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Drop this component on any GameObject to make it interactable.
/// The player needs a <see cref="PlayerInteractor"/> component to detect it.
/// </summary>
public class Interactable : MonoBehaviour
{
    [Tooltip("Shown in the tooltip: \"Press E to [actionText]\"")]
    [SerializeField] private string actionText = "interact";

    [Tooltip("Played once when the player successfully interacts.")]
    [SerializeField] private AudioClip interactSound;

    [Tooltip("Override interaction distance for this object specifically. Leave at -1 to use the PlayerInteractor default.")]
    [SerializeField] private float interactDistanceOverride = -1f;

    [Tooltip("When false the tooltip does not appear and interaction is blocked.")]
    [SerializeField] private bool isEnabled = true;

    [Space]
    [SerializeField] private UnityEvent onInteract;

    // ───────────────────────────────────── public API

    public string ActionText => actionText;
    public bool IsEnabled => isEnabled;

    /// <summary>Effective max distance used when raycasting from the interactor.</summary>
    public float GetInteractDistance(float defaultDistance) =>
        interactDistanceOverride > 0f ? interactDistanceOverride : defaultDistance;

    /// <summary>Called by <see cref="PlayerInteractor"/> when the player presses Interact.</summary>
    public void Interact(AudioSource audioSource)
    {
        if (!isEnabled) return;

        if (interactSound != null && audioSource != null)
            audioSource.PlayOneShot(interactSound);

        onInteract?.Invoke();
    }

    public void SetEnabled(bool value) => isEnabled = value;
}

