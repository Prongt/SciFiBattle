using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    public static Vector3 targetPos;

    //public NativeHashMap<int, ProjectileData> projectileHashMap;

    public int maxNeighbours = 20;
    public int cellSize = 10;
    public int gridSize = 2000;
    public int maxNeighbourDist = 10;
    public NativeHashMap<int, PosRot> boidPositions;
    public NativeMultiHashMap<int, PosRot> boidNeighbours;

    public NativeMultiHashMap<int, NeighbourData> cellMap;

    public static float ArriveWeight = 1;
    public static float AllignmentWeight = 1;
    public static float SeperationWeight = 1;
    public static float CohesionWeight = 1;
    public static float fleeWeight = 1;
    public static float fleeDistance = 1;

    public BufferFromEntity<PosRot> buffer;
    protected override void OnCreateManager()
    {
        //projectileHashMap = new NativeHashMap<int, ProjectileData>(100000, Allocator.Persistent);

        boidPositions = new NativeHashMap<int, PosRot>(2500, Allocator.Persistent);
        boidNeighbours = new NativeMultiHashMap<int, PosRot>(2500 * maxNeighbours, Allocator.Persistent);
        cellMap = new NativeMultiHashMap<int, NeighbourData>(2500 * maxNeighbours, Allocator.Persistent);

        buffer = GetBufferFromEntity<PosRot>(false);

        

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

        
        var updatePosJob = new UpdatePostitionsJob
        {
            positions = boidPositions
        }.Schedule(this, handle);
        

        var cellJob = new CellSpacePartitionJob
        {
            cellSize = cellSize,
            gridSize = gridSize,
            cellMap = cellMap.ToConcurrent(),
            positions = boidPositions
        }.Schedule(this, updatePosJob);
        

        var neighbourJob = new NeighbourJob
        {
            positions = boidPositions,
            neighbourMap = boidNeighbours,
            maxNeighbours = maxNeighbours,
            cellMap = cellMap,
            cellSize = cellSize,
            gridSize = gridSize,
            maxNeighbourDist = maxNeighbourDist,
            neighbourBuffer = GetBufferFromEntity<PosRot>(false)
        }.Schedule(this, cellJob);


        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos },
            weight = ArriveWeight
            
        }.Schedule(this, neighbourJob);

        var seperationJob = new SeperationJob
        {
            maxNeighbours = maxNeighbours,
            neighbourMap = boidNeighbours,
            weight = SeperationWeight,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, arriveJob);

        var cohesionJob = new CohesionJob
        {
            maxNeighbours = maxNeighbours,
            neighbourMap = boidNeighbours,
            weight = CohesionWeight,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, seperationJob);

        var allignJob = new AllignmentJob
        {
            neighbourMap = boidNeighbours,
            weight = AllignmentWeight,
            neighbourBuffer = GetBufferFromEntity<PosRot>(true)
        }.Schedule(this, cohesionJob);

        var fleeJob = new FleeJob
        {
            targetPos = new Translation() { Value = targetPos },
            weight = fleeWeight,
            fleeDist = fleeDistance
        }.Schedule(this, allignJob);


        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            cellMap = cellMap
        }.Schedule(this, fleeJob);
        


        //var projectileJob = new ProjectileSpawnJob
        //{
        //    cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
        //    hashMap = projectileHashMap
        //}.Schedule(this, boidJob);
        //projectileJob.Complete();

        var destroyJob = new DestroyJob()
        {
        }.Schedule(boidJob);

        updatePosJob.Complete();
        cellJob.Complete();
        neighbourJob.Complete();
        arriveJob.Complete();
        seperationJob.Complete();
        cohesionJob.Complete();
        allignJob.Complete();
        fleeJob.Complete();
        boidJob.Complete();
        destroyJob.Complete();

        
        return destroyJob;
    }

    [BurstCompile]
    private struct BoidJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [ReadOnly] public EntityCommandBuffer cmdBuffer;
        public float deltaTime;
        //public Translation targetPos;

        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, ProjectileData> hashMap;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData> cellMap;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            if (!data.inRange)
            {
                Vector3 force = data.force;
                force.Normalize();
                data.force = force;
                
                trans.Value += data.force;
                rot.Value = Quaternion.LookRotation(data.force);
                data.force = Vector3.zero;
            }
            else
            {
                //hashMap.TryAdd(hashMap.Length , new ProjectileData
                //{
                //    speed = 2.0f,
                //    target = enemyData.force,
                //    startingPos = trans.Value
                //});
            }

            var temp = data.force / data.mass;
            data.acceleration = Vector3.Lerp(data.acceleration, temp, deltaTime);
            data.velocity += data.acceleration * deltaTime;

            data.velocity = Vector3.ClampMagnitude(data.velocity, data.maxSpeed);
            var speed = ((Vector3)data.velocity).magnitude;

            if (speed > 0)
            {
                data.velocity *= (1.0f - (data.damping * deltaTime));
            }

            cellMap.Clear();


            if (data.shouldDestroy && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
            //hashMap.Clear();
        }
    }

    [BurstCompile]
    struct SeperationJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, PosRot> neighbourMap;

        public int maxNeighbours;
        public float weight;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
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
            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight));
            data.force = outForce;
        }
    }

    [BurstCompile]
    public struct CohesionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, PosRot> neighbourMap;

        public int maxNeighbours;
        public float weight;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            var centerOfMass = (float3)Vector3.zero;
            

            var count = 0;
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
                count++;
            }

            if (count > 0)
            {
                centerOfMass /= count;
                
                var toTarget = (Vector3)(centerOfMass - trans.Value);
                var desired =  toTarget.normalized * data.maxSpeed;
                force = (desired - (Vector3)data.velocity).normalized;
            }



            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight));
            data.force = outForce;
        }
    }

    [BurstCompile]
    public struct AllignmentJob: IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, PosRot> posRot;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, PosRot> neighbourMap;

        public float weight;
        [NativeDisableParallelForRestriction]
        [ReadOnly] public BufferFromEntity<PosRot> neighbourBuffer;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            Vector3 desired = Vector3.zero;
            Vector3 force = Vector3.zero;
            var count = 0;

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
                count++;
            }

            if (count > 0)
            {
                var r = Quaternion.Euler(rot.Value.value.x, rot.Value.value.y, rot.Value.value.z);
                desired /= count;
                force = desired - (r * Vector3.forward);
            }
            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight));
            data.force = outForce;
        }
    }

    [BurstCompile]
    private struct ArriveJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        public Translation targetPos;
        public Rotation targetRot;
        public float weight;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            var moveSpeed = data.movementSpeed;
            var slowingDist = data.slowingDistance;
            var dir = targetPos.Value - trans.Value;
            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;

            //if (distance <= data.attackRange)
            //{
            //    data.inRange = true;
            //    //data.shouldDestroy = true;               
            //    //In Range of target
            //}
            //else
            //{
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);
                var force = desired * deltaTime;

                float3 outForce = (((Vector3)data.force + ((Vector3)force) * weight));
                data.force = outForce;
                data.inRange = false;
            //}
        }
    }

    [BurstCompile]
    private struct FleeJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public Translation targetPos;
        public Rotation targetRot;
        public float weight;
        public float fleeDist;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            return;

            var desired = targetPos.Value - trans.Value;
            if (((Vector3)desired).magnitude <= fleeDistance)
            {
                ((Vector3)desired).Normalize();
                desired *= data.velocity - data.maxSpeed;
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
        [NativeDisableParallelForRestriction]
        public NativeHashMap<int, PosRot> positions;
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //positions[data.index] = trans.Value;
            var posRot = new PosRot { pos = trans.Value, rot = rot.Value };
            positions.TryAdd(data.index, posRot);
        }
    }

    [BurstCompile]
    private struct NeighbourJob : IJobForEachWithEntity<EnemyData, Translation>
    {
        [NativeDisableParallelForRestriction]
        public NativeHashMap<int, PosRot> positions;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, PosRot> neighbourMap;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData> cellMap;

        public int maxNeighbours;
        public int cellSize;
        public int gridSize;
        public float maxNeighbourDist;

        [NativeDisableParallelForRestriction]
        [ReadOnly]public BufferFromEntity<PosRot> neighbourBuffer;

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans)
        {
            //var count = 0;
            //if (positions.TryGetValue(data.index, out PosRot pRot))
            //{
            //    for (int i = 0; i < positions.Length; i++)
            //    {
            //        if (count < maxNeighbours)
            //        {
            //            if (Vector3.Distance(trans.Value, pRot.pos) < maxNeighbourDist)
            //            {
            //                neighbourMap.Add(data.index, pRot);
            //                count++;
            //            }
            //        }
            //        else
            //        {
            //            continue;
            //        }  
            //    }
            //}

            //var count = 0;
            neighbourBuffer[entity].Clear();
            
            

            NativeMultiHashMapIterator<int> iterator;
            if (cellMap.TryGetFirstValue(data.cell, out NeighbourData nData, out iterator))
            {
                do
                {
                    //neighbourMap.Add(data.index, new PosRot
                    //{
                    //    pos = nData.pos,
                    //    rot = nData.rot
                    //});

                    if (nData.boidIndex != data.index)
                    {
                        neighbourBuffer[entity].Add(new PosRot
                        {
                            pos = nData.pos,
                            rot = nData.rot
                        });
                    }
                    
                    
                } while (cellMap.TryGetNextValue(out nData, ref iterator));
            }
        }

        #region old 
        /*
        public void Execute(Entity entity, int jobIndex, ref EnemyData data, ref Translation trans)
        {
            var maxDist = data.maxNeighbourDist;
            var neighbourCount = 0;
            var boidPos = trans.Value;
            NativeMultiHashMapIterator<int> it;
            var index = data.index;

            //neighbourMap

            //if (hashMap.TryGetFirstValue(index, out Vector3 neighbourPos, out it))
            //{
            //    do
            //    {
            //        //Debug.Log("Success");
            //        if (Vector3.Distance(boidPos, neighbourPos) < maxDist)
            //        {
            //            neighbourCount++;

            //        }
            //        else
            //        {
            //            hashMap.TryRemove(index, neighbourPos);
            //        }
            //    } while (hashMap.TryGetNextValue(out neighbourPos, ref it));
            //}

            NativeMultiHashMapIterator<int> itter;

            //if (cellMap.TryGetFirstValue(data.cell, out NeighbourData info, out itter))
            //{
            //    do
            //    {
            //        if (Vector3.Distance(boidPos, info.pos) < maxDist && info.boidIndex != index && neighbourCount < maxNeighbours)
            //        {
            //            neighbourCount++;
            //            hashMap.Add(index, info.pos);

            //        }
            //    } while (cellMap.TryGetNextValue(out info, ref itter));
            //}

            //cellMap.Clear();
            //Vector3 v;

            //for (int i = 0; i < positions.Length; i++)
            //{
            //    if (neighbourCount > maxNeighbours)
            //    {
            //        break;
            //    }

            //    if (positions.TryGetValue(i, out v))
            //    {
            //        if (Vector3.Distance(boidPos, v) < maxDist)
            //        {
            //            neighbourCount++;
            //            hashMap.Add(index, v);

            //        }
            //    }
            //}
        }
        
            
        public void Execute()
        {
            //var cmap = cellMap;
            //var nmap = neighbourMap;
            //nmap.Clear();
            neighbourMap.Clear();
            //positions.ToConcurrent();
            //int count = 0;

            //for (int i = 0; i < positions.Length; i++)
            //{
            //    if (positions.TryGetValue(i, out PosRot boid))
            //    {
                    
            //        for (int j = 0; j < positions.Length; j++)
            //        {
            //            if (count > maxNeighbours)
            //            {
            //                break;
            //            }
            //            if (i == j)
            //            {
            //                continue;
            //            }
            //            if (positions.TryGetValue(i, out PosRot other))
            //            {
            //                if (Vector3.Distance(boid.pos, other.pos) < maxNeighbourDist)
            //                {
            //                    neighbourMap.Add(i, other);
            //                    count++;
            //                }
            //            }
            //        }
            //    }

                
            //}

            //for (int i = 0; i < positions.Length; i++)
            //{
            //    count = 0;
            //    if (positions.TryGetValue(i, out PosRot p))
            //    {
            //        for (int j = 0; j < positions.Length; j++)
            //        {
            //            if(i != j)
            //            {
            //                if (positions.TryGetValue(j, out PosRot other))
            //                {

            //                }
            //        }
            //    }
            //}

                    //for (int i = 0; i < positions.Length; i++)
                    //{
                    //    count = 0;
                    //    if (positions.TryGetValue(i, out PosRot p))
                    //    {
                    //        var cell = ((int)(p.pos.x / cellSize))
                    //        + ((int)(p.pos.z / cellSize)) * gridSize;

                    //        NativeMultiHashMapIterator<int> it;
                    //        if (cellMap.TryGetFirstValue(cell, out NeighbourData neighbourData, out it))
                    //        {
                    //            do
                    //            {
                    //                //TODO breakout of loop when max neighbours is reached
                    //                if (neighbourData.boidIndex != i && count < maxNeighbours)
                    //                {
                    //                    var pRot = new PosRot { pos = neighbourData.pos, rot = neighbourData.rot };
                    //                    neighbourMap.Add(i, pRot);
                    //                    count++;
                    //                }
                    //            } while (cellMap.TryGetNextValue(out neighbourData, ref it));
                    //        }
                    //    }
                    //}

                }
        
        //public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        //{
        //    var me = positions[data.index];
        //    for (int i = 0; i < positions.Length; i++)
        //    {
        //        if (i == data.index)
        //        {
        //            continue;
        //        }
        //        var other = positions[i];
        //        var count = 0;
                
        //        if (Vector3.Distance(trans.Value, other.pos) < maxNeighbourDist && count < maxNeighbours)
        //        {
        //            neighbourMap.Add(data.index, other);
        //            count++;
        //        }
                
        //    }
        //}
        */
        #endregion
    }

    [BurstCompile]
    struct CellSpacePartitionJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {

        [NativeDisableParallelForRestriction]
        public NativeHashMap<int, PosRot> positions;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, NeighbourData>.Concurrent cellMap;

        public int cellSize;
        public int gridSize;

        #region Old
        /*
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans)
        {
            // TODO Remove previous entry

            var cell = ((int)(trans.Value.x / cellSize))
                + ((int)(trans.Value.z / cellSize)) * gridSize;

            var a = new NativeList<NeighbourData>(cellMap.Length, Allocator.Temp);

            if (data.cell != cell)
            {
                NativeMultiHashMapIterator<int> it;
                if (cellMap.TryGetFirstValue(data.cell, out NeighbourData d, out it))
                {
                    do
                    {
                        if (d.boidIndex == data.index)
                        {
                            cellMap.SetValue(
                                new NeighbourData
                                {
                                    boidIndex = -10,
                                    pos = trans.Value
                                }
                                ,it);
                        }
                        else
                        {
                            a.Add(d);
                        }
                    } while (cellMap.TryGetNextValue(out d, ref it));
                }
            }
            else
            {
                var nd = new NeighbourData
                {
                    boidIndex = data.index,
                    pos = trans.Value
                };
                NativeMultiHashMapIterator<int> it;
                if (cellMap.TryGetFirstValue(cell, out NeighbourData d, out it))
                {
                    do
                    {
                        if (d.boidIndex == -10)
                        {
                            cellMap.SetValue(
                                nd,
                                it
                                );
                        }
                        a.Add(d);
                    } while (cellMap.TryGetNextValue(out d, ref it));
                }
                else
                {
                    cellMap.Add(cell, nd);
                    a.Add(nd);
                }
            }
            data.cell = cell;
        }
        
        public void Execute()
        {
            return;
            //TODO remove old entries instead of clearing entire hashmap
            var cmap = cellMap;
            cmap.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions.TryGetValue(i, out PosRot p))
                {
                    var cell = ((int)(p.pos.x / cellSize))
                + ((int)(p.pos.z / cellSize)) * gridSize;

                    cmap.Add(cell, new NeighbourData
                    {
                        boidIndex = i,
                        pos = positions[i].pos,
                        rot = positions[i].rot
                    });
                }
            }
            cellMap = cmap;
        }
        */
        #endregion
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            //return;
            var pos = trans.Value;
            var cell = ((int)(pos.x / cellSize))
                + ((int)(pos.z / cellSize)) * gridSize;
            cellMap.Add(cell, new NeighbourData
            {
                boidIndex = data.index,
                pos = trans.Value,
                rot = rot.Value
            });
            
            data.cell = cell;

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
