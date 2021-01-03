using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentCube_v0_4_2 : Agent
{
    public float agentRunSpeed = 2f;
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
        //Target.localPosition = GetRandomSpawnPos();
        MoveTargetPos();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
        // Penalty for wandering too much
        SetReward(-5f / 1000f);

        // Punish the agent for touching the walls
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(0.6f, 0.1f, 0.6f));
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].name == "Wall")
                {
                    SetReward(-0.1f);
                }

                if (hitColliders[i].name == "Target")
                {
                    SetReward(1.0f);
                    EndEpisode();
                }
            }
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown("space"))
        {

            MoveTargetPos();
            // print("Target:" + Target.position);
            // = new Vector3(areaBounds.extents.x, 0.5f, areaBounds.extents.z);
        }

        //
        // if (Physics.CheckBox(Target.position, new Vector3(0.5f, 0.1f, 0.5f)) == false)
        // {
        //     print("Not colliding");

        // }
        // else
        // {
        //     print("colliding");
        // }
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

    public void MoveTargetPos()
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
            Target.localPosition = randomSpawnPos;
            // First check
            if (Physics.CheckBox(Target.localPosition, new Vector3(0.6f, 0.3f, 0.6f)) == false)
            {

                foundNewSpawnLocation = true;
            }
        }
    }
}