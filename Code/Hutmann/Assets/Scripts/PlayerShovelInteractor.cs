using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShovelInteractor : MonoBehaviour
{
    private enum LoadedDirtSource
    {
        None,
        Grave,
        Dump
    }

    private enum LookTargetType
    {
        None,
        Obstructed,
        DigTarget,
        DumpTarget
    }

    [Header("References")]
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform rayOrigin;

    [Header("Shovel Requirement")]
    [SerializeField] private ItemDefinition requiredShovelItem;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private bool includeTriggerColliders = true;

    [Header("Debug")]
    [SerializeField] private bool debugInteraction = false;
    [SerializeField] private Color debugDiggableColor = Color.green;
    [SerializeField] private Color debugObstructedColor = Color.red;
    [SerializeField] private Color debugNoHitColor = Color.yellow;

    private bool _interactHeld;
    private bool _interactPressedThisFrame;
    private LoadedDirtSource _loadedDirtSource = LoadedDirtSource.None;
    private ShovelLoadVisual _shovelLoadVisual;
    private GameObject _lastEquippedInstance;
    private GameObject _lastLookedObject;
    private LookTargetType _lastLookTargetType = LookTargetType.None;
    private string _currentLookLabel = "None";

    private void Awake()
    {
        if (equipment == null)
            equipment = GetComponent<PlayerEquipment>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (playerController != null)
            playerController.EquippedInstanceChanged += HandleEquippedInstanceChanged;

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

        RefreshShovelVisualBinding();
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.EquippedInstanceChanged -= HandleEquippedInstanceChanged;

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

        RefreshShovelVisualBinding();

        bool hasActionableTarget = TryGetTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget, out ShovelDumpTarget dumpTarget, out RaycastHit hit, out bool hasRaycastHit);

        if (debugInteraction)
        {
            DrawLookRay(hasRaycastHit, hasActionableTarget, hit.distance);
            LogLookTargetChanges(hasRaycastHit, hasActionableTarget, tapTarget, holdTarget, dumpTarget, hit);
        }

        if (!HasShovelEquipped())
        {
            _interactPressedThisFrame = false;
            return;
        }

        if (IsShovelLoaded())
        {
            if (_interactPressedThisFrame && TryUnloadToTarget(tapTarget, holdTarget, dumpTarget))
            {
                SetLoadedDirtSource(LoadedDirtSource.None);

                if (debugInteraction)
                    Debug.Log("[PlayerShovelInteractor] Dirt unloaded.");
            }
            else if (_interactPressedThisFrame && debugInteraction)
            {
                Debug.Log($"[PlayerShovelInteractor] Shovel is full ({_loadedDirtSource}). Unload at a valid target.");
            }
        }
        else
        {
            if (TryLoadFromTarget(tapTarget, holdTarget, dumpTarget))
            {
                if (debugInteraction)
                    Debug.Log($"[PlayerShovelInteractor] Shovel loaded from {_loadedDirtSource}.");
            }
        }

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

    private bool TryGetTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget, out ShovelDumpTarget dumpTarget, out RaycastHit hit, out bool hasRaycastHit)
    {
        tapTarget = null;
        holdTarget = null;
        dumpTarget = null;
        hit = default;

        QueryTriggerInteraction triggerInteraction = includeTriggerColliders
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        hasRaycastHit = Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, interactDistance, interactMask, triggerInteraction);

        if (!hasRaycastHit)
            return false;

        tapTarget = hit.collider.GetComponentInParent<ShovelTapDigTarget>();
        holdTarget = hit.collider.GetComponentInParent<ShovelHoldDigTarget>();
        dumpTarget = hit.collider.GetComponentInParent<ShovelDumpTarget>();

        if (!IsShovelLoaded())
        {
            bool canDigTap = tapTarget != null && tapTarget.CanDig;
            bool canDigHold = holdTarget != null && holdTarget.CanDig;
            bool canTakeDump = dumpTarget != null && dumpTarget.CanTakeLoad;
            return canDigTap || canDigHold || canTakeDump;
        }

        if (_loadedDirtSource == LoadedDirtSource.Grave)
            return dumpTarget != null && dumpTarget.CanAddLoad;

        bool canRefillTap = tapTarget != null && tapTarget.CanReceiveDirt;
        bool canRefillHold = holdTarget != null && holdTarget.CanReceiveDirt;
        return canRefillTap || canRefillHold;
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

    private void LogLookTargetChanges(bool hasRaycastHit, bool hasActionableTarget, ShovelTapDigTarget tapTarget, ShovelHoldDigTarget holdTarget, ShovelDumpTarget dumpTarget, RaycastHit hit)
    {
        GameObject lookedObject = null;
        LookTargetType lookTargetType = LookTargetType.None;

        if (hasRaycastHit)
        {
            if (hasActionableTarget)
            {
                if (IsShovelLoaded())
                {
                    if (_loadedDirtSource == LoadedDirtSource.Grave)
                    {
                        lookedObject = dumpTarget != null ? dumpTarget.gameObject : null;
                        lookTargetType = LookTargetType.DumpTarget;
                    }
                    else
                    {
                        lookedObject = tapTarget != null ? tapTarget.gameObject : holdTarget != null ? holdTarget.gameObject : null;
                        lookTargetType = LookTargetType.DigTarget;
                    }
                }
                else
                {
                    if (tapTarget != null || holdTarget != null)
                    {
                        lookedObject = tapTarget != null ? tapTarget.gameObject : holdTarget.gameObject;
                        lookTargetType = LookTargetType.DigTarget;
                    }
                    else
                    {
                        lookedObject = dumpTarget != null ? dumpTarget.gameObject : null;
                        lookTargetType = LookTargetType.DumpTarget;
                    }
                }
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

        if (lookTargetType == LookTargetType.DumpTarget)
            return $"DumpTarget: {lookedObject.name} ({distance}m)";

        return $"Obstructed by: {lookedObject.name} ({distance}m)";
    }

    public bool HasLoadedDirt => IsShovelLoaded();

    private bool IsShovelLoaded()
    {
        return _loadedDirtSource != LoadedDirtSource.None;
    }

    private bool TryLoadFromTarget(ShovelTapDigTarget tapTarget, ShovelHoldDigTarget holdTarget, ShovelDumpTarget dumpTarget)
    {
        if (_interactPressedThisFrame && tapTarget != null && tapTarget.TryDigOnce())
        {
            SetLoadedDirtSource(LoadedDirtSource.Grave);
            return true;
        }

        if (_interactHeld && holdTarget != null && holdTarget.TryDigHold(Time.deltaTime))
        {
            SetLoadedDirtSource(LoadedDirtSource.Grave);
            return true;
        }

        if (_interactPressedThisFrame && dumpTarget != null && dumpTarget.TryTakeLoad())
        {
            SetLoadedDirtSource(LoadedDirtSource.Dump);
            return true;
        }

        return false;
    }

    private bool TryUnloadToTarget(ShovelTapDigTarget tapTarget, ShovelHoldDigTarget holdTarget, ShovelDumpTarget dumpTarget)
    {
        if (_loadedDirtSource == LoadedDirtSource.Grave)
            return dumpTarget != null && dumpTarget.TryAddLoad();

        if (_loadedDirtSource == LoadedDirtSource.Dump)
        {
            if (tapTarget != null && tapTarget.TryAddBackOnce())
                return true;

            if (holdTarget != null && holdTarget.TryAddBackLoad())
                return true;
        }

        return false;
    }

    private void SetLoadedDirtSource(LoadedDirtSource source)
    {
        if (_loadedDirtSource == source)
            return;

        _loadedDirtSource = source;
        bool loaded = IsShovelLoaded();

        if (_shovelLoadVisual != null)
            _shovelLoadVisual.SetLoaded(loaded);
    }

    private void HandleEquippedInstanceChanged(GameObject _, ItemDefinition __)
    {
        RefreshShovelVisualBinding();
    }

    private void RefreshShovelVisualBinding()
    {
        if (playerController == null)
            return;

        GameObject equippedInstance = playerController.CurrentEquippedInstance;
        if (_lastEquippedInstance == equippedInstance)
            return;

        _lastEquippedInstance = equippedInstance;
        _shovelLoadVisual = null;

        if (equippedInstance != null)
            _shovelLoadVisual = equippedInstance.GetComponentInChildren<ShovelLoadVisual>(true);

        if (_shovelLoadVisual != null)
            _shovelLoadVisual.SetLoaded(IsShovelLoaded());
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



