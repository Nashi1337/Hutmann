using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// General-purpose interaction handler.
/// Add to Player. Raycasts each frame, shows a tooltip when looking at an
/// <see cref="Interactable "/>, and fires it when the Interact action is pressed.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("References (auto-found if null)")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private InteractionTooltipUI tooltip;
    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private bool includeTriggers   = true;
    [Header("Key Hint")]
    [Tooltip("Shown in the tooltip. Auto-populated from the Interact binding if left empty.")]
    [SerializeField] private string keyHintOverride = "";
    private Interactable _current;
    private AudioSource  _audioSource;
    private InputAction  _interactAction;
    private bool         _pressedThisFrame;
    private string       _resolvedKeyHint = "E";
    // --- Lifecycle ------------------------------------------------------------
    private void Awake()
    {
        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }
    private void OnEnable()
    {
        var pi = GetComponent<PlayerInput>();
        if (pi != null)
        {
            _interactAction = pi.actions.FindAction("Interact");
            if (_interactAction != null)
            {
                _interactAction.started  += OnInteractInput;
                _resolvedKeyHint = ResolveKeyHint(_interactAction);
            }
        }
        if (!string.IsNullOrEmpty(keyHintOverride))
            _resolvedKeyHint = keyHintOverride;
    }
    private void OnDisable()
    {
        if (_interactAction != null)
            _interactAction.started -= OnInteractInput;
        _current = null;
        tooltip?.Hide();
    }
    private void Start()
    {
        if (tooltip == null)
            tooltip = InteractionTooltipUI.CreateDefault();
    }
    private void Update()
    {
        UpdateLook();
        _pressedThisFrame = false;
    }
    // --- Input callback -------------------------------------------------------
    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        _pressedThisFrame = true;
        if (_current != null && _current.IsEnabled)
            _current.Interact(_audioSource);
    }
    // --- Raycast --------------------------------------------------------------
    private void UpdateLook()
    {
        if (rayOrigin == null) return;
        var triggerMode = includeTriggers
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;
        Interactable found = null;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward,
                out RaycastHit hit, interactDistance, interactMask, triggerMode))
        {
            var candidate = hit.collider.GetComponentInParent<Interactable>();
            if (candidate != null && candidate.IsEnabled)
            {
                // Respect per-object distance override
                float maxDist = candidate.GetInteractDistance(interactDistance);
                if (hit.distance <= maxDist)
                    found = candidate;
            }
        }
        if (found != _current)
        {
            _current = found;
            if (_current != null)
                tooltip?.Show(_resolvedKeyHint, _current.ActionText);
            else
                tooltip?.Hide();
        }
    }
    // --- Key hint helper ------------------------------------------------------
    private static string ResolveKeyHint(InputAction action)
    {
        // Walk bindings and return the first non-composite, non-gamepad one
        foreach (var binding in action.bindings)
        {
            if (binding.isComposite || string.IsNullOrEmpty(binding.path)) continue;
            if (binding.path.StartsWith("<Gamepad>")) continue;
            // e.g. "<Keyboard>/e"  ->  "E"
            int slash = binding.path.LastIndexOf('/');
            if (slash >= 0)
            {
                string key = binding.path.Substring(slash + 1);
                // Capitalise single characters
                if (key.Length == 1) return key.ToUpper();
                // Nice names for common ones
                return key switch
                {
                    "leftButton"  => "LMB",
                    "rightButton" => "RMB",
                    "space"       => "Space",
                    "enter"       => "Enter",
                    "f"           => "F",
                    "e"           => "E",
                    _             => key
                };
            }
        }
        return "E";
    }
}
