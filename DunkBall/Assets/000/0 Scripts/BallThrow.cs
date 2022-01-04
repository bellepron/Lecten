using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrow : MonoBehaviour
{
    public void Throw(float power, Transform target, float arrivalTime)
    {
        Vector3 requiredVelocity = RequiredInitialVelocity(target.position, arrivalTime);
        GetComponent<Rigidbody>().velocity = requiredVelocity;
    }

    Vector3 RequiredInitialVelocity(Vector3 _target, float time)
    {
        Vector3 distance = _target - transform.position;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float distance_Y = distance.y;
        float distance_XZ = distanceXZ.magnitude;
        float velocity_XZ = (distance_XZ / time);
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity_Y = (distance_Y / time + .5f * gravity * time);
        Vector3 result = distanceXZ.normalized;
        result *= velocity_XZ;
        result.y = velocity_Y;

        return result;
    }
}