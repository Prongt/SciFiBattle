using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    public static Translation targetPos;



    public int maxNeighbours = 20;


    public NativeMultiHashMap<int, NeighbourData> cellMap;

    public static float maxNeighbourDist = 10;
    public static float ArriveWeight = 1;
    public static float AllignmentWeight = 1;
    public static float SeperationWeight = 1;
    public static float CohesionWeight = 1;
    public static float fleeWeight = 1;
    public static float fleeDistance = 1;
    public static float cellSize = 10;
    public static float gridSize = 2000;
    public static float boidMass = 1;
    public static float boidDamping = 1;
    public static float boidMaxSpeed = 10;
    public static float boidBanking = 1;
    public static float boidWeight = 1;
    public static float moveSpeed = 10;
    public static float slowingDistance = 1;
    public static float stopRange = 1;
    public static float maxForce = 10;
    public static float constrainWeight = 1;
    public static bool BlackHole = false;

    public BufferFromEntity<PosRot> posRotBuffer;
    public BufferFromEntity<Force> forceBuffer;
    protected override void OnCreateManager()
    {




        cellMap = new NativeMultiHashMap<int, NeighbourData>(2500 * maxNeighbours, Allocator.Persistent);

        posRotBuffer = GetBufferFromEntity<PosRot>(false);



        Debug.Log("On Create!");
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        cellMap.Dispose();
        Debug.Log("on destroy");
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        float allign = AllignmentWeight;
        float cohesion = CohesionWeight;
        float arrive = ArriveWeight;
        float seperation = SeperationWeight;
        float weight = boidWeight;

        JobHandle cellJob = new CellSpacePartitionJob
        {
            cellSize = cellSize,
            gridSize = gridSize,
            cellMap = cellMap.ToConcurrent()
        }.Schedule(this, handle);


        JobHandle neighbourJob = new NeighbourJob
        {
            maxNeighbours = maxNeighbours,
            cellMap = cellMap,
            maxNeighbourDist = maxNeighbourDist,
            neighbourBuffer = GetBufferFromEntity<PosRot>(false)
        }.Schedule(this, cellJob);


        JobHandle arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = targetPos,
            weight = arrive,
            stopRange = stopRange,
            slowingDist = slowingDistance,
            moveSpeed = boidMaxSpeed

        }.Schedule(this, neighbourJob);

        JobHandle seperationJob = new SeperationJob
        {
            maxNeighbours = maxNeighbours,
            weight = seperation,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, arriveJob);

        JobHandle cohesionJob = new CohesionJob
        {
            maxNeighbours = maxNeighbours,
            weight = cohesion,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true),
            maxSpeed = boidMaxSpeed
        }.Schedule(this, seperationJob);

        JobHandle allignJob = new AllignmentJob
        {
            weight = allign,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, cohesionJob);

        JobHandle fleeJob = new FleeJob
        {
            targetPos = targetPos,
            weight = fleeWeight,
            fleeDist = fleeDistance,
            maxSpeed = boidMaxSpeed
        }.Schedule(this, allignJob);

        JobHandle constrainJob = new ConstrainJob
        {
            weight = constrainWeight,
            targetPos = targetPos,
            constrainDistance = 300
        }.Schedule(this, fleeJob);


        JobHandle boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            boidDamping = boidDamping,
            boidMass = boidMass,
            boidMaxSpeed = boidMaxSpeed,
            banking = boidBanking,
            weight = weight,
            maxForce = maxForce,
            IsBlackHole = BlackHole
        }.Schedule(this, constrainJob);

        JobHandle miscJob = new MiscJob()
        {
            cellMap = cellMap
        }.Schedule(boidJob);


        JobHandle destroyJob = new DestroyJob()
        {
        }.Schedule(miscJob);


        cellJob.Complete();
        neighbourJob.Complete();
        arriveJob.Complete();
        seperationJob.Complete();
        cohesionJob.Complete();
        allignJob.Complete();
        fleeJob.Complete();
        constrainJob.Complete();
        boidJob.Complete();
        miscJob.Complete();
        destroyJob.Complete();


        return destroyJob;
    }

    [BurstCompile]
    private struct BoidJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [ReadOnly] public EntityCommandBuffer cmdBuffer;
        public float deltaTime;
        public float boidMass;
        public float boidDamping;
        public float boidMaxSpeed;
        public float weight;
        public float banking;
        public float maxForce;

        public bool IsBlackHole;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {

            float3 newAcceleration = (data.force * weight) / boidMass;
            data.acceleration = math.lerp(data.acceleration, newAcceleration, deltaTime);
            data.velocity += data.acceleration * deltaTime;



            data.velocity = Vector3.ClampMagnitude(data.velocity * weight, boidMaxSpeed);


            if (((Vector3)data.velocity).magnitude > float.Epsilon)
            {
                trans.Value += data.velocity * deltaTime;
                data.velocity *= (1.0f - (boidDamping * deltaTime));
            }

            quaternion r = math.slerp(rot.Value, Quaternion.LookRotation(data.velocity), deltaTime);

            rot.Value = r;

            data.force = Vector3.zero;



            if (data.inRange && IsBlackHole && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
        }


    }

    [BurstCompile]
    private struct SeperationJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {


        public int maxNeighbours;
        public float weight;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            //return;
            float3 force = Vector3.zero;

            DynamicBuffer<PosRot> neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector3 desired = (Vector3)trans.Value - neighbours[i].pos;
                force += (float3)(Vector3.Normalize(desired) / desired.magnitude);
            }

            Vector3 outForce = force;
            data.force += (float3)outForce.normalized * weight;
        }
    }

    [BurstCompile]
    public struct CohesionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {

        public int maxNeighbours;
        public float weight;
        public float maxSpeed;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            //return;
            float3 centerOfMass = Vector3.zero;

            float3 force = Vector3.zero;

            DynamicBuffer<PosRot> neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                centerOfMass += (float3)neighbours[i].pos;
            }

            if (neighbours.Length > 0)
            {
                centerOfMass /= neighbours.Length;

                Vector3 toTarget = centerOfMass - trans.Value;
                Vector3 desired = toTarget.normalized * maxSpeed;
                force = (desired - (Vector3)data.velocity).normalized;
            }

            Vector3 outForce = force;
            data.force += (float3)outForce.normalized * weight;
        }
    }

    [BurstCompile]
    public struct AllignmentJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {

        public float weight;
        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            Vector3 desired = Vector3.zero;
            Vector3 force = Vector3.zero;


            DynamicBuffer<PosRot> neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                desired += neighbours[i].rot * Vector3.forward;

            }

            if (neighbours.Length > 0)
            {
                Quaternion r = rot.Value;
                Vector3 vec = r.eulerAngles;
                desired /= neighbours.Length;
                force = desired - ((Vector3)(vec * (float3)Vector3.forward));
            }


            Vector3 outForce = force;
            data.force += (float3)outForce.normalized * weight;
        }
    }

    [BurstCompile]
    private struct ArriveJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        public Translation targetPos;

        public float weight;
        public float moveSpeed;
        public float slowingDist;
        public float stopRange;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            float3 dir = targetPos.Value - trans.Value;
            float distance = ((Vector3)dir).magnitude;

            if (distance <= slowingDist)
            {
                data.inRange = true;

            }
            else
            {
                data.inRange = false;
            }
            float ramped = moveSpeed * (distance / slowingDist);

            float clamped = math.min(ramped, moveSpeed);
            float3 desired = clamped * (dir / distance);
            float3 force = desired * deltaTime;

            Vector3 outForce = force;
            data.force += (float3)outForce.normalized * weight;

        }
    }

    [BurstCompile]
    private struct ConstrainJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public Translation targetPos;

        public float weight;
        public float constrainDistance;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            Vector3 force = Vector3.zero;
            Vector3 toTarget = trans.Value - targetPos.Value;
            if (toTarget.magnitude > constrainDistance)
            {
                force = Vector3.Normalize(toTarget) * (constrainDistance - toTarget.magnitude);
            }
            data.force += (float3)force * weight;
        }
    }

    [BurstCompile]
    private struct FleeJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public Translation targetPos;

        public float weight;
        public float fleeDist;
        public float maxSpeed;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            return;

            float3 desired = targetPos.Value - trans.Value;
            if (((Vector3)desired).magnitude <= fleeDistance)
            {
                ((Vector3)desired).Normalize();
                desired *= data.velocity - maxSpeed;
                float3 force = desired;
                float3 outForce = ((Vector3)data.force + ((Vector3)force * weight));
                data.force = outForce;

                data.fleeing = true;
            }
            else
            {
                data.fleeing = false;
            }
        }
    }

    [BurstCompile]
    private struct NeighbourJob : IJobForEachWithEntity<EnemyData, Translation>
    {

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData> cellMap;

        public int maxNeighbours;

        public float maxNeighbourDist;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans)
        {
            neighbourBuffer[entity].Clear();

            if (cellMap.TryGetFirstValue(data.cell, out NeighbourData nData, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (nData.boidIndex != data.index)
                    {
                        if (math.distance(nData.pos, trans.Value) < maxNeighbourDist)
                        {
                            neighbourBuffer[entity].Add(new PosRot
                            {
                                pos = nData.pos,
                                rot = nData.rot
                            });
                        }
                    }
                } while (cellMap.TryGetNextValue(out nData, ref iterator));
            }
        }
    }

    [BurstCompile]
    private struct CellSpacePartitionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData>.Concurrent cellMap;

        public float cellSize;
        public float gridSize;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            float3 pos = trans.Value;
            float cell = ((int)(pos.x / cellSize))
                + ((int)(pos.z / cellSize)) * gridSize;
            cellMap.Add((int)cell, new NeighbourData
            {
                boidIndex = data.index,
                pos = trans.Value,
                rot = rot.Value
            });

            data.cell = (int)cell;

        }
    }

    [BurstCompile]
    private struct MiscJob : IJob
    {
        public NativeMultiHashMap<int, NeighbourData> cellMap;
        public void Execute()
        {
            cellMap.Clear();
        }
    }
    [BurstCompile]
    private struct DestroyJob : IJob
    {
        //[ReadOnly] public EntityCommandBuffer cmdBuffer;
        public void Execute(Entity entity, int index, ref EnemyData enemyData)
        {
            //TODO re-enable this job and remove destryo logic from BoidJob
            //if (enemyData.shouldDestroy && entity != null)
            //{
            //    cmdBuffer.DestroyEntity(entity);
            //}
        }

        public void Execute()
        {
            //throw new System.NotImplementedException();
        }
    }

}
