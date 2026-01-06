using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 12f;

    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxPitch = 85f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintHeld;
    
    private Vector3 velocity;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraPivot == null)
        {
            Debug.LogError("No camera pivot set");
        }
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void Look()
    {
        float yaw = lookInput.x * mouseSensitivity;
        float pitchDelta = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up, yaw);
        
        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        
        if(cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);
        Vector3 desired = transform.TransformDirection(inputDir);

        float targetSpeed = sprintHeld ? sprintSpeed : walkSpeed;
        Vector3 targetVelXZ = desired * targetSpeed;

        Vector3 currentVelXZ = new Vector3(velocity.x, 0f, velocity.z);
        currentVelXZ = Vector3.MoveTowards(currentVelXZ, targetVelXZ, acceleration * Time.deltaTime);
        
        velocity.x = currentVelXZ.x;
        velocity.z = currentVelXZ.z;

        controller.Move(velocity * Time.deltaTime);
    }
    
    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    
    public void OnSprint(InputAction.CallbackContext ctx) => sprintHeld = ctx.ReadValueAsButton();
}
