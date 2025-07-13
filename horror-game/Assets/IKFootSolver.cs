using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public Transform body;
    public float footOffset;
    public LayerMask ground;
    public float stepDistance;
    public float stepHeight;
    public float speed;

    private Vector3 currentPosition;
    private Vector3 newPosition;
    private Vector3 oldPosition;
    private float lerp = 0f;
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
        transform.position = currentPosition;

        if (IsStepping)
        {
            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPosition;
            lerp += Time.deltaTime * speed;
        }
    }

    public void TryStep()
    {
        if (!IsStepping)
        {
            Ray ray = new Ray(body.position + transform.forward * footOffset, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit info, 10f, ground))
            {
                if (Vector3.Distance(newPosition, info.point) > stepDistance)
                {
                    lerp = 0f;
                    oldPosition = currentPosition;
                    newPosition = info.point;
                }
            }
        }
    }

    private float DistanceToGround()
    {
        Ray ray = new Ray(body.position + transform.forward * footOffset, Vector3.down);
        return Physics.Raycast(ray, out RaycastHit hit, 10f, ground)
            ? Vector3.Distance(newPosition, hit.point)
            : 0f;
    }
}
