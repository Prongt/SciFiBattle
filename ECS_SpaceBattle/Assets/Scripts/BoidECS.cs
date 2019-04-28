﻿using Unity.Burst;
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
    //public static Entity projectileEntity;
    public NativeHashMap<Vector3, ProjectileData> projectileHashMap;
    //public static Rotation 

    protected override void OnCreateManager()
    {
        projectileHashMap = new NativeHashMap<Vector3, ProjectileData>(1000, Allocator.Persistent);
        Debug.Log("On Create!");
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();
        projectileHashMap.Dispose();
        Debug.Log("on destroy");
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        var arriveJob = new ArriveJob
        {
            deltaTime = Time.deltaTime,
            targetPos = new Translation() { Value = targetPos},
            targetRot = new Rotation()
        }.Schedule(this, handle);
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
            hashMap = projectileHashMap
        }.Schedule(this, fleeJob);
        boidJob.Complete();


        var projectileJob = new ProjectileSpawnJob
        {
            cmdBuffer = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            hashMap = projectileHashMap
        }.Schedule(this, boidJob);
        projectileJob.Complete();

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
        //public Entity projectileToSpawn;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<Vector3, ProjectileData> hashMap;

        public void Execute(Entity entity, int index, ref EnemyData enemyData, ref Translation trans, ref Rotation rot)
        {
            //hashMap.Dispose();
            if (!enemyData.inRange)
            {
                trans.Value += enemyData.force;
                rot.Value = Quaternion.LookRotation(enemyData.force);
                enemyData.force = Vector3.zero;
            }
            else
            {
                hashMap.TryAdd(trans.Value, new ProjectileData
                {
                    speed = 2.0f,
                    target = targetPos.Value
                });

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

    private struct ProjectileSpawnJob : IJobForEachWithEntity<ProjectileSpawnData, LocalToWorld, ProjectileData>
    {
        [ReadOnly] public EntityCommandBuffer cmdBuffer;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<Vector3, ProjectileData> hashMap;
        public void Execute(Entity entity, int index, ref ProjectileSpawnData projectileSpawnData, ref LocalToWorld location, ref ProjectileData projectileData)
        {
            var keyArray = hashMap.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < keyArray.Length; i++)
            {
                var instance = cmdBuffer.Instantiate(projectileSpawnData.prefab);
                var position = keyArray[i];

                cmdBuffer.SetComponent(instance, new Translation { Value = position });

                cmdBuffer.AddComponent(instance, hashMap[position]);
                hashMap.Remove(keyArray[i]);
            }
            //hashMap.Dispose();

            //for (int i = 0; i < projectileHashMap.Length; i++)
            //{
            //    var instance = cmdBuffer.Instantiate(projectileSpawnData.prefab);
            //    var position = projectileHashMap.

            //    cmdBuffer.SetComponent(instance, new Translation { Value = position });

            //    enemyData.shouldDestroy = false;
            //    enemyData.inRange = false;
            //    cmdBuffer.AddComponent(instance, enemyData);
            //}

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