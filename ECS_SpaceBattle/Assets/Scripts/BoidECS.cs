using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

public class BoidECS : JobComponentSystem
{
    public static Vector3 targetPos;

    public NativeHashMap<int, ProjectileData> projectileHashMap;

    public int maxNeighbours = 10;
    public NativeArray<Vector3> boidPositions;
    public NativeMultiHashMap<int, Vector3> boidNeighbours;

    protected override void OnCreateManager()
    {
        projectileHashMap = new NativeHashMap<int, ProjectileData>(100000, Allocator.Persistent);

        boidPositions = new NativeArray<Vector3>(5000, Allocator.Persistent);
        boidNeighbours = new NativeMultiHashMap<int, Vector3>(5000 * maxNeighbours, Allocator.Persistent);

        Debug.Log("On Create!");
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        projectileHashMap.Dispose();
        boidPositions.Dispose();
        boidNeighbours.Dispose();
        Debug.Log("on destroy");
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        var neighbourJob = new NeighbourJob
        {
            positions = boidPositions,
            hashMap = boidNeighbours,
            maxNeighbours = maxNeighbours
        }.Schedule(this,handle);
        neighbourJob.Complete();

        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos},
            targetRot = new Rotation()
        }.Schedule(this, neighbourJob);
        arriveJob.Complete();

        var fleeJob = new FleeJob
        {
            targetPos = new Translation() { Value = targetPos },
            targetRot = new Rotation()
        }.Schedule(this, arriveJob);
        fleeJob.Complete();

        var boidJob = new BoidJob()
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos },
            hashMap = boidNeighbours
        }.Schedule(this, fleeJob);
        boidJob.Complete();


        //var projectileJob = new ProjectileSpawnJob
        //{
        //    cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
        //    hashMap = projectileHashMap
        //}.Schedule(this, boidJob);
        //projectileJob.Complete();

        var destroyJob = new DestroyJob()
        {
        }.Schedule(this, boidJob);
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

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, Vector3> hashMap;

        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            //NativeMultiHashMapIterator<int> it;
            //if (hashMap.TryGetFirstValue(2310, out Vector3 neighbourPos, out it))
            //{
            //    do
            //    {
            //        Debug.Log("Yes");
            //    } while (hashMap.TryGetNextValue(out neighbourPos, ref it));
            //}
            //else
            //{
            //    Debug.Log("Erasdasror");
            //}

            if (!enemyData.inRange)
            {
                trans.Value += enemyData.force;
                rot.Value = Quaternion.LookRotation(enemyData.force);
                enemyData.force = Vector3.zero;
            }
            else
            {
                //hashMap.TryAdd(hashMap.Length , new ProjectileData
                //{
                //    speed = 2.0f,
                //    target = enemyData.force,
                //    startingPos = trans.Value
                //});

                #region old
                //Spawn projectiles every few seconds 
                //Send projectiles towards target
                //When in range create explosion effect and damage target

                //ProjectileSpawner.projectilesList.Add(new ProjectileData
                //{
                //    speed = 1.0f,
                //    startingPos = trans.Value,
                //    target = new Translation().Value
                //});

                //var bullet = cmdBuffer.CreateEntity(projectileArchtype);
                //cmdBuffer.SetComponent(entity, new Translation { Value = trans.Value });
                //cmdBuffer.SetComponent(entity, new Rotation { Value = rot.Value });
                //cmdBuffer.AddComponent(entity, new ProjectileData
                //{
                //    speed = 1.0f,
                //    //startingPos = data[i].startingPos,
                //    target = new Translation().Value
                //});

                //cmdBuffer.AddSharedComponent(entity, renderMesh);
                //ComponentData.Spawn(trans.Value, rot.Value);

                //var instance = cmdBuffer.Instantiate(projectileEntity);


                //var instance = cmdBuffer.Instantiate(entity);
                ////var position = trans.Value;

                //cmdBuffer.SetComponent(instance, new Translation { Value = trans.Value });

                //cmdBuffer.SetComponent(instance, new Scale { Value = 5 });

                //enemyData.shouldDestroy = false;
                //enemyData.inRange = false;
                //cmdBuffer.AddComponent(instance, enemyData);
                #endregion
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



            if (enemyData.shouldDestroy && entity != null)
            {
                cmdBuffer.DestroyEntity(entity);
            }
        }
    }

    [BurstCompile]
    private struct ArriveJob : IJobForEachWithEntity<EnemyData, Translation, Rotation>
    {
        public float deltaTime;
        public Translation targetPos;
        public Rotation targetRot;
        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            var moveSpeed = enemyData.movementSpeed;
            var slowingDist = enemyData.slowingDistance;
            var dir = targetPos.Value - trans.Value;
            var distance = new Vector3(dir.x, dir.y, dir.z).magnitude;

            if (distance <= enemyData.attackRange)
            {
                enemyData.inRange = true;
                //enemyData.shouldDestroy = true;               
                //In Range of target
            }
            else
            {
                var ramped = moveSpeed * (distance / slowingDist);
                var clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);

                enemyData.force += desired * deltaTime;
                enemyData.inRange = false;
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
    private struct NeighbourJob : IJobForEachWithEntity<EnemyData, Translation>
    {
        //[NativeDisableParallelForRestriction]
        public NativeArray<Vector3> positions;

        [NativeDisableParallelForRestriction]
        public NativeMultiHashMap<int, Vector3> hashMap;

        public int maxNeighbours;
        public void Execute(Entity entity, int jobIndex, ref EnemyData data, ref Translation trans)
        {
          
            positions[data.index] = trans.Value;
            var maxDist = data.maxNeighbourDist;
            var neighbourCount = 0;
            var boidPos = trans.Value;
            NativeMultiHashMapIterator<int> it;
            var index = data.index;

            if (hashMap.TryGetFirstValue(index, out Vector3 neighbourPos, out it))
            {
                do
                {
                    if (Vector3.Distance(boidPos, neighbourPos) < maxDist)
                    {
                        neighbourCount++;
                    }
                    else
                    {
                        hashMap.TryRemove(index, neighbourPos);
                    }
                } while (hashMap.TryGetNextValue(out neighbourPos, ref it));
            }

            while (neighbourCount > maxNeighbours)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    if (positions[i] != null && i != index)
                    {
                        if (Vector3.Distance(boidPos, positions[i]) < maxDist)
                        {
                            neighbourCount++;
                            hashMap.Add(index, positions[i]);
                        }
                    }
                }
            }
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
    private struct DestroyJob : IJobForEachWithEntity<EnemyData>
    {
        //[ReadOnly] public EntityCommandBuffer cmdBuffer;
        public void Execute(Entity entity, int index, ref EnemyData enemyData)
        {
            //if (enemyData.shouldDestroy && entity != null)
            //{
            //    cmdBuffer.DestroyEntity(entity);
            //}
        }
    }
}