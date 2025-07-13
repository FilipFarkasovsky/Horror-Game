using UnityEngine;
using UnityEngine.AI;

public class IKFootSolver : MonoBehaviour
{
    public Transform body;
    public LayerMask ground;

    [Header("Base Step Settings")]
    public float baseStepDistance;
    public float baseSpeed;
    public float stepHeight;
    public float velocityScaler;
    public Vector3 footOffset;

    [Header("Dynamic Scaling")]
    public NavMeshAgent agent;

    private float stepDistance;
    private float speed;

    private Vector3 currentPosition;
    private Vector3 newPosition;
    private Vector3 oldPosition;
    private float lerp = 1f;

    public bool IsStepping => lerp < 1f;
    public bool IsReadyToStep => !IsStepping && DistanceToGround() > stepDistance;

    void Start()
    {
        currentPosition = transform.position;
        oldPosition = currentPosition;
        newPosition = currentPosition;
    }

    void Update()
    {
        float velocity = agent != null ? agent.velocity.magnitude : 0f;
        //baseSpeed = agent.speed;

        // Avoid instability at low speeds
        if (velocity < 0.05f) velocity = 0f;

        stepDistance = baseStepDistance + velocity * 0.15f;
        speed = baseSpeed + velocity * 0.5f;

        transform.position = currentPosition;

        if (IsStepping)
        {
            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPosition;
            lerp += Time.deltaTime * speed;

            if (lerp >= 1f)
            {
                currentPosition = newPosition;
                lerp = 1f;
            }
        }
    }

    public void TryStep()
    {
        if (!IsStepping && agent != null)
        {
            Vector3 forward = agent.velocity.sqrMagnitude > 0.01f
                ? agent.velocity.normalized
                : body.forward;

            // Apply foot offset in local space
            Vector3 localOffset = new Vector3(footOffset.x, 0, footOffset.z);
            Vector3 worldOffset = body.TransformDirection(localOffset);

            Vector3 rayOrigin = body.position + forward * velocityScaler + worldOffset;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, ground))
            {
                Vector3 localHitPoint = body.InverseTransformPoint(hit.point);

                // Lock X (side) movement to foot spacing
                localHitPoint.x = footOffset.x;

                Vector3 constrainedWorldPoint = body.TransformPoint(localHitPoint);

                if (Vector3.Distance(newPosition, constrainedWorldPoint) > stepDistance)
                {
                    lerp = 0f;
                    oldPosition = currentPosition;
                    newPosition = constrainedWorldPoint;
                }
            }
        }
    }

    private float DistanceToGround()
    {
        if (agent == null) return 0f;

        Vector3 forward = agent.velocity.sqrMagnitude > 0.01f
            ? agent.velocity.normalized
            : body.forward;

        Vector3 localOffset = new Vector3(footOffset.x, 0, footOffset.z);
        Vector3 worldOffset = body.TransformDirection(localOffset);
        Vector3 rayOrigin = body.position + forward * velocityScaler + worldOffset;

        return Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f, ground)
            ? Vector3.Distance(newPosition, hit.point)
            : 0f;
    }
}
