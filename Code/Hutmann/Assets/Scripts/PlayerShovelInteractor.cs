using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShovelInteractor : MonoBehaviour
{
    private enum LookTargetType
    {
        None,
        Obstructed,
        DigTarget
    }

    [Header("References")]
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private Transform rayOrigin;

    [Header("Shovel Requirement")]
    [SerializeField] private ItemDefinition requiredShovelItem;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool debugInteraction = false;
    [SerializeField] private Color debugDiggableColor = Color.green;
    [SerializeField] private Color debugObstructedColor = Color.red;
    [SerializeField] private Color debugNoHitColor = Color.yellow;

    private bool _interactHeld;
    private bool _interactPressedThisFrame;
    private GameObject _lastLookedObject;
    private LookTargetType _lastLookTargetType = LookTargetType.None;
    private string _currentLookLabel = "None";

    private void Awake()
    {
        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();

        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.started += OnInteract;
                interactAction.canceled += OnInteract;
            }
        }
    }

    private void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var interactAction = playerInput.actions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.started -= OnInteract;
                interactAction.canceled -= OnInteract;
            }
        }
    }

    private void Update()
    {
        if (rayOrigin == null || equipment == null)
        {
            _interactPressedThisFrame = false;
            return;
        }

        bool hasDigTargets = TryGetDigTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget, out RaycastHit hit, out bool hasRaycastHit);

        if (debugInteraction)
        {
            DrawLookRay(hasRaycastHit, hasDigTargets, hit.distance);
            LogLookTargetChanges(hasRaycastHit, hasDigTargets, tapTarget, holdTarget, hit);
        }

        if (!HasShovelEquipped())
        {
            _interactPressedThisFrame = false;
            return;
        }

        if (!hasDigTargets)
        {
            _interactPressedThisFrame = false;
            return;
        }

        if (_interactPressedThisFrame && tapTarget != null)
            tapTarget.InteractOnce();

        if (_interactHeld && holdTarget != null)
            holdTarget.InteractHold(Time.deltaTime);

        _interactPressedThisFrame = false;
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _interactHeld = true;
            _interactPressedThisFrame = true;

            if (debugInteraction)
                Debug.Log($"[PlayerShovelInteractor] Interact pressed. Looking at: {_currentLookLabel}");
        }
        else if (ctx.canceled)
        {
            _interactHeld = false;

            if (debugInteraction)
                Debug.Log("[PlayerShovelInteractor] Interact released.");
        }
    }

    private bool TryGetDigTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget, out RaycastHit hit, out bool hasRaycastHit)
    {
        tapTarget = null;
        holdTarget = null;
        hit = default;
        hasRaycastHit = Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore);

        if (!hasRaycastHit)
            return false;

        tapTarget = hit.collider.GetComponentInParent<ShovelTapDigTarget>();
        holdTarget = hit.collider.GetComponentInParent<ShovelHoldDigTarget>();

        return tapTarget != null || holdTarget != null;
    }

    private void DrawLookRay(bool hasRaycastHit, bool hasDigTargets, float hitDistance)
    {
        float rayLength = hasRaycastHit ? hitDistance : interactDistance;

        Color rayColor;
        if (!hasRaycastHit)
            rayColor = debugNoHitColor;
        else if (hasDigTargets)
            rayColor = debugDiggableColor;
        else
            rayColor = debugObstructedColor;

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayLength, rayColor);
    }

    private void LogLookTargetChanges(bool hasRaycastHit, bool hasDigTargets, ShovelTapDigTarget tapTarget, ShovelHoldDigTarget holdTarget, RaycastHit hit)
    {
        GameObject lookedObject = null;
        LookTargetType lookTargetType = LookTargetType.None;

        if (hasRaycastHit)
        {
            if (hasDigTargets)
            {
                lookedObject = tapTarget != null ? tapTarget.gameObject : holdTarget.gameObject;
                lookTargetType = LookTargetType.DigTarget;
            }
            else
            {
                lookedObject = hit.collider.gameObject;
                lookTargetType = LookTargetType.Obstructed;
            }
        }

        _currentLookLabel = BuildLookLabel(lookedObject, lookTargetType, hasRaycastHit, hit.distance);

        if (lookedObject == _lastLookedObject && lookTargetType == _lastLookTargetType)
            return;

        Debug.Log($"[PlayerShovelInteractor] Look target changed: {_currentLookLabel}");
        _lastLookedObject = lookedObject;
        _lastLookTargetType = lookTargetType;
    }

    private string BuildLookLabel(GameObject lookedObject, LookTargetType lookTargetType, bool hasRaycastHit, float hitDistance)
    {
        if (!hasRaycastHit)
            return "None";

        string distance = hitDistance.ToString("0.00");

        if (lookedObject == null)
            return $"Unknown ({distance}m)";

        if (lookTargetType == LookTargetType.DigTarget)
            return $"DigTarget: {lookedObject.name} ({distance}m)";

        return $"Obstructed by: {lookedObject.name} ({distance}m)";
    }

    private bool HasShovelEquipped()
    {
        ItemDefinition currentItem = equipment.CurrentItem;

        if (currentItem == null)
            return false;

        if (requiredShovelItem != null)
            return currentItem == requiredShovelItem;

        return currentItem.itemType == ItemType.Shovel;
    }
}



