using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public float noiseRadius = 5f;
    public LayerMask soundBarriers;
    public bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private float actualSpeed;

    void Start()
    {
        actualSpeed = openSpeed;
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        if (isOpen)
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime * actualSpeed);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, Time.deltaTime * actualSpeed);
    }

    public void ToggleDoor(float openSpeed_)
    {
        if (openSpeed_ != 0)
        {
            actualSpeed = openSpeed_;
        } else
        {
            actualSpeed = openSpeed;
        }
        isOpen = !isOpen;
        MakeNoise(noiseRadius);
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
