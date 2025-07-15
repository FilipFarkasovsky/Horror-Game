using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement_demo : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public float gravity;
    public float jumpVelocity;
    public float jumpNoiseRadius;
    public float walkingNoiseRadius;
    public float noiseInterval = 1f;

    [Header("Sound barriers")]
    public LayerMask soundBarriers;

    [Header("Look Settings")]
    public float lookSensitivity;
    public Transform cameraTransform;
    public float interactDistance;
    public LayerMask interactableLayer;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector2 inputVector;
    private Vector2 lookVector;
    private float verticalVelocity;
    private float cameraPitch;
    private bool isGrounded;
    private float currentSpeed;
    private bool isSprinting;
    private float noiseTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        // Ground check using sphere
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Keeps grounded
        }

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;

        // Move the character
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;

        if (move.magnitude > 0.1f && isGrounded)
        {
            noiseTimer -= Time.deltaTime;

            if (noiseTimer <= 0f)
            {
                MakeNoise(walkingNoiseRadius);
                noiseTimer = noiseInterval;
            }
        }
        else
        {
            noiseTimer = noiseInterval;
        }

        controller.Move((move * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Rotate player left/right
        transform.Rotate(Vector3.up * lookVector.x * lookSensitivity);

        // Rotate camera up/down
        cameraPitch -= lookVector.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    // Input System action handlers
    private void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        lookVector = value.Get<Vector2>();
    }

    private void OnJump()
    {
        if (isGrounded)
        {
            verticalVelocity = jumpVelocity;
            MakeNoise(jumpNoiseRadius);
        }
    }

    private void OnInteract()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Door door = hit.collider.GetComponent<Door>();
            if (door != null)
            {
                door.ToggleDoor(0f);
            }
        }
    }
    public void MakeNoise(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Vector3 directionToTarget = (collider.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                bool wallBetween = Physics.Raycast(transform.position, directionToTarget, distance, soundBarriers);

                float effectiveRadius = wallBetween ? radius * 0.5f : radius;

                if (distance <= effectiveRadius)
                {
                    EnemyAI enemy = collider.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.OnHeardSound(transform.position);
                    }
                }
            }
        }
    }
}
