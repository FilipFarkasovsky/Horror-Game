using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement_demo : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpVelocity = 5f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector2 inputVector;
    private Vector2 lookVector;
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private bool isGrounded;
    private float currentSpeed;
    private bool isSprinting;

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
            MakeNoise(50f);
        }
    }

    public void MakeNoise(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject.CompareTag("Enemy"))
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
