using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class BoidECS : JobComponentSystem
{
    private struct Target
    {
        [ReadOnly] public ComponentDataArray<TargetData> targetData;
        [ReadOnly] public EntityArray targets;
    }

    private struct Enemy
    {
        [ReadOnly] public ComponentDataArray<EnemyData> enemyData;
        [ReadOnly] public EntityArray enemies;
    }

    [Inject] private Target targets;
    [Inject] private Enemy enemies;
    [Inject] private ComponentDataFromEntity<Position> position;
    [Inject] private ComponentDataFromEntity<EnemyData> enemyData;

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        return new BoidJob()
        {
            deltaTime = Time.deltaTime,
            enemyData = enemyData,
            position = position,
            targetsArray = targets.targets,
            enemiesArray = enemies.enemies,

        }.Schedule(handle);
    }

    public static void ScheduleDestroyJob(Entity entityToDestroy)
    {
        var job = new DestroyJob()
        {
            cmdBuffer = Bootstrap.entityCommandBuffer,
            entityToDestroy = entityToDestroy
        }.Schedule();
        job.Complete();
    }


    [BurstCompile]
    private struct BoidJob : IJob
    {
        public float deltaTime;
        public ComponentDataFromEntity<EnemyData> enemyData;
        public ComponentDataFromEntity<Position> position;
        [ReadOnly] public EntityArray targetsArray;
        [ReadOnly] public EntityArray enemiesArray;

        private float moveSpeed;
        private float slowingDist;

        public void Execute()
        {
            moveSpeed = enemyData[enemiesArray[0]].movementSpeed;
            slowingDist = enemyData[enemiesArray[0]].slowingDistance;
            for (int i = 0; i < enemiesArray.Length; i++)
            {
                for (int j = 0; j < targetsArray.Length; j++)
                {
                    Arrive(i, j);
                }
            }
        }

        void Arrive(int enemyIndex, int targetIndex)
        {
            var dir = position[targetsArray[targetIndex]].Value - position[enemiesArray[enemyIndex]].Value;
            //var moveSpeed = enemyData[enemiesArray[enemyIndex]].movementSpeed;
            //var slowingDist = enemyData[enemiesArray[enemyIndex]].slowingDistance;

            float distance = new Vector3(dir.x, dir.y, dir.z).magnitude;
            if (distance < 0.1f)
            {
                //BoidECS.ScheduleDestroyJob(enemiesArray[i]);
            }
            else
            {
                float ramped = moveSpeed * (distance / slowingDist);
                float clamped = Mathf.Min(ramped, moveSpeed);
                var desired = clamped * (dir / distance);

                var enemyPos = position[enemiesArray[enemyIndex]];
                enemyPos.Value += desired * deltaTime;
                position[enemiesArray[enemyIndex]] = enemyPos;
            }
        }

        void Allignment(int enemyIndex)
        {
            Vector3 pos;
             int neighbourCount = 0;

             for (int i = 0; i < enemiesArray.Length; i++)
             {
                 if (i == enemyIndex)
                 {
                     continue;
                }
                 else
                 {
                     if (Vector3.Distance(position[enemiesArray[i]].Value, position[enemiesArray[enemyIndex]].Value) < enemyData[enemiesArray[enemyIndex]].minNeighbourDist)
                     {

                         neighbourCount++;
                     }
                 }
             }
        }
    }

    
    private struct DestroyJob : IJob
    {
        [WriteOnly] public EntityCommandBuffer cmdBuffer;
        public Entity entityToDestroy;
        public void Execute()
        {
            cmdBuffer.DestroyEntity(entityToDestroy);
        }
    }
}

