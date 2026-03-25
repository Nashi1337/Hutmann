using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShovelInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private Transform rayOrigin;

    [Header("Shovel Requirement")]
    [SerializeField] private ItemDefinition requiredShovelItem;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    private bool _interactHeld;
    private bool _interactPressedThisFrame;

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

        if (!HasShovelEquipped())
        {
            _interactPressedThisFrame = false;
            return;
        }

        if (!TryGetDigTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget))
        {
            _interactPressedThisFrame = false;
            return;
        }
        
        Debug.Log($"Dig targets - Tap: {(tapTarget != null ? tapTarget.name : "None")}, Hold: {(holdTarget != null ? holdTarget.name : "None")}");;

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
        }
        else if (ctx.canceled)
        {
            _interactHeld = false;
        }
    }

    private bool TryGetDigTargets(out ShovelTapDigTarget tapTarget, out ShovelHoldDigTarget holdTarget)
    {
        tapTarget = null;
        holdTarget = null;

        if (!Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            return false;

        tapTarget = hit.collider.GetComponentInParent<ShovelTapDigTarget>();
        holdTarget = hit.collider.GetComponentInParent<ShovelHoldDigTarget>();

        return tapTarget != null || holdTarget != null;
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



