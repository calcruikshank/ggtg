using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform targetToFollow;
    Transform targetToRotateTowards;
    public void SetTarget(Transform target)
    {
        targetToFollow = target;
        targetToRotateTowards = target;
    }
    private void Update()
    {
        if (targetToFollow != null)
        {
            this.transform.position = targetToFollow.position;
        }
        if (targetToRotateTowards != null)
        {
            this.transform.rotation = targetToRotateTowards.rotation;
        }
    }

    internal void SetRot(Transform pelvis)
    {
        targetToRotateTowards = pelvis;
    }
}
