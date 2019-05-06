using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    public Vector3 targetPos;
    public Vector3 offset;
    public float followDistance;
    public float maxSpeed;
    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 force;
    [SerializeField] private float mass;
    [SerializeField] private float damping;
    [SerializeField] private float banking;
    [SerializeField] private float slowingDistance;
    private void Update()
    {
        targetPos = ValueAdjuster.cameraTarget;
        if (targetPos == null)
        {
            return;
        }
        //var pos = (transform.position - targetPos) * followDistance;
        //transform.position = pos * Time.deltaTime;

        //var startPos = transform.position;


        //transform.LookAt(targetPos);
        //targetPos += offset;
        force = ArriveForce(targetPos, slowingDistance);

        Vector3 newAcceleration = force / mass;
        acceleration = Vector3.Lerp(acceleration, newAcceleration, Time.deltaTime);
        velocity += acceleration * Time.deltaTime;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if (velocity.magnitude > float.Epsilon)
        {
            Vector3 tempUp = Vector3.Lerp(transform.up, Vector3.up + (acceleration * banking), Time.deltaTime * 3.0f);
            //transform.LookAt(transform.position + velocity, tempUp);
            transform.LookAt(targetPos, tempUp);

            transform.position += velocity * Time.deltaTime;
            velocity *= (1.0f - (damping * Time.deltaTime));
        }
    }

    public Vector3 ArriveForce(Vector3 target, float slowingDistance = 15.0f)
    {
        Vector3 toTarget = (target + offset) - transform.position;

        float distance = toTarget.magnitude;
        if (distance < followDistance)
        {
            return velocity * (1.0f - (damping * Time.deltaTime));
        }
        float ramped = maxSpeed * (distance / slowingDistance);

        float clamped = Mathf.Min(ramped, maxSpeed);
        Vector3 desired = clamped * (toTarget / distance);

        return desired - velocity;
    }
}
