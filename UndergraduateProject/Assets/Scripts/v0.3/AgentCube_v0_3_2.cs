using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentCube_v0_3_2 : Agent
{
    public float agentRunSpeed = 3f;
    public float spawnAreaMarginMultiplier = 0.9f;
    public Transform Target;
    public GameObject ground;
    Bounds areaBounds;
    Rigidbody m_AgentRb;

    void Start()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        areaBounds = ground.GetComponent<Collider>().bounds;
    }

    public override void OnEpisodeBegin()
    {
        // Move the target to a new spot
        Target.localPosition = GetRandomSpawnPos();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);

        // Give reward
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        if (distanceToTarget < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        // Punish the agent if it is taking too long
        SetReward(-5f / 1000f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        var dirToGo = Vector3.zero;
        var movementAction = actionBuffers.DiscreteActions[0];

        switch (movementAction)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                dirToGo = transform.right * -0.75f;
                break;
            case 4:
                dirToGo = transform.right * 0.75f;
                break;
        }
        m_AgentRb.AddForce(dirToGo * agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * spawnAreaMarginMultiplier,
                areaBounds.extents.x * spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * spawnAreaMarginMultiplier,
                areaBounds.extents.z * spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.localPosition + new Vector3(randomPosX, 0.5f, randomPosZ);
            var worldspacepos = transform.TransformPoint(randomSpawnPos);

            if (Physics.CheckBox(worldspacepos, new Vector3(0.6f, 0.1f, 0.6f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }
}
