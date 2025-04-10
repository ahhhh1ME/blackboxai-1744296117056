using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundDistance = 0.4f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private bool invertY = false;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayer;

    // Component references
    private CharacterController characterController;
    private Camera playerCamera;
    private Transform groundCheck;

    // Movement variables
    private Vector3 moveDirection;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    // Mouse look variables
    private float rotationX = 0f;
    private bool cursorLocked = true;

    // Events
    public static event Action<GameObject> OnInteractableFound;
    public static event Action OnInteractableLost;
    public static event Action<GameObject> OnInteract;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (playerCamera == null)
        {
            Debug.LogError("Player camera not found!");
            enabled = false;
            return;
        }

        // Create ground check object
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.parent = transform;
        groundCheck.localPosition = new Vector3(0, -characterController.height/2, 0);

        SetupPlayer();
    }

    private void SetupPlayer()
    {
        currentSpeed = walkSpeed;
        LockCursor();
    }

    private void Update()
    {
        if (GameManager.Instance.isPaused) return;

        HandleMovement();
        HandleMouseLook();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, LayerMask.GetMask("Ground"));

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Apply movement
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Handle running
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (!cursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1 : -1);

        // Rotate camera up/down
        rotationX = Mathf.Clamp(rotationX + mouseY, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            OnInteractableFound?.Invoke(hitObject);

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteract?.Invoke(hitObject);
            }
        }
        else
        {
            OnInteractableLost?.Invoke();
        }
    }

    public void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ToggleCursor()
    {
        if (cursorLocked)
            UnlockCursor();
        else
            LockCursor();
    }

    private void OnEnable()
    {
        if (cursorLocked)
            LockCursor();
    }

    private void OnDisable()
    {
        UnlockCursor();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
