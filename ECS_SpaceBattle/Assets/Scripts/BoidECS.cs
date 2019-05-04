﻿using Unity.Burst;
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

    public int maxNeighbours = 10;
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


    protected override void OnCreateManager()
    {
        //projectileHashMap = new NativeHashMap<int, ProjectileData>(100000, Allocator.Persistent);

        boidPositions = new NativeHashMap<int, PosRot>(2500, Allocator.Persistent);
        boidNeighbours = new NativeMultiHashMap<int, PosRot>(2500 * maxNeighbours, Allocator.Persistent);
        cellMap = new NativeMultiHashMap<int, NeighbourData>(2500 * maxNeighbours, Allocator.Persistent);

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
            cellMap = cellMap,
            positions = boidPositions
        }.Schedule(updatePosJob);
        

        var neighbourJob = new NeighbourJob
        {
            positions = boidPositions,
            neighbourMap = boidNeighbours,
            maxNeighbours = maxNeighbours,
            cellMap = cellMap,
            cellSize = cellSize,
            gridSize = gridSize,
            maxNeighbourDist = maxNeighbourDist
        }.Schedule(cellJob);


        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos },
            targetRot = new Rotation(),
            weight = ArriveWeight
        }.Schedule(this, neighbourJob);

        var seperationJob = new SeperationJob
        {
            maxNeighbours = maxNeighbours,
            neighbourMap = boidNeighbours,
            weight = SeperationWeight
        }.Schedule(this, arriveJob);

        var cohesionJob = new CohesionJob
        {
            maxNeighbours = maxNeighbours,
            neighbourMap = boidNeighbours,
            weight = CohesionWeight
        }.Schedule(this, seperationJob);

        var allignJob = new AllignmentJob
        {
            neighbourMap = boidNeighbours,
            weight = AllignmentWeight
        }.Schedule(this, cohesionJob);

        var fleeJob = new FleeJob
        {
            targetPos = new Translation() { Value = targetPos },
            targetRot = new Rotation()
        }.Schedule(this, allignJob);
        

        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos }
            //hashMap = boidNeighbours
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
        public Translation targetPos;

        //[NativeDisableParallelForRestriction]
        //public NativeHashMap<int, ProjectileData> hashMap;

        //[NativeDisableParallelForRestriction]
        //public NativeMultiHashMap<int, Vector3> hashMap;

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

            //var temp = enemyData.force / enemyData.mass;
            //enemyData.acceleration = Vector3.Lerp(enemyData.acceleration, temp, deltaTime);
            //enemyData.velocity += enemyData.acceleration * deltaTime;

            //enemyData.velocity = Vector3.ClampMagnitude(enemyData.velocity, enemyData.maxSpeed);
            //var speed = ((Vector3)enemyData.velocity).magnitude;

            //if (speed > 0)
            //{
            //    enemyData.velocity *= (1.0f - (enemyData.damping * deltaTime));
            //}



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

        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            var force = (float3)Vector3.zero;
            if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            {
                do
                {
                    var desired = (Vector3)trans.Value - vec.pos;
                    force += (float3)(Vector3.Normalize(desired) / desired.magnitude);
                } while (neighbourMap.TryGetNextValue(out vec, ref it));
            }
            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight)).normalized;
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
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation c2)
        {
            var centerOfMass = (float3)Vector3.zero;
            

            var count = 0;
            var force = (float3)Vector3.zero;
            if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            {
                do
                {
                    count++;
                    centerOfMass += (float3)vec.pos;
                } while (neighbourMap.TryGetNextValue(out vec, ref it));
            }

            if (count > 0)
            {
                centerOfMass /= count;
                
                var toTarget = (Vector3)(centerOfMass - trans.Value);
                var desired =  toTarget.normalized * data.maxSpeed;
                force = (desired - (Vector3)data.velocity).normalized;
            }
            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight)).normalized;
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
        public void Execute(Entity entity, int index, ref EnemyData data, ref Translation trans, ref Rotation rot)
        {
            Vector3 desired = Vector3.zero;
            Vector3 force = Vector3.zero;
            var count = 0;

            if (neighbourMap.TryGetFirstValue(data.index, out PosRot vec, out NativeMultiHashMapIterator<int> it))
            {
                do
                {
                    count++;
                    desired += vec.rot * Vector3.forward;
                } while (neighbourMap.TryGetNextValue(out vec, ref it));
            }

            if (count > 0)
            {
                var r = Quaternion.Euler(rot.Value.value.x, rot.Value.value.y, rot.Value.value.z);
                desired /= count;
                force = desired - (r * Vector3.forward);
            }
            float3 outForce = ((Vector3)data.force + ((Vector3)force * weight)).normalized;
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

            if (distance <= data.attackRange)
            {
                //data.inRange = true;
                //data.shouldDestroy = true;               
                //In Range of target
            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);
                var force = desired * deltaTime;

                float3 outForce = ((Vector3)data.force + ((Vector3)force * weight)).normalized;
                data.force = outForce;
                data.inRange = false;
            }
        }
    }

    [BurstCompile]
    private struct FleeJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public Translation targetPos;
        public Rotation targetRot;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            return;

            var desired = targetPos.Value - trans.Value;
            if (((Vector3)desired).magnitude <= enemyData.fleeDistance)
            {
                ((Vector3)desired).Normalize();
                desired *= enemyData.maxSpeed;
                enemyData.force += enemyData.velocity - desired;
                enemyData.fleeing = true;
            }
            else
            {
                enemyData.fleeing = false;
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
    private struct NeighbourJob : IJob
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
        */
        #endregion
        public void Execute()
        {
            //var cmap = cellMap;
            //var nmap = neighbourMap;
            //nmap.Clear();
            neighbourMap.Clear();

            int count;

            for (int i = 0; i < positions.Length; i++)
            {
                count = 0;
                if (positions.TryGetValue(i, out PosRot p))
                {
                    var cell = ((int)(p.pos.x / cellSize))
                    + ((int)(p.pos.z / cellSize)) * gridSize;

                    NativeMultiHashMapIterator<int> it;
                    if (cellMap.TryGetFirstValue(cell, out NeighbourData neighbourData, out it))
                    {
                        do
                        {
                            //TODO breakout of loop when max neighbours is reached
                            if (neighbourData.boidIndex != i && count < maxNeighbours)
                            {
                                var pRot = new PosRot { pos = neighbourData.pos, rot = neighbourData.rot };
                                neighbourMap.Add(i, pRot);
                                count++;
                            }
                        } while (cellMap.TryGetNextValue(out neighbourData, ref it));
                    }
                }
            }
            //neighbourMap = nmap;
        }
    }

    [BurstCompile]
    struct CellSpacePartitionJob : IJob
    {

        //[NativeDisableParallelForRestriction]
        public NativeHashMap<int, PosRot> positions;

        public NativeMultiHashMap<int, NeighbourData> cellMap;

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
        */
        #endregion
        public void Execute()
        {
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
