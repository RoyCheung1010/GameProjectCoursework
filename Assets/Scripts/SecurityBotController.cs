using UnityEngine;
using UnityEngine.AI;

public class SecurityBotController : MonoBehaviour
{
    public enum BotState { Patrol, Chase, Search, Confused }
    [Header("Current State")]
    public BotState currentState = BotState.Patrol;

    [Header("Patrol Setup")]
    public Transform[] patrolPoints;
    public float stoppingDistance = 0.5f;
    private int currentPointIndex = 0;
    public NavMeshAgent Agent { get; private set; }

    [Header("Detection (Vision)")]
    public float viewRange = 12f;
    [Range(0f, 360f)] public float viewAngle = 90f;
    public float viewRotationOffset = 90f;
    
    public LayerMask obstructionMask;
    public Transform eyeTransform;
    private Transform playerTransform;
    private Collider playerCollider;

    [Header("Chase Settings")]
    public float chaseSpeed = 4.5f;
    public float patrolSpeed = 2.5f;
    public float searchDuration = 4f;
    private float searchTimer = 0f;
    private Vector3 lastSeenPosition;

    [Header("Confusion Setup")]
    public float confusionDuration = 3f;
    private float confusionTimer = 0f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public float rotationOffset = -90f;

    [Header("Graphics")]
    public Animator animator;

    [Header("Coordination")]
    public AI_Orchestrator orchestrator;

    private float checkSightTimer = 0f;
    private float checkSightInterval = 0.5f;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        if (Agent == null)
        {
            enabled = false;
            return;
        }

        Agent.stoppingDistance = stoppingDistance;
        Agent.speed = patrolSpeed;
        Agent.angularSpeed = 120f;
        Agent.updateRotation = false;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            enabled = false;
            return;
        }

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            playerCollider = playerGO.GetComponent<Collider>();
        }

        if (eyeTransform == null) eyeTransform = this.transform;

        currentState = BotState.Patrol;
        GoToNextPoint();
    }

    void Update()
    {
        if (animator != null) animator.SetFloat("Speed", Agent.velocity.magnitude);

        switch (currentState)
        {
            case BotState.Patrol: HandlePatrolState(); break;
            case BotState.Chase: HandleChaseState(); break;
            case BotState.Search: HandleSearchState(); break;
            case BotState.Confused: HandleConfusedState(); break;
        }

        if (currentState == BotState.Patrol)
        {
            if (Agent.hasPath && Agent.pathStatus == NavMeshPathStatus.PathPartial)
                TriggerConfusion();
        }

        if (Agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Agent.velocity.normalized);
            Quaternion offsetRot = lookRotation * Quaternion.Euler(0, rotationOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, offsetRot, Time.deltaTime * rotationSpeed);
        }

        checkSightTimer += Time.deltaTime;
        if (checkSightTimer >= checkSightInterval)
        {
            checkSightTimer = 0f;
            if (playerTransform != null && currentState != BotState.Confused)
            {
                if (CanSeePlayer())
                {
                    lastSeenPosition = playerTransform.position;
                    StartChase();
                }
                else if (currentState == BotState.Chase)
                {
                    StartSearch();
                }
            }
        }
    }
    void HandlePatrolState()
    {
        Agent.isStopped = false;
        Agent.speed = patrolSpeed;
        if (!Agent.pathPending && Agent.remainingDistance < Agent.stoppingDistance)
            GoToNextPoint();
    }

    void HandleChaseState()
    {
        if (playerTransform == null) { StartSearch(); return; }
        
        Agent.isStopped = false;
        Agent.speed = chaseSpeed;
        Agent.SetDestination(playerTransform.position);
        lastSeenPosition = playerTransform.position;
    }

    void HandleSearchState()
    {
        Agent.isStopped = false;
        Agent.speed = patrolSpeed;
 
        if (!Agent.pathPending && Agent.remainingDistance < Agent.stoppingDistance)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                currentState = BotState.Patrol;
                GoToNextPoint();
            }
        }
        else
        {
            Agent.SetDestination(lastSeenPosition);
        }
    }

    void HandleConfusedState()
    {
        Agent.isStopped = true;
        confusionTimer -= Time.deltaTime;
        if (confusionTimer <= 0f)
        {
            currentState = BotState.Patrol;
            Agent.isStopped = false;
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (currentState != BotState.Patrol) return;
        Agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 dirToPlayer = playerTransform.position - eyeTransform.position;
        float dist = dirToPlayer.magnitude;
        if (dist > viewRange) return false;

        Vector3 correctedForward = Quaternion.Euler(0, viewRotationOffset, 0) * eyeTransform.forward;

        float angleToPlayer = Vector3.Angle(correctedForward, dirToPlayer.normalized);
        if (angleToPlayer > viewAngle * 0.5f) return false;

        Ray ray = new Ray(eyeTransform.position + Vector3.up * 0.2f, dirToPlayer.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, viewRange, ~0)) 
        {
            if (hit.collider != null && (hit.collider == playerCollider || hit.collider.CompareTag("Player")))
            {
                return true;
            }
        }
        return false;
    }

    void StartChase()
    {
        if (currentState == BotState.Chase) return;
        currentState = BotState.Chase;
    }

    void StartSearch()
    {
        if (currentState == BotState.Search) return;
        currentState = BotState.Search;
        searchTimer = searchDuration;
        Agent.SetDestination(lastSeenPosition);
    }

    public void TriggerConfusion()
    {
        if (currentState == BotState.Confused) return;
        currentState = BotState.Confused;
        confusionTimer = confusionDuration;
        if (orchestrator != null) orchestrator.AlertAllBots(this);
    }

    public void ForceConfuse()
    {
        if (currentState == BotState.Confused) return;
        currentState = BotState.Confused;
        confusionTimer = confusionDuration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();

            if (gm != null)
            {
                gm.DeductLife();

                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 awayDirection = (playerRb.transform.position - transform.position).normalized;
                    playerRb.AddForce(awayDirection * 200f, ForceMode.Impulse); 
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null) return;

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.12f);
        Gizmos.DrawSphere(eyeTransform.position, viewRange);

        Vector3 correctedForward = Quaternion.Euler(0, viewRotationOffset, 0) * eyeTransform.forward;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle * 0.5f, Vector3.up);

        Vector3 leftDir = leftRayRotation * correctedForward;
        Vector3 rightDir = rightRayRotation * correctedForward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyeTransform.position + Vector3.up * 0.2f, leftDir * viewRange);
        Gizmos.DrawRay(eyeTransform.position + Vector3.up * 0.2f, rightDir * viewRange);
    }
}