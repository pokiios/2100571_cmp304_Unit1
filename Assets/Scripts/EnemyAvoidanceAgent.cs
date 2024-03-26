using System.Diagnostics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class EnemyAvoidanceAgent : Agent
{
    // Public/Exposed Variables
    [SerializeField] float forceMultiplier = 0;
    [SerializeField] float positiveReward;
    [SerializeField] float negativeReward;
    [SerializeField] float timeReward;

    // Private Variables

    GameObject[] children;
    public GameObject enemyParent;
    Stopwatch timer;
    float timeElapsed = 0;
    float currentLongestTime = 0; 
    
    // Get the Rigidbody component for the agent
    void Start() {
        children = new GameObject[enemyParent.transform.childCount];

        for (int i = 0; i < children.Length; i++)
        {
            children[i] = enemyParent.transform.GetChild(i).gameObject;
        }
    }

    // Initialises the agent and enemies when the episode begins
    public override void OnEpisodeBegin()
    {
        // Start timer
        timer = new Stopwatch();
        timer.Start();

        //Move Player to a random spot on the floor at the start of each episode
        this.transform.localPosition = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));

        for (int i = 0; i < children.Length; i++)
        {
            children[i].transform.localPosition = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }
    }
    
    // Collects observations from the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent Position
        sensor.AddObservation(this.transform.localPosition);
        for (int i = 0; i < children.Length; i++)
        {
            // Find nearest enemy to the agent
            sensor.AddObservation(children[i].transform.localPosition);
        }
        
        // Agent Velocity
        sensor.AddObservation(this.transform.localPosition.x);
        sensor.AddObservation(this.transform.localPosition.z);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        float nearestEnemyDistance = 9999f;
        GameObject nearestEnemy = null;

        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];

        this.transform.localPosition += new Vector3(controlSignal.x, 0, controlSignal.z) * forceMultiplier * Time.deltaTime;
        this.transform.rotation = Quaternion.LookRotation(controlSignal);

        // Staying away from the enemy
        for (int i = 0; i < children.Length; i++)
        { 
            if ((children[i].transform.localPosition - this.transform.localPosition).magnitude > nearestEnemyDistance)
            {
                nearestEnemyDistance = (children[i].transform.localPosition - this.transform.localPosition).magnitude;
                nearestEnemy = children[i];
            }
        }

        if (nearestEnemyDistance > 10f)
        {
            SetReward(positiveReward);
        }

        // Give reward if they last longer than the last longest living AI

        if (timer.ElapsedMilliseconds/1000 % 10 == 0)
        {
            SetReward(1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the agent collides with the enemy, end the episode
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            SetReward(negativeReward);
            timer.Stop();
            timeElapsed = timer.ElapsedMilliseconds / 1000;
            if (timeElapsed > currentLongestTime)
            {
                currentLongestTime = timeElapsed;
                SetReward(timeReward);
            }
            EndEpisode();
        }

        // If the agent collides with the wall, end the episode
        else if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(negativeReward);
            timer.Stop();
            timeElapsed = timer.ElapsedMilliseconds / 1000;
            if (timeElapsed > currentLongestTime)
            {
                currentLongestTime = timeElapsed;
                SetReward(timeReward);
            }
            EndEpisode();
        }
    }
}


