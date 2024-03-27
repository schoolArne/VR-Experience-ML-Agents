using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using System.Diagnostics;

public class CubeAgent : Agent
{
    public Transform Target;
    public Transform ZoneTarget;
    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5;
    private bool targetReached = false;
    private float zoneHeight;

    public override void OnEpisodeBegin()
    {
        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        // reset target reached flag
        this.targetReached = false;
        // set zone height
        this.zoneHeight = ZoneTarget.localPosition.y;
        this.transform.localRotation = Quaternion.identity;
        // verplaats de target naar een nieuwe willekeurige locatie
        Target.localPosition = new Vector3(UnityEngine.Random.value * 8 - 4, 0.5f, UnityEngine.Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target en Agent posities
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.ZoneTarget.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Target reached
        if (distanceToTarget < 1.42f)
        {
            if(!this.targetReached)
            {
                SetReward(0.2f);
            }            
            this.targetReached = true;
        }
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ZoneTarget.gameObject)
        {
            if (targetReached)
            {
                SetReward(0.8f);
                EndEpisode();
            }
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}