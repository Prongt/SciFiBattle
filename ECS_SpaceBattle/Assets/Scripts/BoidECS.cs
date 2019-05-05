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

    //public NativeHashMap<int, ProjectileData> projectileHashMap;

    public int maxNeighbours = 20;
    
    
    public NativeHashMap<int, PosRot> boidPositions;
    public NativeMultiHashMap<int, PosRot> boidNeighbours;

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

    public BufferFromEntity<PosRot> posRotBuffer;
    public BufferFromEntity<Force> forceBuffer;
    protected override void OnCreateManager()
    {
        //projectileHashMap = new NativeHashMap<int, ProjectileData>(100000, Allocator.Persistent);

        boidPositions = new NativeHashMap<int, PosRot>(2500, Allocator.Persistent);
        boidNeighbours = new NativeMultiHashMap<int, PosRot>(2500 * maxNeighbours, Allocator.Persistent);
        cellMap = new NativeMultiHashMap<int, NeighbourData>(2500 * maxNeighbours, Allocator.Persistent);

        posRotBuffer = GetBufferFromEntity<PosRot>(false);

        

        Debug.Log("On Create!");
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        //projectileHashMap.Dispose();
        boidPositions.Dispose();
        boidNeighbours.Dispose();
        cellMap.Dispose();
        Debug.Log("on destroy");
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        var allign = AllignmentWeight;
        var cohesion = CohesionWeight;
        var arrive = ArriveWeight;
        var seperation = SeperationWeight;
        var weight = boidWeight;
        
        //var updatePosJob = new UpdatePostitionsJob
        //{
        //    positions = boidPositions
        //}.Schedule(this, handle);
        

        var cellJob = new CellSpacePartitionJob
        {
            cellSize = cellSize,
            gridSize = gridSize,
            cellMap = cellMap.ToConcurrent()
        }.Schedule(this, handle);
        

        var neighbourJob = new NeighbourJob
        {
            maxNeighbours = maxNeighbours,
            cellMap = cellMap,
            maxNeighbourDist = maxNeighbourDist,
            neighbourBuffer = GetBufferFromEntity<PosRot>(false)
        }.Schedule(this, cellJob);


        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = targetPos,
            weight = 10,
            stopRange = stopRange,
            slowingDist = slowingDistance,
            moveSpeed = boidMaxSpeed
            
        }.Schedule(this, neighbourJob);

        var seperationJob = new SeperationJob
        {
            maxNeighbours = maxNeighbours,
            weight = seperation,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, arriveJob);

        var cohesionJob = new CohesionJob
        {
            maxNeighbours = maxNeighbours,
            weight = cohesion,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true),
            maxSpeed = boidMaxSpeed
        }.Schedule(this, seperationJob);

        var allignJob = new AllignmentJob
        {
            weight = allign,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, cohesionJob);

        var fleeJob = new FleeJob
        {
            targetPos = targetPos,
            weight = fleeWeight,
            fleeDist = fleeDistance,
            maxSpeed = boidMaxSpeed
        }.Schedule(this, allignJob);

        var constrainJob = new ConstrainJob
        {
            weight = 10,
            targetPos = targetPos,
            constrainDistance = 300
        }.Schedule(this, fleeJob);


        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            boidDamping = boidDamping,
            boidMass = boidMass,
            boidMaxSpeed = boidMaxSpeed,
            banking = boidBanking,
            weight = weight,
            maxForce = maxForce
        }.Schedule(this, constrainJob);

        var miscJob = new MiscJob()
        {
            cellMap = cellMap
        }.Schedule(boidJob);

        //var projectileJob = new ProjectileSpawnJob
        //{
        //    cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
        //    hashMap = projectileHashMap
        //}.Schedule(this, boidJob);
        //projectileJob.Complete();

        var destroyJob = new DestroyJob()
        {
        }.Schedule(miscJob);

        //updatePosJob.Complete();
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
        //public Translation targetPos;

        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, ProjectileData> hashMap;

        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, NeighbourData> cellMap;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //Vector3 force = data.force * weight;
            //force.Normalize();
                
            
            //float3 velocity = new float3();

            float3 newAcceleration = (data.force * weight) / boidMass;
            data.acceleration = math.lerp(data.acceleration, newAcceleration, deltaTime);
            data.velocity += data.acceleration * deltaTime;



            data.velocity = Vector3.ClampMagnitude(data.velocity * weight, boidMaxSpeed);
            //var speed = ((Vector3)data.velocity).magnitude;


            if (((Vector3)data.velocity).magnitude > float.Epsilon)
            {
                //Quaternion r = rot.Value;
                //var upVec = r.eulerAngles * (float3)Vector3.up;

                //Vector3 tempUp = Vector3.Lerp(upVec, ((float3)Vector3.up * 5) + (acceleration * banking), deltaTime * 3.0f);
                //rot.Value = Quaternion.LookRotation(velocity, tempUp);


                trans.Value += data.velocity * deltaTime;
                data.velocity *= (1.0f - (boidDamping * deltaTime));
            }
            //var pos = math.lerp(trans.Value, (float3)force + trans.Value, deltaTime);
            //trans.Value += (float3)force * deltaTime;
            //trans.Value += pos;
            var r = math.slerp(rot.Value, Quaternion.LookRotation(data.velocity), deltaTime);
            //rot.Value = Quaternion.LookRotation(force);
            rot.Value = r;

            //data.acceleration = newAcceleration;
            //data.velocity = data.velocity;
            data.force = Vector3.zero;



            if (data.shouldDestroy && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
        }

        public float3 CombineForces(ref EnemyData data)
        {
            Vector3 force = Vector3.zero;
            force += (Vector3)data.arriveForce;
            if (force.magnitude >= maxForce)
            {
                force = Vector3.ClampMagnitude(force, maxForce);
                return force;
            }

            force += (Vector3)data.allignForce;
            if (force.magnitude >= maxForce)
            {
                force = Vector3.ClampMagnitude(force, maxForce);
                return force;
            }

            force += (Vector3)data.cohesionForce;
            if (force.magnitude >= maxForce)
            {
                force = Vector3.ClampMagnitude(force, maxForce);
                return force;
            }

            force += (Vector3)data.seperationForce;
            if (force.magnitude >= maxForce)
            {
                force = Vector3.ClampMagnitude(force, maxForce);
                return force;
            }

            force += (Vector3)data.fleeForce;
            if (force.magnitude >= maxForce)
            {
                force = Vector3.ClampMagnitude(force, maxForce);
                return force;
            }
            return force;
        }
    }

    [BurstCompile]
    struct SeperationJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, PosRot> neighbourMap;

        public int maxNeighbours;
        public float weight;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;

        //[NativeDisableParallelForRestriction]
        //[ReadOnly] public BufferFromEntity<Force> forceBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            return;
            var force = (float3)Vector3.zero;
            //if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            //{
            //    do
            //    {
            //        var desired = (Vector3)trans.Value - vec.pos;
            //        force += (float3)(Vector3.Normalize(desired) / desired.magnitude);
            //    } while (neighbourMap.TryGetNextValue(out vec, ref it));
            //}
            var neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                var desired = (Vector3)trans.Value - neighbours[i].pos;
                force += (float3)(Vector3.Normalize(desired) / desired.magnitude);
            }
            //float3 outForce = ((Vector3)data.force + ((Vector3)force * weight));
            Vector3 outForce = (Vector3)force * weight;
            data.force += (float3)outForce.normalized;
            //trans.Value += (float3)outForce.normalized;
        }
    }

    [BurstCompile]
    public struct CohesionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, PosRot> neighbourMap;

        public int maxNeighbours;
        public float weight;
        public float maxSpeed;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            return;
            var centerOfMass = (float3)Vector3.zero;
            
            var force = (float3)Vector3.zero;
            //if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            //{
            //    do
            //    {
            //        count++;
            //        centerOfMass += (float3)vec.pos;
            //    } while (neighbourMap.TryGetNextValue(out vec, ref it));
            //}

            var neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                centerOfMass += (float3)neighbours[i].pos;
            }

            if (neighbours.Length > 0)
            {
                centerOfMass /= neighbours.Length;
                
                var toTarget = (Vector3)(centerOfMass - trans.Value);
                var desired =  toTarget.normalized * maxSpeed;
                force = (desired - (Vector3)data.velocity).normalized;
            }



            //float3 outForce = (Vector3)force * weight;
            //data.force += outForce;
            Vector3 outForce = (Vector3)force * weight;
            data.force += (float3)outForce.normalized;
            //trans.Value += (float3)outForce.normalized;
        }
    }

    [BurstCompile]
    public struct AllignmentJob: IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, PosRot> posRot;

        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, PosRot> neighbourMap;

        public float weight;
        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            return;
            Vector3 desired = Vector3.zero;
            Vector3 force = Vector3.zero;
            

            //if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            //{
            //    do
            //    {
            //        count++;
            //        desired += vec.rot * Vector3.forward;
            //    } while (neighbourMap.TryGetNextValue(out vec, ref it));
            //}

            var neighbours = neighbourBuffer[entity].Reinterpret<PosRot>();
            for (int i = 0; i < neighbours.Length; i++)
            {
                desired += neighbours[i].rot * Vector3.forward;
                
            }

            if (neighbours.Length > 0)
            {
                Quaternion r = rot.Value;
                var vec = r.eulerAngles;
                desired /= neighbours.Length;
                force = desired - ((Vector3)(vec * (float3)Vector3.forward));
            }
            //float3 outForce = force * weight;
            //data.force += outForce;
            Vector3 outForce = (Vector3)force * weight;
            data.force += (float3)outForce.normalized;
            //trans.Value += (float3)outForce.normalized;
        }
    }

    [BurstCompile]
    private struct ArriveJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        public Translation targetPos;
        //public Rotation targetRot;
        public float weight;
        public float moveSpeed;
        public float slowingDist;
        public float stopRange;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            var dir = targetPos.Value - trans.Value;
            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;

            if (distance <= stopRange)
            {
                data.inRange = true;
                //data.shouldDestroy = true;               
                //In Range of target
            }
            //else
            //{
            var ramped = moveSpeed * (distance / slowingDist);
            //var clamped = Mathf.Min(ramped, moveSpeed);
            var clamped = math.min(ramped, moveSpeed);
            var desired = clamped * (dir / distance);
            var force = desired * deltaTime;

            //float3 outForce = (Vector3)force * weight;
            //data.force = outForce;
            Vector3 outForce = (Vector3)force * weight;
            data.force += (float3)outForce.normalized;
            //trans.Value += (float3)outForce.normalized;
            //data.inRange = false;
            //}
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
            return;
            var force = Vector3.zero;
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
        //public Rotation targetRot;
        public float weight;
        public float fleeDist;
        public float maxSpeed;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            return;

            var desired = targetPos.Value - trans.Value;
            if (((Vector3)desired).magnitude <= fleeDistance)
            {
                ((Vector3)desired).Normalize();
                desired *= data.velocity - maxSpeed;
                var force = desired;
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
    private struct UpdatePostitionsJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, PosRot> positions;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            
            //var posRot = new PosRot { pos = trans.Value, rot = rot.Value };
            //positions.TryAdd(data.index, posRot);
        }
    }

    [BurstCompile]
    private struct NeighbourJob : IJobForEachWithEntity<EnemyData, Translation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, PosRot> positions;

        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, PosRot> neighbourMap;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData> cellMap;

        public int maxNeighbours;
        //public int cellSize;
        //public int gridSize;
        public float maxNeighbourDist;

        [NativeDisableParallelForRestriction]
        [ReadOnly]public BufferFromEntity<PosRot> neighbourBuffer;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans)
        {
            neighbourBuffer[entity].Clear();
            NativeMultiHashMapIterator<int> iterator;

            if (cellMap.TryGetFirstValue(data.cell, out NeighbourData nData, out iterator))
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
    struct CellSpacePartitionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {

        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, PosRot> positions;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData>.Concurrent cellMap;

        public float cellSize;
        public float gridSize;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            var pos = trans.Value;
            var cell = ((int)(pos.x / cellSize))
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

    private struct ProjectileSpawnJob : IJobForEachWithEntity<ProjectileSpawnData, LocalToWorld, ProjectileData>
    {
        [ReadOnly] public EntityCommandBuffer cmdBuffer;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<int, ProjectileData> hashMap;
        public void Execute(Entity entity, int index, ref ProjectileSpawnData projectileSpawnData, ref LocalToWorld location, ref ProjectileData projectileData)
        {
            for (int i = 0; i < hashMap.Length; i++)
            {
                if (i >= hashMap.Length)
                {
                    continue;
                }
                var instance = cmdBuffer.Instantiate(projectileSpawnData.prefab);
                var data = hashMap[i];

                var pos = math.transform(location.Value, data.startingPos);
                cmdBuffer.SetComponent(instance, new Translation { Value = pos });

                cmdBuffer.AddComponent(instance, data);
            }
            hashMap.Clear();


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
