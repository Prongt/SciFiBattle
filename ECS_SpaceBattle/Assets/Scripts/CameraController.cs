using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    public static Vector3 targetPos;
    public Vector3 offset;
    public float followDistance;
    //[HideInInspector] public float tempFollowDistance;
    public float maxSpeed;
    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 force;
    [SerializeField] private float mass;
    [SerializeField] private float damping;
    [SerializeField] private float banking;
    //[SerializeField] private float slowingDistance;

    public static bool IsAtTarget = false;

    private void Awake()
    {
        //tempFollowDistance = followDistance;
        IsAtTarget = false;
    }
    private void Update()
    {
        //targetPos = ValueAdjuster.cameraTarget;
        if (targetPos == new Vector3())
        {
            IsAtTarget = false;
            Debug.Log("Null Target");
            return;
        }
        //if (math.distance(transform.position, targetPos) < 30)
        //{
        //    IsAtTarget = true;
        //}
        //else
        //{
        //    IsAtTarget = false;
        //}

        force = ArriveForce(targetPos, followDistance);

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

    public bool CheckAtTarget(Vector3 t)
    {
        if (math.distance(this.transform.position, t) < followDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 ArriveForce(Vector3 target, float slowingDistance = 15.0f)
    {
        Vector3 toTarget = (target + offset) - transform.position;

        float distance = toTarget.magnitude;
        if (distance < slowingDistance)
        {
            //IsAtTarget = true;
            return velocity * (1.0f - (damping * Time.deltaTime));
        }
        
            //IsAtTarget = false;
        
        float ramped = maxSpeed * (distance / slowingDistance * 2);

        float clamped = Mathf.Min(ramped, maxSpeed);
        Vector3 desired = clamped * (toTarget / distance);

        return desired - velocity;
    }
}
