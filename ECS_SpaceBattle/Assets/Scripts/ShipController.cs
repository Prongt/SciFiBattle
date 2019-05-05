using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

public class ShipController : JobComponentSystem
{
    public static float3 shipPos;
    public static Translation targetPos;
    public static NativeArray<float3> posArray;

    protected override void OnCreateManager()
    {
        shipPos = new float3();
        posArray = new NativeArray<float3>(1, Allocator.Persistent);
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        posArray.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        //BoidECS.targetPos = shipPos;

        var ArriveJob = new ArriveJob
        {
            targetPos = targetPos,
            shipPos = shipPos,
            posArray = posArray,
            deltaTime = Time.deltaTime,
            moveSpeed = 10,
            slowingDist = 10,
            stopRange = 5,
            weight = 1
        }.Schedule(this, handle);

        ArriveJob.Complete();
        return ArriveJob;
    }

    private struct ArriveJob : IJobForEachWithEntity<TargetData, Translation>
    {
        public float deltaTime;
        public Translation targetPos;
        public float3 shipPos;
        public float weight;
        public float moveSpeed;
        public float slowingDist;
        public float stopRange;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> posArray;
        public void Execute(Entity entity, int index, ref TargetData data, ref Translation trans)
        {
            var dir = targetPos.Value - trans.Value;
            var distance = ((Vector3)dir).magnitude;

            if (distance <= stopRange)
            {
                //data.inRange = true;

            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);

                var clamped = math.min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);
                var force = desired * deltaTime;

                Vector3 outForce = force;
                

                trans.Value += (float3)outForce.normalized * weight;
                //shipPos = trans.Value;
                posArray[0] = trans.Value;
            }

            
        }
    }
}
