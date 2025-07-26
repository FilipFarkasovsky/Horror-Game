using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public float sprintSpeed;
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

    [Header("Headbob")]
    [SerializeField] private bool headbobEnabled = true;
    [SerializeField, Range(0, 0.05f)] private float _Amplitude = 0.015f;
    [SerializeField, Range(0, 30)] private float _frequency = 10.0f;
    [SerializeField] private Transform cameraHolder = null;
    private float toggleSpeed = 3.0f;
    private Vector3 startPos;

    [Header("Field of View")]
    public Camera playerCamera;
    public float baseFOV = 60f;
    public float sprintFOV = 70f;
    public float fovChangeSpeed = 5f;

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
    public bool isSprinting;
    private bool isCrouching;
    private float noiseTimer;

    private InputAction sprintAction;
    public InputActionAsset InputActions;

    [Header("Stamina Main Parameters")]
    public float playerStamina = 100.0f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float jumpCost = 20;

    [Header("Stamina Regen Parameters")]
    [Range(0, 50)][SerializeField] private float staminaDrain = 0.5f;
    [Range(0, 50)][SerializeField] private float StaminaRegen = 0.5f;

    [Header("Stamina UI Elements")]
    [SerializeField] private Image staminaProgressUI = null;
    [SerializeField] private CanvasGroup sliderCanvasGroup = null;
    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPos = cameraTransform.localPosition;
        sprintAction = InputSystem.actions.FindAction("Sprint");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isSprinting = sprintAction.IsPressed();
        HandleMovement();
        HandleHeadBob();
        HandleLook();
        HandleFieldOfView();
    }
    void HandleFieldOfView()
    {
        if (isSprinting && (inputVector.x + inputVector.y) > 0.3)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, fovChangeSpeed * Time.deltaTime);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, baseFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    private void HandleHeadBob()
    {
        if (!headbobEnabled) return;

        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        if (speed < toggleSpeed || !controller.isGrounded) return;

        Vector3 move = Vector3.zero;
        move.y += Mathf.Sin(Time.time * _frequency) * _Amplitude;
        move.x += Mathf.Cos(Time.time * _frequency / 2) * _Amplitude;
        cameraTransform.localPosition += move;

        if (cameraTransform.localPosition == startPos) return;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, startPos, 1 * Time.deltaTime);

        Vector3 pos = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
        pos += cameraHolder.forward * 15.0f;
        cameraTransform.LookAt(pos);
    }

    private void HandleMovement()
    {
        // Ground check using sphere
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isSprinting && isGrounded && inputVector.magnitude > 0.3)
        {
            playerStamina -= staminaDrain * Time.deltaTime;

            if (playerStamina <= 0)
                playerStamina = 0;
        }
        else{
            playerStamina += StaminaRegen * Time.deltaTime;
           
            if (playerStamina > maxStamina)
                playerStamina = maxStamina;
        }


        if (playerStamina >= maxStamina || playerStamina <= 0) 
            sliderCanvasGroup.alpha = 0;
        else sliderCanvasGroup.alpha = 1;
        staminaProgressUI.fillAmount = playerStamina / maxStamina;


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

        if (playerStamina > 0 && isSprinting)
            controller.Move((move * sprintSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        else
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
