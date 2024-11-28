using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateTest : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public float idleDistance = 10f; // Distance beyond which the enemy enters IDLE state
    public float checkInterval = 1f; // How often to check the state (in seconds)

    private float distanceToPlayer;
    private State currentState;

    [SetUp]
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("CheckState", 0f, checkInterval);
    }

    void CheckState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > idleDistance)
        {
            Debug.Log($"Enemy at {name} is in IDLE state because it's far from the player.");
            currentState = new Idle(gameObject, agent, GetComponent<Animator>(), player, null);
        }
        else
        {
            Debug.Log($"Enemy at {name} is not in IDLE state. Distance to player: {distanceToPlayer}");

            // You can add logic here to determine which state the enemy should be in based on its current state
            // For example:
            if (currentState.name == State.STATE.IDLE)
            {
                currentState = new Patrol(gameObject, agent, GetComponent<Animator>(), player, null);
            }
            else if (currentState.name == State.STATE.PATROL)
            {
                currentState = new Attack(gameObject, agent, GetComponent<Animator>(), player, null);
            }
        }
    }

    void Update()
    {
        // Ensure the NavMeshAgent is active and moving
        if (agent.enabled && !agent.isStopped)
        {
            // If the agent has reached its destination, stop checking for now
            if (agent.remainingDistance < 0.1f && !agent.hasPath)
            {
                CancelInvoke("CheckState");
            }
        }
        else
        {
            // If the agent is stopped or disabled, start checking again
            InvokeRepeating("CheckState", 0f, checkInterval);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, player.position);
        Gizmos.DrawWireSphere(player.position, idleDistance);
    }
}
