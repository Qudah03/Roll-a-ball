using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent navMeshAgent;

    [Header("Level 2 Stealth Settings")]
    public bool isStealthLevel = false; // Toggle this in the Inspector
    public float detectionRange = 5.0f;
    public float startDelay = 5.0f;

    private bool isActivated = true;
    private bool isPatrolling = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (isStealthLevel)
        {
            isActivated = false;
            StartCoroutine(ActivationDelay());
        }
    }

    IEnumerator ActivationDelay()
    {
        yield return new WaitForSeconds(startDelay);
        isActivated = true;
    }

    void Update()
    {
        if (player == null || !isActivated || !navMeshAgent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (isStealthLevel)
        {
            if (distance <= detectionRange)
            {
                // Player is close: Chase them aggressively
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(player.position);
                isPatrolling = false; // Reset patrol state
            }
            else
            {
                // Player is far: Go into Patrol mode
                navMeshAgent.isStopped = false; 
            
                // If we aren't patrolling yet, or we reached our random destination
                if (!isPatrolling || navMeshAgent.remainingDistance < 0.5f)
                {
                    PickRandomPatrolPoint();
                }
            }
        }
        else
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
        }
    }

    void PickRandomPatrolPoint()
    {
        // Pick a random direction within 15 units of the enemy
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 15f;
        randomDirection += transform.position;
    
        NavMeshHit hit;
        // Find the closest valid NavMesh point to that random direction
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, UnityEngine.AI.NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
            isPatrolling = true;
        }
    }
}