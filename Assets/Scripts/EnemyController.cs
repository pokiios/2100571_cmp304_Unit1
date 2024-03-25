using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float forceMultiplier = 0;
    [SerializeField]GameObject agent;

    void Update()
    {
        // Move towards local agent
        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, agent.transform.localPosition, forceMultiplier * Time.deltaTime);
        this.transform.LookAt(agent.transform.localPosition);
    }
}
