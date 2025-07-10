using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyType { Aggresive, Stealth }

    [Header("General")]
    public EnemyType enemyType;

    [Header("Vision")]
    public float viewDistance;
    public float viewAngle;
    public LayerMask obstructionMask;

    [Header("Movement Speeds")]
    public float defaultSpeed;
    public float chaseSpeed;

    [Header("Behavior Settings")]
    public float aggression;
    public float wanderInterval;

    private Transform target;
    private NavMeshAgent agent;
    private float aggressionTimer;
    private float wanderTimer;
    private float investigationTimer;

    private GameObject[] potentialTargets;
    private bool canSeePlayer;
    private bool isChasing;
    private bool wandering;
    private bool investigating;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        aggressionTimer = 0;
        wanderTimer = wanderInterval;

        ChooseRandomTarget();

        if (enemyType == EnemyType.Aggresive)
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
        else if (enemyType == EnemyType.Stealth)
        {
            StartWandering();
        }
    }

    void Update()
    {
        CheckLineOfSight();
        Debug.Log(potentialTargets.Length);

        if (canSeePlayer)
        {
            HandleChase();
        }
        else
        {
            HandleLossOfSight();
        }

        if (enemyType == EnemyType.Stealth && !isChasing)
        {
            agent.speed = defaultSpeed;

            if (investigating)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartWandering();
                }
            }
            else if (wandering)
            {
                HandleWandering();
            }
        }
    }

    void HandleChase()
    {
        isChasing = true;
        aggressionTimer = aggression;
        agent.speed = chaseSpeed;
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
        wandering = false;
        investigating = false;
    }

    void HandleLossOfSight()
    {
        aggressionTimer -= Time.deltaTime;

        if (aggressionTimer <= 0f)
        {
            aggressionTimer = 0f;

            if (isChasing)
            {
                isChasing = false;

                if (!wandering && !investigating)
                {
                    StartWandering();
                }
            }
        }
        else if (isChasing)
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }

    void HandleWandering()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || agent.remainingDistance < 1f)
        {
            Vector3 location = ChooseRandomLocation();
            agent.SetDestination(location);
            wanderTimer = wanderInterval;
        }
    }

    void StartWandering()
    {
        investigating = false;
        wandering = true;

        Vector3 location = ChooseRandomLocation();
        agent.SetDestination(location);
        wanderTimer = wanderInterval;
    }
    void CheckLineOfSight()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        canSeePlayer = false;

        foreach (GameObject o in potentialTargets)
        {
            Vector3 directionToPlayer = (o.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, o.transform.position);

            if (distanceToPlayer < viewDistance)
            {
                float angle = Vector3.Angle(transform.forward, directionToPlayer);
                if (angle < viewAngle / 2f)
                {
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, distanceToPlayer, obstructionMask))
                    {
                        if (distanceToPlayer < closestDistance)
                        {
                            closestDistance = distanceToPlayer;
                            closestTarget = o.transform;
                            Debug.Log(o.gameObject.name + "is my target!");
                            canSeePlayer = true;
                        }
                    }
                }
            }
        }

        if (canSeePlayer)
        {
            target = closestTarget;
        }
    }


    void ChooseRandomTarget()
    {
        potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        if (potentialTargets.Length > 0)
        {
            int randomIndex = Random.Range(0, potentialTargets.Length);
            target = potentialTargets[randomIndex].transform;
        }
        else
        {
            target = null;
        }
    }

    public void OnHeardSound(Vector3 soundPosition)
    {
        if (enemyType == EnemyType.Stealth && !canSeePlayer)
        {
            wandering = false;
            investigating = true;
            agent.SetDestination(soundPosition);
        }
    }
    Vector3 ChooseRandomLocation()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 20f;
            randomDirection.y = 0f;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return transform.position;
    }
}
